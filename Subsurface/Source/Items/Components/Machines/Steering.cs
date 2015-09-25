﻿using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Subsurface.Items.Components
{
    class Steering : Powered
    {
        private Vector2 currVelocity;
        private Vector2 targetVelocity;

        private bool autoPilot;

        private SteeringPath steeringPath;

        private static PathFinder pathFinder;

        private float networkUpdateTimer;
        private bool valueChanged;

        bool AutoPilot
        {
            get { return autoPilot; }
            set
            {
                if (value == autoPilot) return;

                autoPilot = value;

                if (autoPilot)
                {
                    if (pathFinder==null) pathFinder = new PathFinder(WayPoint.WayPointList, false);
                    steeringPath = pathFinder.FindPath(
                        ConvertUnits.ToSimUnits(Level.Loaded.Position),
                        ConvertUnits.ToSimUnits(Level.Loaded.EndPosition));
                }
            }
        }

        private Vector2 TargetVelocity
        {
            get { return targetVelocity;}
            set 
            {
                if (!MathUtils.IsValid(value)) return;
                targetVelocity.X = MathHelper.Clamp(value.X, -100.0f, 100.0f);
                targetVelocity.Y = MathHelper.Clamp(value.Y, -100.0f, 100.0f);
            }
        }

        public Vector2 CurrTargetVelocity
        {
            get { return targetVelocity; }
        }

        public Steering(Item item, XElement element)
            : base(item, element)
        {
            isActive = true;
        }
        
        public override void Update(float deltaTime, Camera cam)
        {
            //base.Update(deltaTime, cam);

            //if (voltage < minVoltage) return;
                     
            if (autoPilot)
            {
                //if (steeringPath==null)
                //{
                //    PathFinder pathFinder = new PathFinder(WayPoint.WayPointList, false);
                //    steeringPath = pathFinder.FindPath(
                //        ConvertUnits.ToSimUnits(Level.Loaded.StartPosition),
                //        ConvertUnits.ToSimUnits(Level.Loaded.EndPosition));
                //}

                steeringPath.GetNode(Vector2.Zero, 20.0f);

                if (steeringPath.CurrentNode!=null)
                {
                    float prediction = 10.0f;

                    Vector2 futurePosition = Submarine.Loaded.Speed * prediction;

                    Vector2 targetSpeed = (steeringPath.CurrentNode.Position - futurePosition);

                    //float dist = targetSpeed.Length();
                    targetSpeed = Vector2.Normalize(targetSpeed);

                    TargetVelocity = targetSpeed*100.0f;
                }
            }
            else if (valueChanged)
            {
                networkUpdateTimer -= deltaTime;
                if (networkUpdateTimer<=0.0f)
                {
                    item.NewComponentEvent(this, true);
                    networkUpdateTimer = 1.0f;
                    valueChanged = false;
                }
            }

            item.SendSignal(targetVelocity.X.ToString(CultureInfo.InvariantCulture), "velocity_x_out");
            item.SendSignal((-targetVelocity.Y).ToString(CultureInfo.InvariantCulture), "velocity_y_out");
        }

        public override void DrawHUD(SpriteBatch spriteBatch, Character character)
        {
            //if (voltage < minVoltage) return;

            int width = GuiFrame.Rect.Width, height = GuiFrame.Rect.Height;
            int x = GuiFrame.Rect.X;
            int y = GuiFrame.Rect.Y;

            GuiFrame.Draw(spriteBatch);

            Rectangle velRect = new Rectangle(x + 20, y + 20, width - 40, height - 40);
            GUI.DrawRectangle(spriteBatch, velRect, Color.White, false);

            if (GUI.DrawButton(spriteBatch, new Rectangle(x + width - 150, y + height - 30, 150, 30), "Autopilot"))
            {
                AutoPilot = !AutoPilot;

                item.NewComponentEvent(this, true);
            }

            GUI.DrawLine(spriteBatch,
                new Vector2(velRect.Center.X,velRect.Center.Y), 
                new Vector2(velRect.Center.X + currVelocity.X, velRect.Center.Y - currVelocity.Y), 
                Color.Gray);

            Vector2 targetVelPos = new Vector2(velRect.Center.X + targetVelocity.X, velRect.Center.Y - targetVelocity.Y);

            GUI.DrawLine(spriteBatch,
                new Vector2(velRect.Center.X, velRect.Center.Y),
                targetVelPos,
                Color.LightGray);

            GUI.DrawRectangle(spriteBatch, new Rectangle((int)targetVelPos.X - 5, (int)targetVelPos.Y - 5, 10, 10), Color.White);

            if (Vector2.Distance(PlayerInput.MousePosition, new Vector2(velRect.Center.X, velRect.Center.Y)) < 200.0f)
            {
                GUI.DrawRectangle(spriteBatch, new Rectangle((int)targetVelPos.X -10, (int)targetVelPos.Y - 10, 20, 20), Color.Red);

                if (PlayerInput.LeftButtonDown())
                {
                    TargetVelocity = PlayerInput.MousePosition - new Vector2(velRect.Center.X, velRect.Center.Y);
                    targetVelocity.Y = -targetVelocity.Y;

                    valueChanged = true;
                }
            }
        }

        public override void ReceiveSignal(string signal, Connection connection, Item sender, float power=0.0f)
        {
            if (connection.Name == "velocity_in")
            {
                currVelocity = ToolBox.ParseToVector2(signal, false);
            }  
        }

        public override void FillNetworkData(Networking.NetworkEventType type, Lidgren.Network.NetOutgoingMessage message)
        {
            message.Write(targetVelocity.X);
            message.Write(targetVelocity.Y);

            message.Write(autoPilot);
        }

        public override void ReadNetworkData(Networking.NetworkEventType type, Lidgren.Network.NetIncomingMessage message)
        {
            Vector2 newTargetVelocity   = Vector2.Zero;
            bool newAutoPilot           = false;

            try
            {
                newTargetVelocity = new Vector2(message.ReadFloat(), message.ReadFloat());
                newAutoPilot = message.ReadBoolean();
            }

            catch
            {
                return;
            }

            TargetVelocity = newTargetVelocity;
            AutoPilot = newAutoPilot;
        }
    }
}
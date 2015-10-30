﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barotrauma.Networking
{
    enum SelectionMode
    {
        Manual = 0, Random = 1, Vote = 2
    }

    partial class GameServer : NetworkMember 
    {
        public bool ShowNetStats;

        private TimeSpan refreshMasterInterval = new TimeSpan(0, 0, 40);
        private TimeSpan sparseUpdateInterval = new TimeSpan(0, 0, 0, 3);

        private SelectionMode subSelectionMode, modeSelectionMode;

        private bool randomizeSeed = true;
        
        private bool registeredToMaster;

        private BanList banList;

        private string password;

        private GUIFrame settingsFrame;

        public float AutoRestartTimer;

        private bool autoRestart;

        public bool AutoRestart
        {
            get { return (connectedClients.Count == 0) ? false : autoRestart; }
            set
            {
                autoRestart = value;

                AutoRestartTimer = autoRestart ? 20.0f : 0.0f;
            }
        }

        public SelectionMode SubSelectionMode
        {
            get { return subSelectionMode; }
        }

        public SelectionMode ModeSelectionMode
        {
            get { return modeSelectionMode; }
        }

        public bool RandomizeSeed
        {
            get { return randomizeSeed; }
        }

        public BanList BanList
        {
            get { return banList; }
        }

        private void CreateSettingsFrame()
        {
            settingsFrame = new GUIFrame(new Rectangle(0,0,GameMain.GraphicsWidth,GameMain.GraphicsHeight), Color.Black*0.5f);

            GUIFrame innerFrame = new GUIFrame(new Rectangle(0,0,400,400), null, Alignment.Center, GUI.Style, settingsFrame);

            var randomizeLevelBox = new GUITickBox(new Rectangle(0, 0, 20, 20), "Randomize level seed", Alignment.Left, innerFrame);
            randomizeLevelBox.OnSelected = ToggleRandomizeSeed;
            
            new GUITextBlock(new Rectangle(0, 35, 100, 20), "Submarine selection:", GUI.Style, innerFrame);
            var selectionFrame = new GUIFrame(new Rectangle(0, 60, 300, 20), null, innerFrame);
            for (int i = 0; i<3; i++)
            {
                var selectionTick = new GUITickBox(new Rectangle(i * 100, 00, 20, 20), ((SelectionMode)i).ToString(), Alignment.Left, selectionFrame);
                selectionTick.Selected = i == (int)subSelectionMode;
                selectionTick.OnSelected = SwitchSubSelection;
                selectionTick.UserData = (SelectionMode)i;
            }
            
            new GUITextBlock(new Rectangle(0, 85, 100, 20), "Mode selection:", GUI.Style, innerFrame);
            selectionFrame = new GUIFrame(new Rectangle(0, 110, 300, 20), null, innerFrame);
            for (int i = 0; i<3; i++)
            {
                var selectionTick = new GUITickBox(new Rectangle(i*100, 0, 20, 20), ((SelectionMode)i).ToString(), Alignment.Left, selectionFrame);
                selectionTick.Selected = i == (int)modeSelectionMode;
                selectionTick.OnSelected = SwitchModeSelection;
                selectionTick.UserData = (SelectionMode)i;
            }

            var closeButton = new GUIButton(new Rectangle(0, 0, 100, 20), "Close", Alignment.BottomRight, GUI.Style, innerFrame);
            closeButton.OnClicked = ToggleSettingsFrame;
        }

        private bool SwitchSubSelection(GUITickBox tickBox)
        {
            subSelectionMode = (SelectionMode)tickBox.UserData;

            foreach (GUIComponent otherTickBox in tickBox.Parent.children)
            {
                if (otherTickBox == tickBox) continue;
                ((GUITickBox)otherTickBox).Selected = false;
            }

            Voting.AllowSubVoting = subSelectionMode == SelectionMode.Vote;

            if (subSelectionMode==SelectionMode.Random)
            {
                GameMain.NetLobbyScreen.SubList.Select(Rand.Range(0, GameMain.NetLobbyScreen.SubList.CountChildren));
            }

            return true;
        }

        private bool SwitchModeSelection(GUITickBox tickBox)
        {
            modeSelectionMode = (SelectionMode)tickBox.UserData;

            foreach (GUIComponent otherTickBox in tickBox.Parent.children)
            {
                if (otherTickBox == tickBox) continue;
                ((GUITickBox)otherTickBox).Selected = false;
            }

            Voting.AllowModeVoting = modeSelectionMode == SelectionMode.Vote;

            if (modeSelectionMode == SelectionMode.Random)
            {
                GameMain.NetLobbyScreen.ModeList.Select(Rand.Range(0, GameMain.NetLobbyScreen.ModeList.CountChildren));
            }

            return true;
        }

        private bool ToggleRandomizeSeed(GUITickBox tickBox)
        {
            randomizeSeed = tickBox.Selected;
            return true;
        }

        public bool ToggleSettingsFrame(GUIButton button, object obj)
        {
            if (settingsFrame==null)
            {
                CreateSettingsFrame();
            }
            else
            {
                settingsFrame = null;
            }

            return true;
        }
    }
}
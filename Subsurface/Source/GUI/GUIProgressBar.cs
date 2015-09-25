﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Subsurface
{
    public class GUIProgressBar : GUIComponent
    {
        private bool isHorizontal;

        private GUIFrame frame;
        private float barSize;

        private int margin;

        public delegate float ProgressGetterHandler();
        public ProgressGetterHandler ProgressGetter;

        public bool IsHorizontal
        {
            get { return isHorizontal; }
        }

        public float BarSize
        {
            get { return barSize; }
            set 
            {
                float oldBarSize = barSize;
                barSize = MathHelper.Clamp(value, 0.0f, 1.0f);
                if (barSize!=oldBarSize) UpdateRect();
            }
        }

        public GUIProgressBar(Rectangle rect, Color color, float barSize, GUIComponent parent = null)
            : this(rect,color,barSize, (Alignment.Left | Alignment.Top), parent)
        {
        }

        public GUIProgressBar(Rectangle rect, Color color, float barSize, Alignment alignment, GUIComponent parent = null)
            : base(null)
        {
            this.rect = rect;
            this.color = color;
            isHorizontal = (rect.Width > rect.Height);
            
            this.alignment = alignment;

            margin = 5;

            if (parent != null)
                parent.AddChild(this);

            frame = new GUIFrame(new Rectangle(0,0,0,0), Color.White, null, this);

            this.barSize = barSize;
            UpdateRect();
        }

        private void UpdateRect()
        {
            rect = new Rectangle(
                frame.Rect.X + margin,
                frame.Rect.Y + margin,
                isHorizontal ? (int)((frame.Rect.Width - margin * 2) * barSize) : (frame.Rect.Width - margin * 2),
                isHorizontal ? (frame.Rect.Height - margin * 2) : (int)((frame.Rect.Height - margin * 2) * barSize));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible) return;

            if (ProgressGetter != null) BarSize = ProgressGetter();

            DrawChildren(spriteBatch);

            GUI.DrawRectangle(spriteBatch, rect, color * (color.A / 255.0f), true);
        }

    }
}
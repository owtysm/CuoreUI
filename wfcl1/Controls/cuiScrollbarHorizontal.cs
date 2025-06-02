using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CuoreUI.Controls
{
    [ToolboxBitmap(typeof(HScrollBar))]
    public partial class cuiScrollbarHorizontal : Control
    {
        private int thumbWidth;
        private ScrollableControl targetControl;
        private ScrollMessageFilter messageFilter;

        private bool isThumbHovered = false;
        private bool isThumbPressed = false;
        private int thumbPosition = 0;
        private int initialMouseX = 0;
        private int initialThumbPosition = 0;

        public int ThumbWidth
        {
            get
            {
                return thumbWidth;
            }
            set
            {
                thumbWidth = value;
                Invalidate();
            }
        }

        [Category("CuoreUI")]
        public ScrollableControl TargetControl
        {
            get
            {
                return targetControl;
            }
            set
            {
                if (targetControl != value)
                {
                    if (targetControl != null)
                    {
                        targetControl.Scroll -= TargetControl_Scroll;
                        targetControl.Resize -= TargetControl_Resize;
                        messageFilter?.ReleaseHandle();
                    }

                    if (Dock != DockStyle.None)
                    {
                        Dock = DockStyle.None;
                    }

                    targetControl = value;

                    if (targetControl != null)
                    {
                        targetControl.Scroll += TargetControl_Scroll;
                        targetControl.Resize += TargetControl_Resize;
                        messageFilter = new ScrollMessageFilter(this);
                        messageFilter.AssignHandle(targetControl.Handle);
                        BindToTargetControl();
                    }
                }
            }
        }

        private void BindToTargetControl()
        {
            if (targetControl != null)
            {
                this.Width = targetControl.Width;
                this.Location = new Point(targetControl.Left, targetControl.Bottom - this.Height);
                this.BringToFront();
                UpdateThumbPosition();

                if (Parent is Form == false)
                {
                    FindForm()?.Controls.Add(this);
                }
            }
        }

        public cuiScrollbarHorizontal()
        {
            this.SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.thumbWidth = 50;
            MinimumSize = new Size(50, 20);
        }

        private void TargetControl_Scroll(object sender, ScrollEventArgs e)
        {
            UpdateThumbPosition();
            Refresh();
        }

        private void TargetControl_Resize(object sender, EventArgs e)
        {
            BindToTargetControl();
            Refresh();
        }

        private void UpdateThumbPosition()
        {
            if (targetControl != null)
            {
                int max = targetControl.HorizontalScroll.Maximum - targetControl.HorizontalScroll.LargeChange + 1;
                int value = targetControl.HorizontalScroll.Value;

                float ratio = (float)value / max;
                thumbPosition = (int)((Width - thumbWidth) * ratio);
                Invalidate();
            }
        }

        private int privateRounding = 5;

        [Category("CuoreUI")]
        public int Rounding
        {
            get
            {
                return privateRounding;
            }
            set
            {
                privateRounding = value;
                Invalidate();
            }
        }

        private Color privateBackground = Color.Transparent;

        [Category("CuoreUI")]
        public Color Background
        {
            get
            {
                return privateBackground;
            }
            set
            {
                privateBackground = value;
                Invalidate();
            }
        }

        private Color privateThumbColor = Drawing.PrimaryColor;

        [Category("CuoreUI")]
        public Color ThumbColor
        {
            get
            {
                return privateThumbColor;
            }
            set
            {
                privateThumbColor = value;
                Invalidate();
            }
        }

        private Color privateHoveredThumbColor = Drawing.TranslucentPrimaryColor;

        [Category("CuoreUI")]
        public Color HoveredThumbColor
        {
            get
            {
                return privateHoveredThumbColor;
            }
            set
            {
                privateHoveredThumbColor = value;
                Invalidate();
            }
        }

        private Color privatePressedThumbColor = Drawing.TranslucentPrimaryColor;

        [Category("CuoreUI")]
        public Color PressedThumbColor
        {
            get
            {
                return privatePressedThumbColor;
            }
            set
            {
                privatePressedThumbColor = value;
                Invalidate();
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (targetControl != null)
            {
                if (Location != new Point(targetControl.Left, targetControl.Bottom - this.Height))
                {
                    Location = new Point(targetControl.Left, targetControl.Bottom - this.Height);
                }

                targetControl.AutoScroll = true;
                Width = targetControl.Width;
            }

            e.Graphics.Clear(BackColor);

            Rectangle modifiedCR = new Rectangle(0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);

            GraphicsPath bgPath = Helper.RoundRect(modifiedCR, Rounding);
            e.Graphics.FillPath(new SolidBrush(Background), bgPath);

            Rectangle thumbRect = new Rectangle(thumbPosition, 0, thumbWidth - 1, Height - 1);
            GraphicsPath thumbPath = Helper.RoundRect(thumbRect, Rounding);
            e.Graphics.FillPath(
                  isThumbPressed ? new SolidBrush(PressedThumbColor)
                : isThumbHovered ? new SolidBrush(HoveredThumbColor)
                : new SolidBrush(ThumbColor), thumbPath);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                Rectangle thumbRect = new Rectangle(thumbPosition, 0, thumbWidth, Height);
                if (thumbRect.Contains(e.Location))
                {
                    isThumbPressed = true;
                    Capture = true;
                    initialMouseX = e.X;
                    initialThumbPosition = thumbPosition;
                    Invalidate();
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                isThumbPressed = false;
                Capture = false;
                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (isThumbPressed)
            {
                int deltaX = e.X - initialMouseX;
                int newThumbPosition = initialThumbPosition + deltaX;
                newThumbPosition = Math.Max(0, Math.Min(Width - thumbWidth, newThumbPosition));
                float ratio = (float)newThumbPosition / (Width - thumbWidth);

                if (targetControl != null)
                {
                    int max = targetControl.HorizontalScroll.Maximum - targetControl.HorizontalScroll.LargeChange + 1;
                    targetControl.HorizontalScroll.Value = (int)(max * ratio);
                }

                thumbPosition = newThumbPosition;
                Refresh();
            }
            else
            {
                Rectangle thumbRect = new Rectangle(thumbPosition, 0, thumbWidth, Height);
                bool hovered = thumbRect.Contains(e.Location);

                if (hovered != isThumbHovered)
                {
                    isThumbHovered = hovered;
                    Invalidate();
                }
            }
        }

        private class ScrollMessageFilter : NativeWindow
        {
            private cuiScrollbarHorizontal scrollbar;

            public ScrollMessageFilter(cuiScrollbarHorizontal scrollbar)
            {
                this.scrollbar = scrollbar;
            }

            protected override void WndProc(ref Message m)
            {
                const int WM_HSCROLL = 0x0114;
                const int WM_MOUSEWHEEL = 0x020A;

                if (m.Msg == WM_HSCROLL || m.Msg == WM_MOUSEWHEEL)
                {
                    scrollbar.UpdateThumbPosition();
                }

                base.WndProc(ref m);

                scrollbar.Refresh();
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.Style |= 1;
                return createParams;
            }
        }
    }
}

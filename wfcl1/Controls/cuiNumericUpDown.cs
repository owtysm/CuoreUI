using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CuoreUI.Controls
{
    [ToolboxBitmap(typeof(NumericUpDown))]
    [DefaultEvent("ValueChanged")]
    public partial class cuiNumericUpDown : UserControl
    {
        public static class States
        {
            public const int Normal = 1;
            public const int Hovered = 2;
            public const int Pressed = 3;
        }

        public cuiNumericUpDown()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        private Padding privateRounding = new System.Windows.Forms.Padding(4, 4, 4, 4);
        public Padding Rounding
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

        private Color privateNormalBackground = CuoreUI.Drawing.PrimaryColor;
        public Color NormalBackground
        {
            get
            {
                return privateNormalBackground;
            }
            set
            {
                privateNormalBackground = value;
                Invalidate();
            }
        }

        private Color privateHoverBackground = CuoreUI.Drawing.TranslucentPrimaryColor;
        public Color HoverBackground
        {
            get
            {
                return privateHoverBackground;
            }
            set
            {
                privateHoverBackground = value;
                Invalidate();
            }
        }

        private Color privatePressedBackground = CuoreUI.Drawing.PrimaryColor;
        public Color PressedBackground
        {
            get
            {
                return privatePressedBackground;
            }
            set
            {
                privatePressedBackground = value;
                Invalidate();
            }
        }

        private Color privateNormalOutline = Color.Empty;
        public Color NormalOutline
        {
            get
            {
                return privateNormalOutline;
            }
            set
            {
                privateNormalOutline = value;
                Invalidate();
            }
        }

        private Color privateHoverOutline = Color.Empty;
        public Color HoverOutline
        {
            get
            {
                return privateHoverOutline;
            }
            set
            {
                privateHoverOutline = value;
                Invalidate();
            }
        }

        private Color privatePressedOutline = Color.Empty;
        public Color PressedOutline
        {
            get
            {
                return privatePressedOutline;
            }
            set
            {
                privatePressedOutline = value;
                Invalidate();
            }
        }

        private Color privateNormalArrowColor = Color.FromArgb(128, 255, 255, 255);
        public Color NormalArrowColor
        {
            get
            {
                return privateNormalArrowColor;
            }
            set
            {
                privateNormalArrowColor = value;
                Invalidate();
            }
        }

        private Color privateHoverArrowColor = Color.FromArgb(255, 255, 255, 255);
        public Color HoverArrowColor
        {
            get
            {
                return privateHoverArrowColor;
            }
            set
            {
                privateHoverArrowColor = value;
                Invalidate();
            }
        }

        private Color privatePressedArrowColor = Color.FromArgb(128, 255, 255, 255);
        public Color PressedArrowColor
        {
            get
            {
                return privatePressedArrowColor;
            }
            set
            {
                privatePressedArrowColor = value;
                Invalidate();
            }
        }

        byte _btn1State = States.Normal;
        byte btn1State
        {
            get
            {
                return _btn1State;
            }
            set
            {
                if (_btn1State != value)
                {
                    _btn1State = value;
                    Refresh();
                }
            }
        }
        byte _btn2State = States.Normal;
        byte btn2State
        {
            get
            {
                return _btn2State;
            }
            set
            {
                if (_btn2State != value)
                {
                    _btn2State = value;
                    Refresh();
                }
            }
        }

        private float privateValue = 50;
        private float privateMinValue = 0;
        private float privateMaxValue = 100;

        private float privateStepSize = 5;

        public float Value
        {
            get
            {
                return privateValue;
            }
            set
            {
                value = Math.Min(privateMaxValue, Math.Max(value, privateMinValue));
                bool isNewValue = value != privateValue;
                privateValue = value;
                Refresh();
                if (isNewValue)
                    ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public float MinValue
        {
            get
            {
                return privateMinValue;
            }
            set
            {
                if (value < privateMaxValue && value <= privateValue)
                {
                    privateMinValue = value;
                    Refresh();
                }
            }
        }

        public float MaxValue
        {
            get
            {
                return privateMaxValue;
            }
            set
            {
                if (value > privateMinValue && value >= privateValue)
                {
                    privateMaxValue = value;
                    Refresh();
                }
            }
        }

        public float StepSize
        {
            get
            {
                return privateStepSize;
            }
            set
            {
                privateStepSize = value;
                Refresh();
            }
        }

        public event EventHandler ValueChanged;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (PointToClient(Cursor.Position).Y > Height / 2)
            {
                btn1State = States.Normal;
                btn2State = States.Pressed;

                Value -= StepSize;
            }
            else
            {
                btn1State = States.Pressed;
                btn2State = States.Normal;

                Value += StepSize;
            }

            base.OnClick(e);
            Focus();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (PointToClient(Cursor.Position).Y > Height / 2)
            {
                btn1State = States.Normal;
                btn2State = States.Hovered;
            }
            else
            {
                btn1State = States.Hovered;
                btn2State = States.Normal;
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            btn1State = States.Normal;
            btn2State = States.Normal;

            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            int halfHeight = Height / 2;
            Rectangle btnRectangle = new Rectangle(0, 0, Width - 2, halfHeight);

            byte currentBtn = 0;
            void DrawButton(int buttonState)
            {
                GraphicsPath bgPath = null;
                GraphicsPath arrowPath = null;

                Rectangle arrowRectangle = btnRectangle;
                arrowRectangle.Width = Math.Min(arrowRectangle.Width, arrowRectangle.Height) / 2;
                arrowRectangle.Height = arrowRectangle.Width - 1;
                arrowRectangle.X = btnRectangle.Width / 2 - arrowRectangle.Width / 2;

                arrowRectangle.Inflate(0, -2);

                switch (currentBtn)
                {
                    case 0:
                        arrowRectangle.Y = 3 + halfHeight / 2 - arrowRectangle.Width / 2;

                        bgPath = Helper.RoundRect(btnRectangle, new Padding(Rounding.Left, Rounding.Top, 0, 0));
                        arrowPath = Helper.UpArrow(arrowRectangle);
                        break;
                    case 1:
                        arrowRectangle.Y = 1 + (Height + halfHeight) / 2 - arrowRectangle.Width / 2;

                        bgPath = Helper.RoundRect(btnRectangle, new Padding(0, 0, Rounding.Right, Rounding.Bottom));
                        arrowPath = Helper.DownArrow(arrowRectangle);
                        break;
                }

                SolidBrush arrowBrush = null;
                SolidBrush bgBrush = null;
                Pen outlinePen = null;

                switch (buttonState)
                {
                    case States.Normal:
                        arrowBrush = new SolidBrush(NormalArrowColor);
                        bgBrush = new SolidBrush(NormalBackground);
                        outlinePen = new Pen(NormalOutline);
                        break;
                    case States.Hovered:
                        arrowBrush = new SolidBrush(HoverArrowColor);
                        bgBrush = new SolidBrush(HoverBackground);
                        outlinePen = new Pen(HoverOutline);
                        break;
                    case States.Pressed:
                        arrowBrush = new SolidBrush(PressedArrowColor);
                        bgBrush = new SolidBrush(PressedBackground);
                        outlinePen = new Pen(PressedOutline);
                        break;
                }

                e.Graphics.FillPath(bgBrush, bgPath);
                e.Graphics.DrawPath(outlinePen, bgPath);
                e.Graphics.FillPath(arrowBrush, arrowPath);

                bgPath.Dispose();
                arrowPath.Dispose();
                arrowBrush.Dispose();
                bgBrush.Dispose();
                outlinePen.Dispose();
            }

            // button 1

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            DrawButton(btn1State);

            // button 2

            btnRectangle.Offset(0, halfHeight - 1);

            // if button's height is odd, cover the visible seam
            if (Height % 2 == 1)
            {
                btnRectangle.Y++;
                using (Pen br = new Pen(NormalBackground))
                {
                    e.Graphics.DrawLine(br, new PointF(btnRectangle.Location.X + 0.5f, btnRectangle.Location.Y), new PointF(Width - 2.5f, btnRectangle.Location.Y));
                }
            }

            currentBtn++;
            DrawButton(btn2State);

            base.OnPaint(e);
        }
    }
}

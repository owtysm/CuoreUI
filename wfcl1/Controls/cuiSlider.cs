using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CuoreUI.Controls
{
    [ToolboxBitmap(typeof(TrackBar))]
    [DefaultEvent("ValueChanged")]
    public partial class cuiSlider : UserControl
    {
        public cuiSlider()
        {
            InitializeComponent();
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        private float privateValue = 100;
        private float privateMinValue = 0;
        private float privateMaxValue = 100;

        // [0 - 1]
        public double GetProgress()
        {
            // if this is true what are you even doing
            if (MaxValue == MinValue)
                return 0;

            return (double)(Value - MinValue) / (MaxValue - MinValue);
        }

        // [-1 - 1]
        private double GetProgressHalfNormalized()
        {
            double progress = GetProgress();
            progress = (-progress);

            if (progress < 0)
            {
                progress = -progress;
            }

            return progress * 2;
        }

        public float Value
        {
            get
            {
                return privateValue;
            }
            set
            {
                if (value >= privateMinValue && value <= privateMaxValue)
                {
                    bool isNewValue = value != privateValue;

                    privateValue = (int)value;

                    UpdateThumbRectangle();
                    Refresh();

                    if (isNewValue)
                    {
                        ValueChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        private void UpdateThumbRectangle()
        {
            float thumbHeight = (Height / 8f) * 5;
            float halfThumbHeight = thumbHeight / 2;

            double progInverted = GetProgressHalfNormalized();
            ThumbRectangle = new RectangleF((float)((Width * GetProgress()) - ((ThumbRectangle.Width / 2) * progInverted) - (1 * progInverted)), (Height / 2) - halfThumbHeight - 1, thumbHeight, thumbHeight);
        }

        private void UpdateThumbRectangle(out float halfThumb)
        {
            float thumbHeight = (Height / 8f) * 5;
            float halfThumbHeight = thumbHeight / 2;

            double progInverted = GetProgressHalfNormalized();
            ThumbRectangle = new RectangleF((float)((Width * GetProgress()) - ((ThumbRectangle.Width / 2) * progInverted) - (1 * progInverted)), (Height / 2) - halfThumbHeight - 1, thumbHeight, thumbHeight);

            halfThumb = halfThumbHeight;
        }

        public event EventHandler ValueChanged;

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

        private Color privateTrackColor = Color.FromArgb(64, 128, 128, 128);
        public Color TrackColor
        {
            get
            {
                return privateTrackColor;
            }
            set
            {
                privateTrackColor = value;
                Refresh();
            }
        }

        private Color privateThumbColor = CuoreUI.Drawing.PrimaryColor;
        public Color ThumbColor
        {
            get
            {
                return privateThumbColor;
            }
            set
            {
                privateThumbColor = value;
                Refresh();
            }
        }

        RectangleF ThumbRectangle = Rectangle.Empty;

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF trackRectangle = new RectangleF(0, 0, Width - 1, (Height / 8) + 0.5f);
            trackRectangle.Y = (Height / 2) - (trackRectangle.Height / 2) - 0.5f;

            float halfThumbHeight;
            UpdateThumbRectangle(out halfThumbHeight);

            trackRectangle.Inflate(-halfThumbHeight, 0);
            using (GraphicsPath trackPath = Helper.RoundRect(trackRectangle, (int)((trackRectangle.Height + 0.5f) / 2)))
            using (SolidBrush trackBrush = new SolidBrush(TrackColor))
            {
                e.Graphics.FillPath(trackBrush, trackPath);
            }

            using (Pen thumbOutlinePen = new Pen(BackColor, ThumbOutlineThickness))
            using (SolidBrush thumbBrush = new SolidBrush(ThumbColor))
            {
                e.Graphics.DrawRectangles(thumbOutlinePen, new RectangleF[] { ThumbRectangle });
                e.Graphics.FillEllipse(thumbBrush, ThumbRectangle);
            }

            base.OnPaint(e);
        }

        private int privateThumbOutlineThickness = 3;
        public int ThumbOutlineThickness
        {
            get
            {
                return privateThumbOutlineThickness;
            }
            set
            {
                privateThumbOutlineThickness = value;
                Refresh();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            UpdateThumbRectangle();
            base.OnResize(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            OnMouseMove(new MouseEventArgs(MouseButtons.Left, 1, PointToClient(Cursor.Position).X, PointToClient(Cursor.Position).Y, 0));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Left)
            {
                float thumbWidth = ThumbRectangle.Width;
                float progress = Clamp((float)(e.X - (thumbWidth / 2)) / (Width - thumbWidth), 0f, 1f);

                Value = MinValue + progress * (MaxValue - MinValue);
            }
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    }
}

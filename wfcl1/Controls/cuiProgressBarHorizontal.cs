using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace CuoreUI.Controls
{
    [ToolboxBitmap(typeof(ProgressBar))]
    public partial class cuiProgressBarHorizontal : UserControl
    {
        public cuiProgressBarHorizontal()
        {
            InitializeComponent();
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        private int privateValue = 50;

        [Category("CuoreUI")]
        public int Value
        {
            get
            {
                return privateValue;
            }
            set
            {
                privateValue = value;
                Invalidate();
            }
        }

        private int privateMaxValue = 100;

        [Category("CuoreUI")]
        public int MaxValue
        {
            get
            {
                return privateMaxValue;
            }
            set
            {
                privateMaxValue = value;
                Invalidate();
            }
        }

        private bool privateFlipped = false;

        [Category("CuoreUI")]
        public bool Flipped
        {
            get
            {
                return privateFlipped;
            }
            set
            {
                privateFlipped = value;
                Invalidate();
            }
        }

        private Color privateBackground = Color.FromArgb(64, 128, 128, 128);

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

        private Color privateForeground = CuoreUI.Drawing.PrimaryColor;

        [Category("CuoreUI")]
        public Color Foreground
        {
            get
            {
                return privateForeground;
            }
            set
            {
                privateForeground = value;
                Invalidate();
            }
        }

        private int privateRounding = 8;

        [Category("CuoreUI")]
        public int Rounding
        {
            get
            {
                return privateRounding;
            }
            set
            {
                if (value > (ClientRectangle.Height / 2))
                {
                    privateRounding = ClientRectangle.Height / 2;
                    Rounding = privateRounding;
                }
                else
                {
                    privateRounding = value;
                }
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;

            Bitmap tempBitmap = new Bitmap(ClientSize.Width * 2, ClientSize.Height * 2);
            using (GraphicsPath roundBackground = Helper.RoundRect(new Rectangle(0, 0, ClientSize.Width * 2, ClientSize.Height * 2), Rounding * 2))
            using (Graphics tempGraphics = Graphics.FromImage(tempBitmap))
            {
                tempGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                tempGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                tempGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                tempGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;


                tempGraphics.SetClip(roundBackground);

                float filledPercent = (float)Value / MaxValue;
                float foreWidth = ClientRectangle.Width * filledPercent * 2;
                RectangleF foreHalf = new RectangleF(0, 0, foreWidth, ClientRectangle.Height * 2 + 1);
                RectangleF client = new RectangleF(foreWidth - Rounding - (foreWidth / 4), 0, ClientRectangle.Width * 2 - foreWidth + (Rounding * 2) + (foreWidth / 4), ClientRectangle.Height * 2);

                using (SolidBrush brush = new SolidBrush(Background))
                {
                    tempGraphics.FillRectangle(brush, client);
                }

                using (GraphicsPath graphicsPath = Helper.RoundRect(foreHalf, Rounding * 2))
                using (SolidBrush brush = new SolidBrush(Foreground))
                {
                    tempGraphics.FillPath(brush, graphicsPath);
                }
            }

            if (Flipped)
            {
                tempBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }

            e.Graphics.DrawImage(tempBitmap, ClientRectangle);

            tempBitmap.Dispose();

            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}

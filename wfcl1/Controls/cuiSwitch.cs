using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CuoreUI.Drawing;

namespace CuoreUI.Controls
{
    [ToolboxBitmap(typeof(ProgressBar))]
    [DefaultEvent("CheckedChanged")]
    public partial class cuiSwitch : UserControl
    {
        public cuiSwitch()
        {
            InitializeComponent();

            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            Size = new Size(48, 24);

            if (DesignMode)
            {
                privateBackground = BackColor;
            }

            ForeColor = Color.FromArgb(171, 171, 171);
        }

        private double elapsedTime = 0;

        const int Duration = 350;
        double xDistance = 2;
        float startX = 0;

        bool animationFinished = true;

        public async Task AnimateThumbLocation()
        {
            if (animating)
            {
                animationsInQueue++;
                return;
            }
            animating = true;

            startX = thumbX;

            if (Checked)
            {
                targetX = Width - 3.5f - (Height - 7) - (OutlineThickness / 2) + 0.5f; // lines 55 & 332
            }
            else
            {
                targetX = (Height / 2f) - (thumbRectangleInt.Height / 2f) + (OutlineThickness / 2) - 1.5f; // lines 59 & 336
            }

            xDistance = -(startX - targetX);

            double durationRatio = Duration / 1000.0;

            animationFinished = false;
            elapsedTime = 0;

            DateTime lastFrameTime = DateTime.Now;

            int desiredFrameInterval = 1000 / Drawing.GetHighestRefreshRate();
            EmergencySetLocation(Duration + desiredFrameInterval);

            while (true)
            {
                DateTime rightnow = DateTime.Now;
                double elapsedMilliseconds = (rightnow - lastFrameTime).TotalMilliseconds;
                lastFrameTime = rightnow;

                elapsedTime += (elapsedMilliseconds / Duration);

                if (elapsedTime >= Duration || animationFinished || animationsInQueue > 0)
                {
                    if (animationsInQueue > 0)
                    {
                        animationsInQueue--;
                        _ = AnimateThumbLocation();
                    }

                    //thumbX = (int)targetX;
                    animating = false;
                    animationFinished = false;
                    elapsedTime = 0;
                    Refresh();
                    return;
                }

                // sextic easing function
                double easing = CuoreUI.Drawing.EasingFunctions.FromEasingType(EasingTypes.SextOut, elapsedTime, Duration / (double)1000) * durationRatio;

                thumbX = (int)(startX + (xDistance * easing));
                Refresh();

                await Task.Delay(desiredFrameInterval);
            }
        }

        int animationsInQueue = 0;

        private async void EmergencySetLocation(int duration)
        {
            await Task.Delay(duration);
            thumbX = (int)targetX;
            animationFinished = true;
            Refresh();
        }

        private bool privateChecked = false;
        [Description("Whether the switch is on or off.")]
        public bool Checked
        {
            get
            {
                return privateChecked;
            }
            set
            {
                if (!animating)
                {
                    privateChecked = value;
                    CheckedChanged?.Invoke(this, EventArgs.Empty);
                    _ = AnimateThumbLocation();
                }
                Invalidate();
            }
        }

        private Color privateBackground = Color.Black;
        [Description("The rounded background for the CHECKED switch.")]
        public Color CheckedBackground
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

        private Color privateUncheckedBackground = Color.Black;
        [Description("The rounded background for the UNCHECKED switch.")]
        public Color UncheckedBackground
        {
            get
            {
                return privateUncheckedBackground;
            }
            set
            {
                privateUncheckedBackground = value;
                Invalidate();
            }
        }

        private Color privateCheckedForeground = CuoreUI.Drawing.PrimaryColor;
        [Description("The checked foreground.")]
        public Color CheckedForeground
        {
            get
            {
                return privateCheckedForeground;
            }
            set
            {
                privateCheckedForeground = value;
                Invalidate();
            }
        }

        private Color privateUncheckedForeground = Color.FromArgb(34, 34, 34);
        [Description("The unchecked foreground.")]
        public Color UncheckedForeground
        {
            get
            {
                return privateUncheckedForeground;
            }
            set
            {
                privateUncheckedForeground = value;
                Invalidate();
            }
        }

        private bool privateOutlineStyle = true;
        [Description("The style of the outline.")]
        public bool OutlineStyle
        {
            get
            {
                return privateOutlineStyle;
            }
            set
            {
                privateOutlineStyle = value;
                Invalidate();
            }
        }

        private Color privateOutlineColor = Color.FromArgb(34, 34, 34);
        [Description("The color of the outline.")]
        public Color UncheckedOutlineColor
        {
            get
            {
                return privateOutlineColor;
            }
            set
            {
                privateOutlineColor = value;
                Invalidate();
            }
        }

        private Color privateCheckedOutlineColor = CuoreUI.Drawing.PrimaryColor;
        [Description("The color of the checked outline.")]
        public Color CheckedOutlineColor
        {
            get
            {
                return privateCheckedOutlineColor;
            }
            set
            {
                privateCheckedOutlineColor = value;
                Invalidate();
            }
        }

        private Color privateSymbolColor = Color.FromArgb(34, 34, 34);
        [Description("The color of the outline.")]
        public Color UncheckedSymbolColor
        {
            get
            {
                return privateSymbolColor;
            }
            set
            {
                privateSymbolColor = value;
                Invalidate();
            }
        }

        private Color privateCheckedSymbolColor = CuoreUI.Drawing.PrimaryColor;
        [Description("The color of the checked outline.")]
        public Color CheckedSymbolColor
        {
            get
            {
                return privateCheckedSymbolColor;
            }
            set
            {
                privateCheckedSymbolColor = value;
                Invalidate();
            }
        }

        private float privateOutlineThickness = 1f;
        [Description("The thickness of the outline.")]
        public float OutlineThickness
        {
            get
            {
                return privateOutlineThickness;
            }
            set
            {
                privateOutlineThickness = value;
                Invalidate();
            }
        }

        private int thumbX = 2;

        private bool animating = false;

        float targetX = 2;
        RectangleF thumbRect;

        private bool privateShowSymbols = true;
        public bool ShowSymbols
        {
            get
            {
                return privateShowSymbols;
            }
            set
            {
                privateShowSymbols = value;
                Invalidate();
            }
        }

        public event EventHandler CheckedChanged;

        private Size privateThumbShrinkSize = new Size(0, 0);
        public Size ThumbSizeModifier
        {
            get
            {
                return privateThumbShrinkSize;
            }
            set
            {
                privateThumbShrinkSize = value;
                Refresh();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (animating == false)
            {
                if (Checked)
                {
                    thumbX = (int)(Width - 3.5f - (Height - 7) - (OutlineThickness / 2));
                }
                else
                {
                    thumbX = (int)((Height / 2f) - (thumbRectangleInt.Height / 2f) + (OutlineThickness / 2) - 2);
                }
            }
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int Rounding;
            try
            {
                Rounding = (Height / 2) - 1;
            }
            catch
            {
                Rounding = 1;
            }

            Rectangle modifiedCR = ClientRectangle;
            modifiedCR.Inflate(-1, -1);

            modifiedCR.Inflate(-(int)OutlineThickness, -(int)OutlineThickness);
            int temporaryRounding = Rounding - (int)OutlineThickness;

            GraphicsPath roundBackground = Helper.RoundRect(modifiedCR, temporaryRounding);

            using (SolidBrush brush = new SolidBrush(Checked ? CheckedBackground : UncheckedBackground))
            {
                e.Graphics.FillPath(brush, roundBackground);
            }

            int thumbDim = Height - 7;
            thumbRect = new RectangleF(thumbX, 3, thumbDim, thumbDim);
            thumbRect.Offset(0.5f, 0.5f);
            thumbRect.Inflate(-(int)(OutlineThickness), -(int)(OutlineThickness));

            thumbRect.Inflate(ThumbSizeModifier);

            Rectangle temporaryThumbRect = thumbRectangleInt;
            temporaryThumbRect.Offset(1, 0);

            temporaryThumbRect.Height = temporaryThumbRect.Width;

            using (SolidBrush brush = new SolidBrush(Checked ? CheckedForeground : UncheckedForeground))
            {
                e.Graphics.FillEllipse(brush, thumbRect);
            }

            using (Pen outlinePen = new Pen(Checked ? CheckedOutlineColor : UncheckedOutlineColor, OutlineThickness))
            {
                e.Graphics.DrawPath(outlinePen, roundBackground);
            }

            using (Pen graphicsPen = new Pen(UncheckedSymbolColor, Height / 10))
            {
                graphicsPen.StartCap = LineCap.Round;
                graphicsPen.EndCap = LineCap.Round;
                if (ShowSymbols)
                {
                    if (Checked)
                    {
                        graphicsPen.Color = CheckedSymbolColor;

                        temporaryThumbRect.Offset(0, 1);
                        e.Graphics.DrawPath(graphicsPen, Helper.Checkmark(temporaryThumbRect));
                    }
                    else
                    {
                        temporaryThumbRect.Inflate(-(int)(Height / 6.2f), -(int)(Height / 6.2f));

                        e.Graphics.DrawPath(graphicsPen, Helper.Crossmark(temporaryThumbRect));
                    }
                }
            }

            base.OnPaint(e);
        }

        Rectangle thumbRectangleInt
        {
            get
            {
                return new Rectangle((int)thumbRect.X, (int)thumbRect.Y, (int)thumbRect.Width, (int)thumbRect.Height);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            thumbX = 3;
            if (Width > 0)
            {
                int thumbDim = Height - 7;
                thumbX = Math.Min(Width - 5 - thumbDim, 3);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (animating == false)
            {
                Checked = !Checked;
            }
        }

        private void cuiSwitch_Load(object sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
        }
    }
}

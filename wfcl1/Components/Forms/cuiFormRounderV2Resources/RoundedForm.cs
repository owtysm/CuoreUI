using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static CuoreUI.Helper.Win32;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;

namespace CuoreUI.Components.cuiFormRounderV2Resources
{
    public partial class RoundedForm : Form
    {
        private Color privateBackgroundColor = Color.White;
        private Color privateBorderColor = Color.White;
        private Bitmap backImage;
        private Graphics backGraphics;

        private bool privateInvalidateNextDrawCall = false;
        public bool InvalidateNextDrawCall
        {
            get
            {
                return privateInvalidateNextDrawCall;
            }
            set
            {
                if (value == true)
                {
                    DrawForm(null, EventArgs.Empty);
                    privateInvalidateNextDrawCall = false;
                }
            }
        }

        private Image privateBackgroundImage;
        public Image BackgroundImageOfTargetForm
        {
            get => privateBackgroundImage;
            set
            {
                privateBackgroundImage?.Dispose();
                privateBackgroundImage = value;
                DrawForm(null, EventArgs.Empty);
            }
        }

        public Color BackgroundColor
        {
            get => privateBackgroundColor;
            set
            {
                privateBackgroundColor = value;
                Invalidate();
            }
        }

        public Color BorderColor
        {
            get => privateBorderColor;
            set
            {
                privateBorderColor = value;
                Invalidate();
            }
        }

        public int Rounding = 8;

        public RoundedForm(Color init_backgroundColor, Color init_borderColor, ref int RoundValue, bool show = true)
        {
            PerPixelAlphaBlend.SetBitmap(new Bitmap(1, 1), 0, Width, Height, Handle);

            Visible = show;
            InitializeComponent();
            SetStyles();
            BackgroundColor = init_backgroundColor;
            BorderColor = init_borderColor;
            Rounding = RoundValue;
        }

        private void SetStyles()
        {
            // turns out UserPaint was causing a white rectangle on top left (in most cases)
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            DoubleBuffered = true;
            this.UpdateStyles();
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        const int GWL_EXSTYLE = -20;
        const int WS_EX_LAYERED = 0x00080000;
        const int WS_EX_TRANSPARENT = 0x00000020;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // turn on WS_EX_TOOLWINDOW style bit
                cp.ExStyle |= 0x80;

                // workaround to get rid of the win7 style window when app launches
                cp.ExStyle |= WS_EX_LAYERED;
                cp.ExStyle |= WS_EX_TRANSPARENT | 0x20 | 0x08000000;
                return cp;
            }
        }

        private Bitmap targetFormBt = null;
        public Form TargetForm = null;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitializeWindowFix();
        }
        public void DrawForm(object sender, EventArgs e)
        {
            if (stop)
            {
                return;
            }

            try
            {
                privateBackgroundColor = (Color)TargetForm?.BackColor;
            }
            catch
            {

            }

            SuspendLayout();
            try
            {

                if (backImage == null || backImage.Size != Size || BackgroundColor != TargetForm?.BackColor || InvalidateNextDrawCall)
                {
                    InvalidateNextDrawCall = false;

                    backImage?.Dispose();
                    backGraphics?.Dispose();
                    backImage = new Bitmap(Width, Height);
                    backGraphics = Graphics.FromImage(backImage);
                }
                else
                {
                    backGraphics.Clear(Color.Transparent);
                }

                Rectangle gradientRectangle = new Rectangle(0, 0, Width - 2, Height - 2);
                GraphicsPath roundedRectangle = Helper.RoundRect(gradientRectangle, Rounding);

                Rectangle subtractRectangle = gradientRectangle;
                subtractRectangle.Offset(1, 1);
                subtractRectangle.Inflate(-1, -1);

                Rectangle fillinoutlineRectangle = subtractRectangle;
                fillinoutlineRectangle.Offset(-1, -1);

                GraphicsPath subtractPath = Helper.RoundRect(subtractRectangle, Rounding);
                GraphicsPath fillinoutlinePath = Helper.RoundRect(fillinoutlineRectangle, Rounding);

                using (Pen BorderPen = new Pen(BorderColor))
                {
                    backGraphics.SmoothingMode = SmoothingMode.AntiAlias;

                    if (Rounding > 0)
                    {
                        using (Pen BackgroundPen = new Pen(BackgroundColor, 1))
                        using (SolidBrush BackgroundBrush = new SolidBrush(BackgroundColor))
                        {
                            backGraphics.FillPath(BackgroundBrush, fillinoutlinePath);
                            backGraphics.DrawPath(BackgroundPen, fillinoutlinePath);
                        }

                        // means cuiFormRounder's 'EnhanceBorders' property is set to True
                        if (backgroundImageTextureBrush != null)
                        {
                            using (Pen EnhanceBordersPen = new Pen(backgroundImageTextureBrush, 1))
                            {
                                backGraphics.DrawPath(EnhanceBordersPen, fillinoutlinePath);
                            }
                        }
                    }

                    backGraphics.DrawPath(BorderPen, roundedRectangle);
                    backGraphics.SmoothingMode = SmoothingMode.None;
                }

                // Tag should ALWAYS be a double.
                // if not - it's either not initialized yet, or Tag was intentionally set to an unsupported value by the dev
                byte opacity = (byte)((double)Tag * 255);
                PerPixelAlphaBlend.SetBitmap(initialized ? backImage : null, initialized ? opacity : (byte)0, Left, Top, Handle);
            }
            finally
            {
                ResumeLayout();
            }
        }
        private void InitializeWindowFix()
        {
            Location = new Point(-Width + 1, -Height + 1);

            // black border and win7 pre init tool window style fix
            IntPtr currentStyle = GetWindowLong(Handle, GWL_EXSTYLE);

            // remove WS_EX_LAYERED and WS_EX_TRANSPARENT from this rounded form
            IntPtr newStyle = (IntPtr)((int)currentStyle & ~WS_EX_LAYERED & ~WS_EX_TRANSPARENT);
            SetWindowLong(Handle, GWL_EXSTYLE, newStyle);

            initialized = true;

            //Location = TargetForm.Location - new Size(1, 1);
            //Opacity = opacityBefore;
        }

        TextureBrush backgroundImageTextureBrush = null;

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!DesignMode)
            {
                var cp = NativeMethods.GetWindowLong(Handle, NativeMethods.GWL_EXSTYLE);
                cp |= NativeMethods.WS_EX_LAYERED;
                NativeMethods.SetWindowLong(Handle, NativeMethods.GWL_EXSTYLE, cp);
            }

            DrawForm(this, EventArgs.Empty);
        }

        protected override void OnResize(EventArgs e)
        {
            Region = null;
        }

        private void RoundedForm_PaddingChanged(object sender, EventArgs e)
        {
            DrawForm(this, e);
        }

        protected override void OnPaddingChanged(EventArgs e)
        {
            base.OnPaddingChanged(e);
            DrawForm(this, e);
        }

        private bool stop = false;

        internal void Stop()
        {
            stop = true;
            backImage?.Dispose();
            backGraphics?.Dispose();

            PaddingChanged -= RoundedForm_PaddingChanged;

            Dispose();
        }

        internal void UpdBitmap()
        {
            targetFormBt?.Dispose();
            backgroundImageTextureBrush?.Image?.Dispose();
            backgroundImageTextureBrush?.Dispose();
            backgroundImageTextureBrush = null;
        }

        internal void UpdBitmap(Bitmap experimentalBitmap)
        {
            // Dispose of the existing bitmap if it's not the same instance
            if (!ReferenceEquals(targetFormBt, experimentalBitmap))
            {
                targetFormBt?.Dispose();
                targetFormBt = experimentalBitmap; // Assign new Bitmap
            }

            // Dispose of and recreate the TextureBrush
            backgroundImageTextureBrush?.Dispose();
            backgroundImageTextureBrush = new TextureBrush(targetFormBt)
            {
                WrapMode = WrapMode.Clamp
            };
        }

    }
}

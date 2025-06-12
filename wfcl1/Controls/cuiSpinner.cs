using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static CuoreUI.Drawing;

namespace CuoreUI.Controls
{
    [Description("Loading spinner animated control")]
    [ToolboxBitmap(typeof(BackgroundWorker))]
    public partial class cuiSpinner : Control
    {
        Timer designerExclusiveRotationTimer = new Timer();

        private float privateRotateSpeed = 2;

        [Category("CuoreUI")]
        public float RotateSpeed
        {
            get
            {
                return privateRotateSpeed;
            }
            set
            {
                privateRotateSpeed = value;
                Invalidate();
            }
        }

        public bool RotateEnabled = true;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (DesignMode == false)
            {
                cuiSpinner_Load(this, EventArgs.Empty);
            }
        }

        public cuiSpinner()
        {
            InitializeComponent();

            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            Rotation = 0;

            if (alreadySpinning == false)
            {
                FrameDrawn -= RotateOnFrameDrawn;
                FrameDrawn += RotateOnFrameDrawn;
                alreadySpinning = true;
            }
        }

        bool alreadySpinning = false;

        private void RotateOnFrameDrawn(object sender, EventArgs e)
        {
            universalRotateLogic();
        }

        Drawing.TimeDeltaInstance tdi = new TimeDeltaInstance();
        void universalRotateLogic()
        {
            Rotation += (((RotateSpeed / 2)) * tdi.TimeDelta) % 360;
        }

        private Color privateArcColor = CuoreUI.Drawing.PrimaryColor;

        [Category("CuoreUI")]
        public Color ArcColor
        {
            get
            {
                return privateArcColor;
            }
            set
            {
                privateArcColor = value;
                Invalidate();
            }
        }

        private Color privateRingColor = Color.FromArgb(64, 128, 128, 128);

        [Category("CuoreUI")]
        public Color RingColor
        {
            get
            {
                return privateRingColor;
            }
            set
            {
                privateRingColor = value;
                Invalidate();
            }
        }

        private float privateRotation = 0;

        [Category("CuoreUI")]
        public float Rotation
        {
            get
            {
                return privateRotation;
            }
            set
            {
                if (value >= 360)
                {
                    value -= 360;
                }
                privateRotation = value;
                Refresh();
            }
        }

        private float privateArcSize = 5;

        [Category("CuoreUI")]
        public float Thickness
        {
            get
            {
                return privateArcSize;
            }
            set
            {
                privateArcSize = value;
                Invalidate();
            }
        }
        float ArcDegrees = 90;

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Rotation > 720)
            {
                Rotation = 0;
            }

            if (DesignMode && designerExclusiveRotationTimer.Enabled == false)
            {
                designerExclusiveRotationTimer.Start();
            }
            else if (!DesignMode && designerExclusiveRotationTimer.Enabled == true)
            {
                designerExclusiveRotationTimer.Stop();
            }

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            float SpinnerThickness = Thickness * 2f;

            RectangleF ClientConsideringArcSize = ClientRectangle;
            ClientConsideringArcSize.Width = Math.Min(ClientConsideringArcSize.Width, ClientConsideringArcSize.Height);
            ClientConsideringArcSize.Width = Math.Max(SpinnerThickness * 2 + (Thickness * 2), ClientConsideringArcSize.Width);
            ClientConsideringArcSize.Height = ClientConsideringArcSize.Width;
            ClientConsideringArcSize.Inflate(-SpinnerThickness, -SpinnerThickness);

            GraphicsPath ringPath = new GraphicsPath();
            ringPath.AddArc(ClientConsideringArcSize, 0, 360);

            GraphicsPath arcPath = new GraphicsPath();
            arcPath.AddArc(ClientConsideringArcSize, Rotation, ArcDegrees);

            GraphicsPath combinedPath = new GraphicsPath();

            combinedPath.AddPath(ringPath, false);
            combinedPath.AddPath(arcPath, false);

            using (Pen ringPen = new Pen(RingColor, SpinnerThickness))
            {
                e.Graphics.DrawPath(ringPen, ringPath);
            }

            using (Pen arcPen = new Pen(ArcColor, SpinnerThickness)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            })
            {
                e.Graphics.DrawPath(arcPen, arcPath);
            }

            base.OnPaint(e);
        }

        private void cuiSpinner_Load(object sender, EventArgs e)
        {
            Rotation = 0;
        }
    }
}

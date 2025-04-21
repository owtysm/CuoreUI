using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CuoreUI.Controls
{
    [ToolboxBitmap(typeof(Panel))]
    public partial class cuiPanel : Panel
    {
        public cuiPanel()
        {
            InitializeComponent();
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            Refresh();
        }

        private Color privatePanelColor = CuoreUI.Drawing.PrimaryColor;
        public Color PanelColor
        {
            get
            {
                return privatePanelColor;
            }
            set
            {
                privatePanelColor = value;
                Invalidate();
            }
        }

        private Color privatePanelOutlineColor = CuoreUI.Drawing.PrimaryColor;
        public Color PanelOutlineColor
        {
            get
            {
                return privatePanelOutlineColor;
            }
            set
            {
                privatePanelOutlineColor = value;
                Invalidate();
            }
        }

        private float privateOutlineThickness = 1;
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

        private Padding privateRounding = new Padding(8, 8, 8, 8);
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

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle modifiedCR = ClientRectangle;
            modifiedCR.Width -= 1;
            modifiedCR.Height -= 1;

            GraphicsPath roundBackground = Helper.RoundRect(modifiedCR, Rounding);
            using (SolidBrush brush = new SolidBrush(PanelColor))
            using (Pen pen = new Pen(PanelOutlineColor, OutlineThickness))
            {
                e.Graphics.FillPath(brush, roundBackground);
                e.Graphics.DrawPath(pen, roundBackground);
            }

            base.OnPaint(e);
        }
    }
}

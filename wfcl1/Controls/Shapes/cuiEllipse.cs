using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CuoreUI.Controls.Shapes
{
    public partial class cuiEllipse : UserControl
    {
        private Color privateOutlineColor = Color.Empty;

        [Category("CuoreUI")]
        public Color OutlineColor
        {
            get
            {
                return privateOutlineColor;
            }
            set
            {
                privateOutlineColor = value;
                Refresh();
            }
        }

        private Color privatePanelColor = CuoreUI.Drawing.PrimaryColor;

        [Category("CuoreUI")]
        public Color PanelColor
        {
            get
            {
                return privatePanelColor;
            }
            set
            {
                privatePanelColor = value;
                Refresh();
            }
        }

        private int privateOutlineThickness = 1;

        [Category("CuoreUI")]
        public int OutlineThickness
        {
            get
            {
                return privateOutlineThickness;
            }
            set
            {
                privateOutlineThickness = value;
                Refresh();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Refresh();
        }

        public cuiEllipse()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Rectangle modifiedCR = ClientRectangle;
            modifiedCR.Width -= 1;
            modifiedCR.Height -= 1;

            modifiedCR.Inflate(-OutlineThickness, -OutlineThickness);

            e.Graphics.FillEllipse(new SolidBrush(PanelColor), modifiedCR);
            e.Graphics.DrawEllipse(new Pen(OutlineColor, OutlineThickness), modifiedCR);
        }
    }
}

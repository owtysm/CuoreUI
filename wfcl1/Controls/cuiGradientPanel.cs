using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CuoreUI.Controls
{
    public partial class cuiGradientPanel : UserControl
    {
        public cuiGradientPanel()
        {
            InitializeComponent();
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            Refresh();
        }

        Color privatePanelColor1 = CuoreUI.Drawing.PrimaryColor;

        [Category("CuoreUI")]
        public Color PanelColor1
        {
            get
            {
                return privatePanelColor1;
            }
            set
            {
                privatePanelColor1 = value;
                Invalidate();
            }
        }

        Color privatePanelColor2 = Color.Transparent;

        [Category("CuoreUI")]
        public Color PanelColor2
        {
            get
            {
                return privatePanelColor2;
            }
            set
            {
                privatePanelColor2 = value;
                Invalidate();
            }
        }

        Color privatePanelOutlineColor1 = CuoreUI.Drawing.PrimaryColor;

        [Category("CuoreUI")]
        public Color PanelOutlineColor1
        {
            get
            {
                return privatePanelOutlineColor1;
            }
            set
            {
                privatePanelOutlineColor1 = value;
                Invalidate();
            }
        }

        Color privatePanelOutlineColor2 = CuoreUI.Drawing.PrimaryColor;

        [Category("CuoreUI")]
        public Color PanelOutlineColor2
        {
            get
            {
                return privatePanelOutlineColor2;
            }
            set
            {
                privatePanelOutlineColor2 = value;
                Invalidate();
            }
        }

        float privateOutlineThickness = 1;

        [Category("CuoreUI")]
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

        Padding privateRounding = new Padding(8, 8, 8, 8);

        [Category("CuoreUI")]
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

        float privateGradientAngle = 0;

        [Category("CuoreUI")]
        public float GradientAngle
        {
            get
            {
                return privateGradientAngle;
            }
            set
            {
                privateGradientAngle = value % 360;
                Invalidate();
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle modifiedCR = ClientRectangle;
            modifiedCR.Inflate(-1, -1);

            using (GraphicsPath roundBackground = Helper.RoundRect(modifiedCR, Rounding))
            {
                using (Pen br = new Pen(BackColor))
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    modifiedCR, privatePanelColor1, privatePanelColor2, privateGradientAngle, true))
                {
                    e.Graphics.FillPath(brush, roundBackground);
                    e.Graphics.DrawPath(br, roundBackground); // offset fix
                }

                using (LinearGradientBrush borderBrush = new LinearGradientBrush(
                    modifiedCR, privatePanelOutlineColor1, privatePanelOutlineColor2, privateGradientAngle, true))
                {
                    using (Pen pen = new Pen(borderBrush, privateOutlineThickness))
                    {
                        e.Graphics.DrawPath(pen, roundBackground);
                    }
                }
            }

            base.OnPaint(e);
        }
    }
}
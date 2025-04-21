using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CuoreUI.Controls
{
    public partial class cuiGroupBox : Panel
    {
        public cuiGroupBox()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            Content = "Group Box";
        }

        private Padding privateRounding = new Padding(4);
        [Category("CuoreUI")]
        [Description("How round will the inside border be.")]
        public Padding Rounding
        {
            get
            {
                return privateRounding;
            }
            set
            {
                privateRounding = value;
                Refresh();
            }
        }

        private Color privateBorderColor = Color.FromArgb(64, 128, 128, 128);
        [Category("CuoreUI")]
        [Description("The color of the inner border.")]
        public Color BorderColor
        {
            get
            {
                return privateBorderColor;
            }
            set
            {
                privateBorderColor = value;
                Refresh();
            }
        }

        private string privateContent = "cuiGroupBox";
        [Category("CuoreUI")]
        [Description("The text that appears on top left.")]
        public string Content
        {
            get
            {
                return privateContent;
            }
            set
            {
                privateContent = value;
                Refresh();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // l t r b
            Padding = new Padding(Rounding.Left, Rounding.Top + Font.Height - 2, Rounding.Right, Rounding.Bottom);

            Pen borderPen = new Pen(privateBorderColor);
            SolidBrush textBrush = new SolidBrush(ForeColor);

            // Draw the border rectangle
            Rectangle modifiedCR = new Rectangle(0, Font.Height / 2, Width - 1, Height - Font.Height / 2 - 1);
            GraphicsPath roundedPath = Helper.RoundRect(modifiedCR, Rounding);
            g.DrawPath(borderPen, roundedPath);

            // Draw the text at the top-left of the box
            Size textSize = TextRenderer.MeasureText(Content, Font);
            Rectangle textRect = new Rectangle(8, 0, textSize.Width + Font.Height, textSize.Height);
            g.FillRectangle(new SolidBrush(BackColor), textRect); // Clear background behind text
            g.DrawString(Content, Font, textBrush, textRect, new StringFormat() { Alignment = StringAlignment.Center });

            borderPen.Dispose();
            textBrush.Dispose();

            base.OnPaint(e);
        }
    }
}

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CuoreUI.Controls
{
    [ToolboxBitmap(typeof(Label))]
    public partial class cuiLabel : UserControl
    {
        public cuiLabel()
        {
            InitializeComponent();
            DoubleBuffered = true;
            AutoScaleMode = AutoScaleMode.None;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        private string privateContent = "Your text here!";
        public string Content
        {
            get
            {
                return Regex.Escape(privateContent);
            }
            set
            {
                privateContent = Regex.Unescape(value);
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            using (StringFormat stringFormat = new StringFormat() { Alignment = HorizontalAlignment, LineAlignment = VerticalAlignment })
            using (SolidBrush brush = new SolidBrush(ForeColor))
            {
                e.Graphics.DrawString(privateContent, Font, brush, ClientRectangle, stringFormat);
            }

            base.OnPaint(e);
        }

        private StringAlignment privateHorizontalAlignment = StringAlignment.Center;
        private StringAlignment privateVerticalAlignment = StringAlignment.Near;

        public StringAlignment HorizontalAlignment
        {
            get
            {
                return privateHorizontalAlignment;
            }
            set
            {
                privateHorizontalAlignment = value;
                Invalidate();
            }
        }

        public StringAlignment VerticalAlignment
        {
            get
            {
                return privateVerticalAlignment;
            }
            set
            {
                privateVerticalAlignment = value;
                Invalidate();
            }
        }
    }
}

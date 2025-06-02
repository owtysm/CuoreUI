using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CuoreUI.Controls
{
    [ToolboxBitmap(typeof(SplitContainer))]
    public partial class cuiSeparator : UserControl
    {
        public cuiSeparator()
        {
            InitializeComponent();
            ForeColor = Color.FromArgb(128, 128, 128, 128);
        }

        [Category("CuoreUI")]
        public override Color ForeColor 
        {
            get => base.ForeColor; 
            set => base.ForeColor = value;
        }

        private float privateThickness = 0.5f;

        [Category("CuoreUI")]
        public float Thickness
        {
            get
            {
                return privateThickness;
            }
            set
            {
                privateThickness = value;
                Invalidate();
            }
        }

        private bool privateVertical = false;

        [Category("CuoreUI")]
        public bool Vertical
        {
            get
            {
                return privateVertical;
            }
            set
            {
                privateVertical = value;
                Invalidate();
            }
        }

        private int privateSeparatorMargin = 8;

        [Category("CuoreUI")]
        public int SeparatorMargin
        {
            get
            {
                return privateSeparatorMargin;
            }
            set
            {
                privateSeparatorMargin = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            GraphicsPath tempPath = new GraphicsPath();
            RectangleF lineRect = RectangleF.Empty;

            if (Vertical)
            {
                int halfX = Width / 2;
                lineRect = new RectangleF(halfX, SeparatorMargin, Thickness, Height - (SeparatorMargin * 2));
            }
            else
            {
                int halfY = Height / 2;
                lineRect = new RectangleF(SeparatorMargin, halfY, Width - (SeparatorMargin * 2), Thickness);
                ;
            }

            tempPath.AddRectangle(lineRect);

            using (Pen pen = new Pen(ForeColor, Thickness))
            {
                e.Graphics.DrawPath(pen, tempPath);
            }
            base.OnPaint(e);
        }
    }
}

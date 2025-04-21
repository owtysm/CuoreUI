using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CuoreUI.TabControlStuff
{
    public partial class cuiTabPage : TabPage
    {
        public int Rounding = 5;

        public cuiTabPage()
        {
            InitializeComponent();
            ForeColor = Color.FromArgb(64, 128, 128, 128);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (SolidBrush brush = new SolidBrush(ForeColor))
            {
                e.Graphics.FillPath(brush, Helper.RoundRect(ClientRectangle, Rounding));
            }
        }

    }
}

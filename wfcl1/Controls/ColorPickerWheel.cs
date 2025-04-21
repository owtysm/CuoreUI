using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CuoreUI.Properties;
using static CuoreUI.Components.Forms.ColorPickerForm;

namespace CuoreUI.Controls
{
    [ToolboxItem(false)]
    [Description("Don't use, an internal part of color picker window.")]
    public partial class ColorPickerWheel : cuiPictureBox
    {
        Point lastClickPosition = new Point(-8, -8);
        public Themes Theme = Themes.Dark;

        public ColorPickerWheel()
        {
            Content = Resources.all_colours;
        }

        public ColorPickerWheel(Themes inputTheme)
        {
            Content = Resources.all_colours;
            Theme = inputTheme;
        }

        public void UpdatePos()
        {
            lastClickPosition = PointToClient(Cursor.Position);
            Refresh();
        }

        Pen whitepen = new Pen(Color.White, 1);
        Pen blackpen = new Pen(Color.Black, 1);

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int size = 8;

            RectangleF circleRect = new RectangleF(lastClickPosition.X - size, lastClickPosition.Y - size, size * 2, size * 2);

            e.Graphics.DrawEllipse(Theme == Themes.Dark ? whitepen : blackpen, circleRect);
        }
    }
}

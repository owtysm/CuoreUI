using System;
using System.Drawing;
using System.Windows.Forms;
using static CuoreUI.Helper.Win32;

namespace CuoreUI.Components.Forms
{
    public partial class TooltipForm : Form
    {
        public TooltipForm()
        {
            InitializeComponent();
            TextChanged += TooltipForm_TextChanged;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE;
                return cp;
            }
        }

        private void TooltipForm_TextChanged(object sender, EventArgs e)
        {
            cuiLabel1.Content = Text;
            Size textSize = CreateGraphics().MeasureString(Text, cuiLabel1.Font).ToSize();
            Size = new Size(textSize.Width + 2 + cuiLabel1.Font.Height, textSize.Height * 2);
        }

        private void TooltipForm_Resize(object sender, EventArgs e)
        {
            cuiLabel1.Location = new Point(0, cuiLabel1.Font.Height / 2);
            cuiLabel1.Width = Width;
            cuiLabel1.Height = Height;
        }

        private void TooltipForm_ForeColorChanged(object sender, EventArgs e)
        {
            cuiLabel1.ForeColor = ForeColor;
        }
    }
}

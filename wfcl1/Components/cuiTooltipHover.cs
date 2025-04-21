using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CuoreUI.Components.Forms;
using static CuoreUI.Helper;

namespace CuoreUI.Components
{
    public partial class cuiTooltipHover : Component
    {
        private TooltipForm tooltipForm => TooltipController.tooltipForm;
        public cuiTooltipHover()
        {
            InitializeComponent();
        }

        public cuiTooltipHover(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        private Control privateTargetControl;
        public Control TargetControl
        {
            get => privateTargetControl;
            set
            {
                privateTargetControl = value;
                if (privateTargetControl != null)
                {
                    value.MouseHover += MouseHover;
                }
            }
        }

        private string privateContent = "Tooltip Text";
        public string Content
        {
            get => privateContent;
            set
            {
                privateContent = value;
            }
        }

        public Color ForeColor
        {
            get
            {
                return tooltipForm.ForeColor;
            }
            set
            {
                tooltipForm.ForeColor = value;
            }
        }

        public Color BackColor
        {
            get
            {
                return tooltipForm.BackColor;
            }
            set
            {
                tooltipForm.BackColor = value;
            }
        }

        private async void MouseHover(object sender, System.EventArgs e)
        {
            tooltipForm.Text = privateContent;

            tooltipForm.Location = Cursor.Position - new Size((tooltipForm.Width / 2), -1);

            ToggleFormVisibilityWithoutActivating(tooltipForm, true);

            while (true)
            {
                await Task.Delay(Drawing.LazyInt32TimeDelta);
                if (TargetControl.ClientRectangle.Contains(TargetControl.PointToClient(Cursor.Position)) == false)
                {
                    break;
                }

                tooltipForm.Location = Cursor.Position - new Size((tooltipForm.Width / 2), -1);
            }

            ToggleFormVisibilityWithoutActivating(tooltipForm, false);
        }
    }
}

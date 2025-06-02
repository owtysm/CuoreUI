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
        [Category("CuoreUI")]
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
        [Category("CuoreUI")]
        public string Content
        {
            get => privateContent;
            set
            {
                privateContent = value;
            }
        }

        private Size privatePositionOffset = new Size(0, 0);
        [Category("CuoreUI")]
        public Size TooltipPositionOffset
        {
            get => privatePositionOffset;
            set
            {
                privatePositionOffset = value;
            }
        }

        [Category("CuoreUI")]
        [Description("Where the tooltip should show up, relative to where the cursor is.")]
        public enum Position
        {
            Custom,
            Top,
            Left,
            Right,
            Bottom
        }

        [Category("CuoreUI")]
        public Position TooltipPosition { get; set; } = Position.Top;

        [Category("CuoreUI")]
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

        [Category("CuoreUI")]
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
                await Task.Delay(Drawing.LazyTimeDelta);
                if (TargetControl.ClientRectangle.Contains(TargetControl.PointToClient(Cursor.Position)) == false)
                {
                    break;
                }

                Size offset = Size.Empty;

                if (TooltipPosition == Position.Custom)
                {
                    offset = new Size((tooltipForm.Width / 2), -1);
                }
                else if (TooltipPosition == Position.Top)
                {
                    offset = new Size((tooltipForm.Width / 2), 32);
                }
                else if (TooltipPosition == Position.Left)
                {
                    offset = new Size(tooltipForm.Width, tooltipForm.Height / 2);
                }
                else if (TooltipPosition == Position.Right)
                {
                    offset = new Size(0, tooltipForm.Height / 2);
                }
                else if (TooltipPosition == Position.Bottom)
                {
                    offset = new Size((tooltipForm.Width / 2), -24);
                }

                tooltipForm.Location = Cursor.Position - offset + privatePositionOffset;
            }

            ToggleFormVisibilityWithoutActivating(tooltipForm, false);
        }
    }
}

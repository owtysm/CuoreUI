using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CuoreUI.Components.Forms;
using static CuoreUI.Components.Forms.ColorPickerForm;

namespace CuoreUI.Components
{
    [ToolboxBitmap(typeof(ColorDialog))]
    public partial class cuiColorPicker : Component
    {
        private ColorPickerForm PickerForm;

        private Color privateColor;
        public Color Color
        {
            get
            {
                return privateColor;
            }
            set
            {
                privateColor = value;
            }
        }

        private bool privateEnableThemeChangeButton = true;
        [Description("Lets the USER toggle the theme between Light and Dark with a button.")]
        public bool EnableThemeChangeButton
        {
            get
            {
                return privateEnableThemeChangeButton;
            }
            set
            {
                privateEnableThemeChangeButton = value;
                PickerForm?.ToggleThemeSwitchButton(value);
            }
        }

        public cuiColorPicker()
        {
            InitializeComponent();
        }

        public bool isShowingDialog
        {
            get
            {
                return PickerForm != null && PickerForm.Visible;
            }
        }

        public cuiColorPicker(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        public async Task<DialogResult> ShowDialog()
        {
            if (isShowingDialog)
            {
                PickerForm.Close();
            }

            PickerForm = new ColorPickerForm() { Theme = this.Theme };
            PickerForm?.ToggleThemeSwitchButton(privateEnableThemeChangeButton);
            PickerForm.Show();

            bool canExitLoop = false;

            PickerForm.FormClosing += (s, e) =>
            {
                Color = PickerForm.ColorVal;
                canExitLoop = true;
            };

            while (!canExitLoop)
            {
                await Task.Delay(100);
            }

            return PickerForm.DialogResult;
        }

        private Themes privateTheme = Themes.Light;
        public Themes Theme
        {
            get
            {
                return privateTheme;
            }
            set
            {
                privateTheme = value;
            }
        }
    }
}

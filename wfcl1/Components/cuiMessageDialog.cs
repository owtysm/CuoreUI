using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CuoreUI.Components.Forms;

namespace CuoreUI.Components
{
    [Description("Modern dialog which disables interaction with the form until option is chosen")]
    public partial class cuiMessageDialog : Component
    {
        public cuiMessageDialog()
        {
            InitializeComponent();
        }

        [Category("CuoreUI Dimmer Colors")]
        public Color DimColor { get; set; } = Color.FromArgb(160, 0, 0, 0);

        [Category("CuoreUI Dialog")]
        public Size ButtonSize { get; set; } = new Size(80, 32);

        [Category("CuoreUI Dialog")]
        public Padding DialogPadding { get; set; } = new Padding(20);

        private DialogResult result = DialogResult.None;

        [Category("CuoreUI Dialog Colors")]
        public Color BackColor { get; set; } = Color.White;

        [Category("CuoreUI Dialog Colors")]
        public Color ForeColor { get; set; } = Color.Black;

        [Category("CuoreUI Dialog")]
        [Description("Height automatically adjusts if the text is too big.")]
        public Size DialogSize { get; set; } = new Size(300, 200);

        [Category("CuoreUI Dialog")]
        [Description("How rounded should the dialog box be?")]
        public int Rounding { get; set; } = 8;

        //

        [Category("CuoreUI Dialog Text")]
        public string OKText { get; set; } = "OK";

        [Category("CuoreUI Dialog Text")]
        public string YesText { get; set; } = "Yes";

        [Category("CuoreUI Dialog Text")]
        public string NoText { get; set; } = "No";

        [Category("CuoreUI Dialog Text")]
        public string CancelText { get; set; } = "Cancel";

        //

        public async Task<DialogResult> ShowDialog(Form parentForm, string description, string title = "")
        {
            return await ShowDialog(parentForm, description, title, MessageBoxButtons.OK);
        }

        public async Task<DialogResult> ShowDialog(Form parentForm, string description, string title, MessageBoxButtons messageBoxButtons)
        {
            using (MessageDialog md = new MessageDialog())
            {
                md.OKText = OKText;
                md.CancelText = CancelText;
                md.YesText = YesText;
                md.NoText = NoText;

                md.Rounding = Rounding;
                md.Size = DialogSize;
                md.DimColor = DimColor;
                md.DialogPadding = DialogPadding;
                md.DialogResult = result;
                md.BackColor = BackColor;
                md.ForeColor = ForeColor;

                return await md.ShowDialog(parentForm, description, title, messageBoxButtons, DialogSize, ButtonSize);
            }
        }
    }
}

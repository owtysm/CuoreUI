using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using CuoreUI.Controls;

namespace CuoreUI.Components.Forms
{
    public partial class MessageDialog : Form
    {
        public Color DimColor { get; set; } = Color.FromArgb(160, 0, 0, 0);
        public System.Drawing.Font ButtonFont { get; set; } = SystemFonts.DefaultFont;
        public Size ButtonSize { get; set; } = new Size(80, 32);
        public Padding DialogPadding { get; set; } = new Padding(20);

        private DialogResult result = DialogResult.None;

        private Form parentForm;
        private Form dimmerForm;
        private Form dialogForm;

        public int Rounding { get; set; }

        public string OKText = "OK";
        public string YesText = "Yes";
        public string NoText = "No";
        public string CancelText = "Cancel";

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);

            if (dialogForm != null && !dialogForm.IsDisposed)
            {
                dialogForm.BackColor = BackColor;
            }
        }

        public MessageDialog()
        {
            InitializeComponent();
        }

        [Obsolete("Use ShowDialog(parent, text) instead.", true)]
        new public void ShowDialog()
        {
            throw new ArgumentNullException("parent parameter not specified in cuiMessageDialog.ShowDialog()! Use cuiMessageDialog.ShowDialog(parent, text) constructor instead!");
        }

        FormBorderStyle parentBorderStyleBefore;
        bool rounderExists = false;

        public Task<DialogResult> ShowDialog(Form parent, string text, string title, MessageBoxButtons buttons, Size targetSize)
        {
            var tcs = new TaskCompletionSource<DialogResult>();

            var dimmer = new Form
            {
                BackColor = Color.FromArgb(255, DimColor),
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                Owner = parent,
                ShowInTaskbar = false,
                Opacity = DimColor.A / 255d,
                TopMost = true
            };

            string[] buttonsText = { OKText, YesText, NoText, CancelText };

            var dialog = new MessageDialog(text, title, buttons, parent, targetSize, buttonsText)
            {
                parentForm = parentForm,
                BackColor = this.BackColor,
                ForeColor = this.ForeColor,
                Owner = dimmer,
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                ShowInTaskbar = false,
                MinimizeBox = false,
                MaximizeBox = false,
                TopMost = true
            };

            bool parentEnabledBefore = parent.Enabled;
            parentBorderStyleBefore = parent.FormBorderStyle;
            bool parentTopMostBefore = parent.TopMost;
            bool parentMaximized = parent.WindowState == FormWindowState.Maximized;

            int dimmerRounding = 0;
            if (!parentMaximized)
            {
                cuiFormRounder rounderFound = FormsRegisteredByRounder.GetRounderByForm(parent);
                rounderExists = rounderFound != null;

                bool automaticallyFindRounding = Helper.Win32.IsWindows11() == false || rounderExists;
                dimmerRounding = parentBorderStyleBefore == FormBorderStyle.None ? 0 : 8;
                if (automaticallyFindRounding)
                {
                    if (rounderExists)
                    {
                        dimmerRounding = rounderFound.Rounding;
                    }
                }
            }

            // Environment.OSVersion.Version is unreliable
            var rounder = new cuiFormRounder
            {
                TargetForm = dialog,
                Rounding = Rounding
            };
            var rounder2 = new cuiFormRounder
            {
                TargetForm = dimmer,
                OutlineColor = Color.Transparent,
                Rounding = parentMaximized ? 0 : dimmerRounding,
            };

            dialog.Opacity = 0.01d;
            dialog.Location = new Point(1 - dialog.Width, 1 - dialog.Height);
            dialog.Show(dimmer);

            RECT frameRect;
            DwmGetWindowAttribute(parent.Handle, DWMWA_EXTENDED_FRAME_BOUNDS, out frameRect, Marshal.SizeOf(typeof(RECT)));
            dimmer.Bounds = frameRect.ToRectangle();
            if (!rounderExists)
            {
                dimmer.Size += new Size(1, 1);
            }
            dialog.Location = new Point(dimmer.Location.X + (dimmer.Width / 2 - dialog.Width / 2), dimmer.Location.Y + (dimmer.Height / 2 - dialog.Height / 2));

            parentForm = parent;
            dimmerForm = dimmer;
            dialogForm = dialog;

            Timer dialogFixerTimer = new Timer() { Interval = 1000 };
            dialogFixerTimer.Tick += (_, __) =>
            {
                ParentLocationChanged(this, EventArgs.Empty);
                ParentSizeChanged(this, EventArgs.Empty);
            };

            parent.LocationChanged += ParentLocationChanged;
            parent.SizeChanged += ParentSizeChanged;

            dialog.FormClosed += (_, __) =>
            {
                dialogFixerTimer.Stop();
                parent.LocationChanged -= ParentLocationChanged;
                parent.SizeChanged -= ParentSizeChanged;

                tcs.TrySetResult(dialog.result);

                dimmer.Hide();
                dialog.Hide();

                parent.Focus();

                //rounder.Dispose();
                rounder2.Dispose();
                dialog.Dispose();
                dimmer.Close();

                parent.TopMost = parentTopMostBefore;
                parent.Enabled = parentEnabledBefore;
                parent.FormBorderStyle = parentBorderStyleBefore;

                parent.BringToFront();
                parent.Focus();
            };

            dialogFixerTimer.Start();

            parent.TopMost = true;
            parent.Enabled = false;
            if (parentBorderStyleBefore != FormBorderStyle.None)
            {
                parent.FormBorderStyle = FormBorderStyle.FixedSingle;
            }

            dimmer.Show();

            dialog.Owner = dimmer;
            dialog.Opacity = 1;

            rounder.roundedFormObj.Owner = dimmer;
            rounder.roundedFormObj.TopMost = true;

            return tcs.Task;
        }

        private void ParentSizeChanged(object sender, EventArgs e)
        {
            if (parentForm == null || dimmerForm == null || dialogForm == null || parentForm.IsDisposed || dimmerForm.IsDisposed || dialogForm.IsDisposed)
            {
                return;
            }

            RECT frameRect;
            DwmGetWindowAttribute(parentForm.Handle, DWMWA_EXTENDED_FRAME_BOUNDS, out frameRect, Marshal.SizeOf(typeof(RECT)));
            Size newsize = Size = frameRect.ToRectangle().Size;

            if (!rounderExists)
            {
                newsize += new Size(1, 1);
            }

            dimmerForm.Size = newsize;

            dialogForm.Location = new Point(dimmerForm.Location.X + (dimmerForm.Width / 2 - dialogForm.Width / 2), dimmerForm.Location.Y + (dimmerForm.Height / 2 - dialogForm.Height / 2));
        }

        private void ParentLocationChanged(object sender, EventArgs e)
        {
            if (parentForm == null || dimmerForm == null || dialogForm == null || parentForm.IsDisposed || dimmerForm.IsDisposed || dialogForm.IsDisposed)
            {
                return;
            }

            RECT frameRect;
            DwmGetWindowAttribute(parentForm.Handle, DWMWA_EXTENDED_FRAME_BOUNDS, out frameRect, Marshal.SizeOf(typeof(RECT)));
            dimmerForm.Location = frameRect.ToRectangle().Location;
        }

        // Doing this because
        // form.Bounds returns too big of an area (includes invisible resize hitboxes)
        // form.ClientRectangle is too little of an area (excludes the title bar)
        // GetWindowRect returned the same as form.Bounds
        // 

        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

        const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int Left, Top, Right, Bottom;
            public Rectangle ToRectangle() => Rectangle.FromLTRB(Left, Top, Right, Bottom);
        }

        //

        private MessageDialog(string text, string title, MessageBoxButtons buttons, Form parent, Size targetSize, string[] buttonsText)
        {
            SuspendLayout();

            Text = title;
            Padding = DialogPadding;
            Size = targetSize;

            OKText = buttonsText[0];
            YesText = buttonsText[1];
            NoText = buttonsText[2];
            CancelText = buttonsText[3];

            using (Graphics a = CreateGraphics())
            using (StringFormat sf = new StringFormat())
            {
                var titleLabel = new cuiLabel
                {
                    Content = title,
                    HorizontalAlignment = StringAlignment.Near,
                    VerticalAlignment = StringAlignment.Near,
                    Font = new Font(Font.FontFamily, Font.Size + 4, FontStyle.Regular),
                };
                titleLabel.Height = (int)a.MeasureString(title, titleLabel.Font).Height + 10;
                titleLabel.Dock = DockStyle.Top;

                var textLabel = new cuiLabel
                {
                    Content = text,
                    HorizontalAlignment = StringAlignment.Near,
                    VerticalAlignment = StringAlignment.Center,
                };
                sf.Alignment = textLabel.HorizontalAlignment;
                sf.LineAlignment = textLabel.VerticalAlignment;
                textLabel.Height = (int)a.MeasureString(text, textLabel.Font, textLabel.Width, sf).Height;
                textLabel.Dock = DockStyle.Top;

                var buttonHolder = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.RightToLeft,
                    Dock = DockStyle.Bottom,
                    Padding = new Padding(0),
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink
                };

                // prevents the buttons were overlapping with the textLabel
                int heightDiff = buttonHolder.Top - textLabel.Bottom;
                if (heightDiff < 0)
                {
                    int newSize = Math.Min(Height + heightDiff, parent.Height);
                    //MessageBox.Show($"{Height} | {newSize}");
                    Height = newSize;
                }

                void AddButton(string buttonText, DialogResult res)
                {
                    var btn = new cuiButton
                    {
                        NormalBackground = Color.Transparent,
                        HoverBackground = Color.Transparent,
                        PressedBackground = Color.Transparent,

                        NormalForeColor = Color.Gray,
                        HoverForeColor = Color.Gray,
                        PressedForeColor = Color.FromArgb(128, Color.Gray),

                        Content = buttonText,
                        Font = ButtonFont,
                        Size = ButtonSize,
                        DialogResult = DialogResult.OK
                    };
                    btn.Click += (_, __) => { result = res; Close(); };
                    buttonHolder.Controls.Add(btn);
                }

                switch (buttons)
                {
                    case MessageBoxButtons.OK:
                        AddButton(OKText, DialogResult.OK);
                        break;
                    case MessageBoxButtons.OKCancel:
                        AddButton(CancelText, DialogResult.Cancel);
                        AddButton(OKText, DialogResult.OK);
                        break;
                    case MessageBoxButtons.YesNo:
                        AddButton(NoText, DialogResult.No);
                        AddButton(YesText, DialogResult.Yes);
                        break;
                    case MessageBoxButtons.YesNoCancel:
                        AddButton(NoText, DialogResult.No);
                        AddButton(YesText, DialogResult.Yes);
                        AddButton(CancelText, DialogResult.Cancel);
                        break;
                }

                Controls.Add(textLabel);
                Controls.Add(titleLabel);
                Controls.Add(buttonHolder);
            }

            ResumeLayout(true);
            PerformLayout();
        }
    }
}

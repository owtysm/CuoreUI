using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CuoreUI.Components
{
    [ToolboxBitmap(typeof(Form))]
    public partial class cuiFormDrag : Component
    {
        private Form targetForm;
        public cuiFormDrag(IContainer container)
        {
            container.Add(this);
        }

        [Category("CuoreUI")]
        public Form TargetForm
        {
            get
            {
                return targetForm;
            }
            set
            {
                if (targetForm != null)
                {
                    targetForm.MouseMove -= MouseMove;
                }

                targetForm = value;

                if (targetForm != null)
                {
                    targetForm.MouseMove += MouseMove;
                }
            }
        }

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(TargetForm.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }
    }
}

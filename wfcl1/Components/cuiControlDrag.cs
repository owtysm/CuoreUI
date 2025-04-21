using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CuoreUI
{
    [ToolboxBitmap(typeof(Button))]
    public partial class cuiControlDrag : Component
    {
        private Control targetControl;
        private Point previousMousePosition;
        private Form parentForm;

        public cuiControlDrag(IContainer container)
        {
            container.Add(this);
        }

        public Control TargetControl
        {
            get => targetControl;
            set
            {
                if (targetControl != null)
                {
                    targetControl.MouseDown -= MouseDown;
                    targetControl.MouseMove -= MouseMove;
                }

                targetControl = value;

                if (targetControl != null)
                {
                    targetControl.MouseDown += MouseDown;
                    targetControl.MouseMove += MouseMove;
                }
            }
        }

        private void MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                previousMousePosition = Cursor.Position;
                parentForm = targetControl?.FindForm();
            }
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && parentForm != null)
            {
                Point currentMousePosition = Cursor.Position;
                int deltaX = currentMousePosition.X - previousMousePosition.X;
                int deltaY = currentMousePosition.Y - previousMousePosition.Y;

                parentForm.Left += deltaX;
                parentForm.Top += deltaY;

                previousMousePosition = currentMousePosition;
            }
        }
    }
}

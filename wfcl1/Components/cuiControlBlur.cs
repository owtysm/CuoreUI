using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CuoreUI.Components
{
    [ToolboxBitmap(typeof(Panel))]
    public partial class cuiControlBlur : Component
    {
        private Bitmap cachedBitmap;
        public cuiControlBlur(IContainer container)
        {
            container.Add(this);
        }

        private Control privateTargetControl;
        public Control TargetControl
        {
            get
            {
                return privateTargetControl;
            }
            set
            {
                if (TargetControl is Form || value is Form || value == null)
                {
                    privateTargetControl = null;
                    cachedBitmap?.Dispose();
                    cachedBitmap = null;
                    if ((Debugger.IsAttached || DesignMode) && value != null)
                    {
                        MessageBox.Show($"Cannot set TargetControl to type Form in this cuiControlBlur instance.\nBlurring the whole form would be too expensive for winforms, sorry.", "CuoreUI");
                    }
                    return;
                }

                if (privateTargetControl != null)
                {
                    privateTargetControl.Paint -= TargetControl_Paint;
                    privateTargetControl.Invalidated -= TargetControl_Invalidated;
                }
                value.Parent?.Refresh();

                privateTargetControl = value;
                if (privateTargetControl != null)
                {
                    privateTargetControl.Paint += TargetControl_Paint;
                    privateTargetControl.Invalidated += TargetControl_Invalidated;
                }
                cachedBitmap?.Dispose();
                cachedBitmap = null;
                privateTargetControl?.Invalidate();
            }
        }

        private float privateBlurAmount = 1.5f;
        public float BlurAmount
        {
            get
            {
                return privateBlurAmount;
            }
            set
            {
                if (value > 0)
                {
                    privateBlurAmount = value;
                }
                cachedBitmap?.Dispose();
                cachedBitmap = null;
                privateTargetControl?.Refresh();
            }
        }

        private void TargetControl_Invalidated(object sender, InvalidateEventArgs e)
        {
            cachedBitmap?.Dispose();
            cachedBitmap = null;
        }

        private unsafe void TargetControl_Paint(object sender, PaintEventArgs e)
        {
            //e.Graphics.FillRectangle(new SolidBrush(TargetControl.BackColor), TargetControl.ClientRectangle);
            if (cachedBitmap == null || cachedBitmap.Width != privateTargetControl.Width || cachedBitmap.Height != privateTargetControl.Height)
            {
                cachedBitmap?.Dispose();
                cachedBitmap = new Bitmap(privateTargetControl.Width, privateTargetControl.Height);
                using (Graphics g = Graphics.FromImage(cachedBitmap))
                {

                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                    g.PixelOffsetMode = PixelOffsetMode.None;
                    g.InterpolationMode = InterpolationMode.Low;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;

                    privateTargetControl.DrawToBitmap(cachedBitmap, new Rectangle(0, 0, privateTargetControl.Width, privateTargetControl.Height));

                    //GaussianBlur.Apply(ref cachedBitmap, BlurAmount);
                    Drawing.Imaging.ImageBlurs.QuadraticBlur.Apply(ref cachedBitmap, BlurAmount);

                }
            }

            e.Graphics.DrawImage(cachedBitmap, privateTargetControl.ClientRectangle);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                cachedBitmap = null;
                cachedBitmap?.Dispose();
                TargetControl.Paint -= TargetControl_Paint;
                TargetControl.Invalidated -= TargetControl_Invalidated;
                TargetControl.Invalidate();
                TargetControl = null;
            }
            base.Dispose(disposing);
        }
    }
}

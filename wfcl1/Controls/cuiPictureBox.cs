using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Color = System.Drawing.Color;
using Image = System.Drawing.Image;
using Matrix = System.Drawing.Drawing2D.Matrix;
using Pen = System.Drawing.Pen;

namespace CuoreUI.Controls
{
    [ToolboxBitmap(typeof(PictureBox))]
    public partial class cuiPictureBox : UserControl
    {
        public cuiPictureBox()
        {
            InitializeComponent();
            // double buffer removes flickering when animating opacity and/or location with cuiControlAnimator
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        Image cachedImage = null;
        private Image privateContent = null;

        [Category("CuoreUI")]
        public Image Content
        {
            get
            {
                return privateContent;
            }
            set
            {
                if (value != null)
                {
                    privateContent = value;
                    cachedImage = null;
                    cachedImageBrush = null;
                    TintImage();
                }
                else
                {
                    privateContent = null;
                }
                Invalidate();
            }
        }

        private void DiposeIfPossible(ref Image target)
        {
            try
            {
                target?.Dispose();
            }
            catch {; }
            target = null;
        }

        private void DiposeIfPossible(ref TextureBrush target)
        {
            try
            {
                target?.Image?.Dispose();
                target?.Dispose();
            }
            catch { }
            target = null;
        }

        private Padding privateCornerRadius = new Padding(8, 8, 8, 8);

        [Category("CuoreUI")]
        public Padding Rounding
        {
            get
            {
                return privateCornerRadius;
            }
            set
            {
                privateCornerRadius = value;
                Invalidate();
            }
        }

        private void TintImage()
        {
            if (privateContent == null)
                return;

            Bitmap tintedBitmap = new Bitmap(privateContent.Width, privateContent.Height);

            using (Graphics graphics = Graphics.FromImage(tintedBitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                float r = ImageTint.R / 255f;
                float g = ImageTint.G / 255f;
                float b = ImageTint.B / 255f;
                float a = ImageTint.A / 255f;

                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
                    new float[] {r, 0, 0, 0, 0},
                    new float[] {0, g, 0, 0, 0},
                    new float[] {0, 0, b, 0, 0},
                    new float[] {0, 0, 0, a, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

                ImageAttributes imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                try
                {
                    graphics.DrawImage(privateContent, new Rectangle(0, 0, privateContent.Width, privateContent.Height),
                                0, 0, privateContent.Width, privateContent.Height,
                                GraphicsUnit.Pixel, imageAttributes);
                }
                catch (InvalidOperationException)
                {

                }
            }

            cachedImageBrush = new TextureBrush(tintedBitmap, WrapMode.Clamp);
            cachedImage = tintedBitmap;
        }

        private TextureBrush cachedImageBrush = null;

        private Color privateImageTint = Color.White;

        [Category("CuoreUI")]
        public Color ImageTint
        {
            get
            {
                return privateImageTint;
            }
            set
            {
                privateImageTint = value;
                TintImage();
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (cachedImage == null)
            {
                DiposeIfPossible(ref privateContent);
                DiposeIfPossible(ref cachedImage);
                DiposeIfPossible(ref cachedImageBrush);
                return;
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Rectangle fixedCR = ClientRectangle;
            fixedCR.Inflate(-1, -1);

            using (Pen pen = new Pen(PanelOutlineColor, OutlineThickness))
            using (GraphicsPath roundBg = Helper.RoundRect(fixedCR, Rounding))
            {
                GenerateTransformMatrix();

                e.Graphics.FillPath(cachedImageBrush, roundBg);
                e.Graphics.DrawPath(pen, roundBg);
            }

            base.OnPaint(e);
        }

        private int privateRotation = 0;

        [Category("CuoreUI")]
        public int Rotation
        {
            get
            {
                return privateRotation;
            }
            set
            {
                if (privateRotation != value)
                {
                    privateRotation = value % 360;
                    Content = Content; // because refresh doesnt transform image but Content's setter does
                }
            }
        }

        // empty to not affect already existing projects that use cuoreui and have a picturebox
        private Color privatePanelOutlineColor = Color.Empty;

        [Category("CuoreUI")]
        public Color PanelOutlineColor
        {
            get
            {
                return privatePanelOutlineColor;
            }
            set
            {
                privatePanelOutlineColor = value;
                Invalidate();
            }
        }

        private float privateOutlineThickness = 1;

        [Category("CuoreUI")]
        public float OutlineThickness
        {
            get
            {
                return privateOutlineThickness;
            }
            set
            {
                privateOutlineThickness = value;
                Invalidate();
            }
        }

        private void GenerateTransformMatrix()
        {
            if (cachedImageBrush.Image == null)
            {
                return;
            }

            Size imageSize = cachedImageBrush.Image.Size;

            float scaleX = (float)Width / imageSize.Width;
            float scaleY = (float)Height / imageSize.Height;

            using (Matrix matrix = new Matrix())
            {
                matrix.RotateAt(Rotation, new PointF(Width / 2f, Height / 2f));
                matrix.Scale(scaleX, scaleY);
                cachedImageBrush.Transform = matrix;
            }
        }

        private void cuiPictureBox_Resize(object sender, EventArgs e)
        {
            if (Content != null)
            {
                Content = Content;
            }
        }
    }
}

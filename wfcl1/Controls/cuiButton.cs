using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Image = System.Drawing.Image;

namespace CuoreUI.Controls
{
    [ToolboxBitmap(typeof(Button))]
    [DefaultEvent("Click")]
    public partial class cuiButton : UserControl
    {

        public static class States
        {
            public const int Normal = 1;
            public const int Hovered = 2;
            public const int Pressed = 3;
        }

        public cuiButton()
        {
            InitializeComponent();
            DoubleBuffered = true;
            ForeColor = Color.White;
            Font = new Font("Microsoft Sans Serif", 9.75f);
        }

        private DialogResult privateDialogResult = DialogResult.None;
        public DialogResult DialogResult
        {
            get
            {
                return privateDialogResult;
            }
            set
            {
                privateDialogResult = value;
                Refresh();
            }
        }

        private string privateContent = "Your text here!";
        public string Content
        {
            get
            {
                return privateContent;
            }
            set
            {
                privateContent = value;
                Invalidate();
            }
        }

        private Padding privateRounding = new Padding(8, 8, 8, 8);
        public Padding Rounding
        {
            get
            {
                return privateRounding;
            }
            set
            {
                privateRounding = value;
                Invalidate();
            }
        }

        private Color privateNormalBackground = CuoreUI.Drawing.PrimaryColor;
        public Color NormalBackground
        {
            get
            {
                return privateNormalBackground;
            }
            set
            {
                privateNormalBackground = value;
                Invalidate();
            }
        }

        private Color privateHoverBackground = Color.FromArgb(200, 255, 106, 0);
        public Color HoverBackground
        {
            get
            {
                return privateHoverBackground;
            }
            set
            {
                privateHoverBackground = value;
                Invalidate();
            }
        }

        private Color privatePressedBackground = CuoreUI.Drawing.PrimaryColor;
        public Color PressedBackground
        {
            get
            {
                return privatePressedBackground;
            }
            set
            {
                privatePressedBackground = value;
                Invalidate();
            }
        }

        private Color privateNormalOutline = Color.Empty;
        public Color NormalOutline
        {
            get
            {
                return privateNormalOutline;
            }
            set
            {
                privateNormalOutline = value;
                Invalidate();
            }
        }

        private Color privateHoverOutline = Color.Empty;
        public Color HoverOutline
        {
            get
            {
                return privateHoverOutline;
            }
            set
            {
                privateHoverOutline = value;
                Invalidate();
            }
        }

        private Color privatePressedOutline = Color.Empty;
        public Color PressedOutline
        {
            get
            {
                return privatePressedOutline;
            }
            set
            {
                privatePressedOutline = value;
                Invalidate();
            }
        }

        private bool privateCheckButton = false;
        public bool CheckButton
        {

            get
            {
                return privateCheckButton;
            }
            set
            {
                privateCheckButton = value;
                Invalidate();
            }
        }

        private bool privateChecked = false;
        public bool Checked
        {
            get
            {
                return privateChecked;
            }
            set
            {
                privateChecked = value;
                Invalidate();
            }
        }

        private int state = 1;
        private SolidBrush privateBrush = new SolidBrush(Color.Black);
        private Pen privatePen = new Pen(Color.Black);
        StringFormat stringFormat = new StringFormat() { Alignment = StringAlignment.Center };

        private Image privateImage = null;
        public Image Image
        {
            get
            {
                return privateImage;
            }
            set
            {
                privateImage = value;
                Invalidate();
            }
        }


        private Color privateCheckedBackground = CuoreUI.Drawing.PrimaryColor;
        public Color CheckedBackground
        {
            get
            {
                return privateCheckedBackground;
            }
            set
            {
                privateCheckedBackground = value;
                Invalidate();
            }
        }

        private Color privateCheckedOutline = CuoreUI.Drawing.PrimaryColor;
        public Color CheckedOutline
        {
            get
            {
                return privateCheckedOutline;
            }
            set
            {
                privateCheckedOutline = value;
                Invalidate();
            }
        }

        private bool privateImageAutoCenter = true;

        public bool ImageAutoCenter
        {
            get
            {
                return privateImageAutoCenter;
            }
            set
            {
                privateImageAutoCenter = value;
                Invalidate();
            }
        }

        private float privateOutlineThickness = 1.6f;
        public float OutlineThickness
        {
            get
            {
                return privateOutlineThickness;
            }
            set
            {
                privateOutlineThickness = Math.Max(value, 0);
                privatePen.Width = value;
            }
        }

        private Point privateImageExpand = Point.Empty;
        public Point ImageExpand
        {
            get
            {
                return privateImageExpand;
            }
            set
            {
                privateImageExpand = value;
                Invalidate();
            }
        }

        Color privateCheckedForeColor = Color.White;
        public Color CheckedForeColor
        {
            get
            {
                return privateCheckedForeColor;
            }
            set
            {
                privateCheckedForeColor = value;
                Refresh();
            }
        }

        Color privatePressedForeColor = Color.White;
        public Color PressedForeColor
        {
            get
            {
                return privatePressedForeColor;
            }
            set
            {
                privatePressedForeColor = value;
                Refresh();
            }
        }

        public Color NormalForeColor
        {
            get
            {
                return ForeColor;
            }
            set
            {
                ForeColor = value;
            }
        }

        Color privateHoverForeColor = Color.White;
        public Color HoverForeColor
        {
            get
            {
                return privateHoverForeColor;
            }
            set
            {
                privateHoverForeColor = value;
                Refresh();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            Rectangle modifiedCR = ClientRectangle;
            modifiedCR.Width -= 1;
            modifiedCR.Height -= 1;

            GraphicsPath roundBackground = Helper.RoundRect(modifiedCR, Rounding);

            Color renderedBackgroundColor = Color.Empty;
            Color renderedOutlineColor = Color.Empty;
            Color renderedTint = NormalImageTint;
            Color renderedForeColor = Color.Empty;

            if (CheckButton && Checked)
            {
                renderedBackgroundColor = CheckedBackground;
                renderedOutlineColor = CheckedOutline;
                renderedTint = CheckedImageTint;
            }
            else
            {
                switch (state)
                {
                    case States.Normal:
                        renderedBackgroundColor = NormalBackground;
                        renderedOutlineColor = NormalOutline;
                        renderedForeColor = NormalForeColor;
                        break;

                    case States.Hovered:
                        renderedBackgroundColor = HoverBackground;
                        renderedOutlineColor = HoverOutline;
                        renderedTint = HoveredImageTint;
                        renderedForeColor = HoverForeColor;
                        break;

                    case States.Pressed:
                        renderedBackgroundColor = PressedBackground;
                        renderedOutlineColor = PressedOutline;
                        renderedTint = PressedImageTint;
                        renderedForeColor = PressedForeColor;
                        break;
                }
            }

            privateBrush.Color = renderedBackgroundColor;
            privatePen.Color = renderedOutlineColor;

            e.Graphics.FillPath(privateBrush, roundBackground);

            if (OutlineThickness > 0)
            {
                e.Graphics.DrawPath(privatePen, roundBackground);
            }

            Rectangle textRectangle = ClientRectangle;
            int textY = (Height / 2) - (Font.Height / 2);
            textRectangle.Location = new Point(0, textY);

            Rectangle imageRectangle = textRectangle;
            imageRectangle.Height = Font.Height;
            imageRectangle.Width = imageRectangle.Height;
            imageRectangle.Inflate(ImageExpand.X, ImageExpand.Y);

            if (ImageAutoCenter && privateImage != null)
            {
                imageRectangle.X = (Width / 2) - (imageRectangle.Width / 2);
                int TextOffsetFromImage = (int)e.Graphics.MeasureString(Content, Font, textRectangle.Width, stringFormat).Width;
                imageRectangle.X -= TextOffsetFromImage / 2;
                textRectangle.X += imageRectangle.Width / 2;
            }

            textRectangle.Offset(privateTextOffset);
            imageRectangle.Offset(privateImageOffset);

            using (SolidBrush brush = new SolidBrush(renderedForeColor))
            {
                e.Graphics.DrawString(privateContent, Font, brush, textRectangle, stringFormat);
            }

            if (privateImage != null)
            {
                float tintR = renderedTint.R / 255f;
                float tintG = renderedTint.G / 255f;
                float tintB = renderedTint.B / 255f;
                float tintA = renderedTint.A / 255f;

                // Create a color matrix that will apply the tint color
                ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                {
            new float[] {tintR, 0, 0, 0, 0},
            new float[] {0, tintG, 0, 0, 0},
            new float[] {0, 0, tintB, 0, 0},
            new float[] {0, 0, 0, tintA, 0},
            new float[] {0, 0, 0, 0, 1}
                });

                // Create image attributes and set the color matrix
                ImageAttributes imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(colorMatrix);

                // Draw the image with the tint
                e.Graphics.DrawImage(
                    privateImage,
                    imageRectangle,
                    0, 0, privateImage.Width, privateImage.Height,
                    GraphicsUnit.Pixel,
                    imageAttributes);
            }

            base.OnPaint(e);
        }

        private Color privateImageTint = Color.White;
        public Color NormalImageTint
        {
            get
            {
                return privateImageTint;
            }
            set
            {
                privateImageTint = value;
                Invalidate();
            }
        }

        private Color privateHoveredImageTint = Color.White;
        public Color HoveredImageTint
        {
            get
            {
                return privateHoveredImageTint;
            }
            set
            {
                privateHoveredImageTint = value;
                Invalidate();
            }
        }

        private Color privateCheckedImageTint = Color.White;
        public Color CheckedImageTint
        {
            get
            {
                return privateCheckedImageTint;
            }
            set
            {
                privateCheckedImageTint = value;
                Invalidate();
            }
        }

        private Color privatePressedImageTint = Color.White;
        public Color PressedImageTint
        {
            get
            {
                return privatePressedImageTint;
            }
            set
            {
                privatePressedImageTint = value;
                Invalidate();
            }
        }

        private Point privateImageOffset = new Point(0, 0);
        public Point ImageOffset
        {
            get
            {
                return privateImageOffset;
            }
            set
            {
                privateImageOffset = value;
                Invalidate();
            }
        }

        private Point privateTextOffset = new Point(0, 0);
        public Point TextOffset
        {
            get
            {
                return privateTextOffset;
            }
            set
            {
                privateTextOffset = value;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (ClientRectangle.Contains(e.Location))
            {
                if (state == 3)
                {
                    if (CheckButton)
                    {
                        Checked = !Checked;
                    }
                }

                if (state != 1)
                {
                    state = 2;
                }
                Invalidate();
            }
            else
            {
                state = 1;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            state = 1;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            state = 2;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            state = 3;

            if (privateDialogResult != DialogResult.None)
            {
                Form parentForm = FindForm();
                if (parentForm != null)
                {
                    parentForm.DialogResult = privateDialogResult;
                }
            }

            Focus();
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            state = 1;
            base.OnLostFocus(e);
        }
    }
}

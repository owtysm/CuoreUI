using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CuoreUI.Properties;

namespace CuoreUI.Controls
{
    [Description("Lets the user select a file or drop it onto the control")]
    [DefaultEvent("FileDropped")]
    public partial class cuiFileDropper : Control
    {
        private bool hover = false;
        private readonly StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        [Category("CuoreUI")]
        public bool Multiselect { get; set; } = false;

        private Color privatePanelColor = Color.FromArgb(16, 255, 255, 255);

        [Category("CuoreUI")]
        public Color PanelColor
        {
            get
            {
                return privatePanelColor;
            }
            set
            {
                privatePanelColor = value;
                Invalidate();
            }
        }

        private Color privatePanelOutlineColor = Color.FromArgb(128, 128, 128, 128);

        [Category("CuoreUI")]
        public Color DashedOutlineColor
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

        private bool privateDashedOutline = true;

        [Category("CuoreUI")]
        public bool DashedOutline
        {
            get
            {
                return privateDashedOutline;
            }
            set
            {
                privateDashedOutline = value;
                Invalidate();
            }
        }

        private int privateDashLength = 8;

        [Category("CuoreUI")]
        public int DashLength
        {
            get
            {
                return privateDashLength;
            }
            set
            {
                privateDashLength = value;
                Invalidate();
            }
        }

        private Padding privateRounding = new Padding(8, 8, 8, 8);

        [Category("CuoreUI")]
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

        public cuiFileDropper()
        {
            InitializeComponent();
            AllowDrop = true;
            ForeColor = Color.Gray;
            Cursor = Cursors.Hand;
            Size = new Size(270, 135);
        }

        [Category("CuoreUI")]
        public string NormalContent { get; set; } = "Drop file here";

        [Category("CuoreUI")]
        public string HoverContent { get; set; } = "Release to drop";

        [Category("CuoreUI")]
        [Description("Example: txt files (*.txt)|*.txt|All files (*.*)|*.*\nLeave empty for all file extensions.")]
        public string Filter { get; set; } = "";

        private Color privateHoverForeColor = Color.FromArgb(128, 128, 128, 128);

        [Category("CuoreUI")]
        public Color HoverForeColor
        {
            get => privateHoverForeColor;
            set { privateHoverForeColor = value; Invalidate(); }
        }

        [Category("CuoreUI")]
        public Color NormalForeColor
        {
            get => ForeColor;
            set { ForeColor = value; Invalidate(); }
        }

        private Color privateHoverUploadForeColor = CuoreUI.Drawing.PrimaryColor;

        [Category("CuoreUI")]
        public Color HoverUploadForeColor
        {
            get => privateHoverUploadForeColor;
            set { privateHoverUploadForeColor = value; Invalidate(); }
        }

        private Color privateForeUploadColor = CuoreUI.Drawing.PrimaryColor;

        [Category("CuoreUI")]
        public Color NormalUploadForeColor
        {
            get => privateForeUploadColor;
            set { privateForeUploadColor = value; Invalidate(); }
        }

        private bool privateClickToUpload = true;

        [Category("CuoreUI")]
        public bool UploadWithClick
        {
            get
            {
                return privateClickToUpload;
            }
            set
            {
                privateClickToUpload = value;
                Refresh();
            }
        }

        [Category("CuoreUI")]
        public string UploadContent { get; set; } = "Click to upload";

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Rectangle modifiedCR = ClientRectangle;
            modifiedCR.Width -= 1;
            modifiedCR.Height -= 1;

            modifiedCR.Inflate(-(int)(OutlineThickness), -(int)(OutlineThickness));

            using (GraphicsPath roundBackground = Helper.RoundRect(modifiedCR, Rounding))
            using (SolidBrush brush = new SolidBrush(PanelColor))
            using (Pen pen = new Pen(DashedOutlineColor, OutlineThickness) { DashStyle = DashedOutline ? DashStyle.Dash : DashStyle.Solid })
            using (SolidBrush textBrush = new SolidBrush(hover ? HoverForeColor : NormalForeColor))
            {
                if (DashedOutline)
                {
                    pen.DashStyle = DashStyle.Custom;
                    pen.DashPattern = new float[] { DashLength, DashLength };
                }

                e.Graphics.FillPath(brush, roundBackground);
                e.Graphics.DrawPath(pen, roundBackground);

                string line1 = hover ? HoverContent : NormalContent;
                string line2 = UploadWithClick ? (hover ? UploadContent : UploadContent) : null;

                SizeF size1 = e.Graphics.MeasureString(line1, Font);
                SizeF size2 = line2 != null ? e.Graphics.MeasureString(line2, Font) : SizeF.Empty;

                float totalHeight = size1.Height + (line2 != null ? size2.Height : 0f);
                float startY = modifiedCR.Top + (modifiedCR.Height - totalHeight) / 2;

                if (privateImage != null)
                {
                    int imageHalfHeight = privateImageSize.Height / 2;
                    int halfPadding = ImagePadding / 2;

                    Rectangle imageRectangle = new Rectangle(
                        Width / 2 - privateImageSize.Width / 2,
                        (int)(startY - imageHalfHeight - halfPadding),
                        privateImageSize.Width,
                        privateImageSize.Height
                    );
                    e.Graphics.DrawImage(privateImage, imageRectangle);

                    float tintR = ImageTint.R / 255f;
                    float tintG = ImageTint.G / 255f;
                    float tintB = ImageTint.B / 255f;
                    float tintA = ImageTint.A / 255f;

                    ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                    {
        new float[] {tintR, 0, 0, 0, 0},
        new float[] {0, tintG, 0, 0, 0},
        new float[] {0, 0, tintB, 0, 0},
        new float[] {0, 0, 0, tintA, 0},
        new float[] {0, 0, 0, 0, 1}
                    });

                    ImageAttributes imageAttributes = new ImageAttributes();
                    imageAttributes.SetColorMatrix(colorMatrix);

                    e.Graphics.DrawImage(
                        privateImage,
                        imageRectangle,
                        0, 0, privateImage.Width, privateImage.Height,
                        GraphicsUnit.Pixel,
                        imageAttributes
                    );

                    int imageRectHalfHeight = imageRectangle.Height / 2;

                    RectangleF textRect1 = new RectangleF(
                        modifiedCR.Left,
                        startY + imageRectHalfHeight + halfPadding,
                        modifiedCR.Width,
                        size1.Height
                    );
                    e.Graphics.DrawString(line1, Font, textBrush, textRect1, sf);

                    if (line2 != null)
                    {
                        using (SolidBrush uploadTextBrush = new SolidBrush(hover ? HoverUploadForeColor : NormalUploadForeColor))
                        {
                            RectangleF textRect2 = new RectangleF(
                                modifiedCR.Left,
                                startY + size1.Height + imageRectHalfHeight + halfPadding,
                                modifiedCR.Width,
                                size2.Height
                            );
                            e.Graphics.DrawString(line2, Font, uploadTextBrush, textRect2, sf);
                        }
                    }
                }
                else
                {
                    RectangleF textRect1 = new RectangleF(modifiedCR.Left, startY, modifiedCR.Width, size1.Height);
                    e.Graphics.DrawString(line1, Font, textBrush, textRect1, sf);

                    if (line2 != null)
                    {
                        using (SolidBrush uploadTextBrush = new SolidBrush(hover ? HoverUploadForeColor : NormalUploadForeColor))
                        {
                            RectangleF textRect2 = new RectangleF(modifiedCR.Left, startY + size1.Height, modifiedCR.Width, size2.Height);
                            e.Graphics.DrawString(line2, Font, uploadTextBrush, textRect2, sf);
                        }
                    }
                }
            }

            base.OnPaint(e);
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
                drgevent.Effect = DragDropEffects.Copy;
        }
        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
            {
                drgevent.Effect = DragDropEffects.Copy;
                bool alreadyHovering = hover;
                hover = true;

                if (alreadyHovering != hover)
                {
                    Refresh();
                }
            }
        }
        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);
            hover = false;
            Refresh();
        }

        private Image privateImage = Resources.ic_fluent_folder_add_24_regular;

        [Category("CuoreUI")]
        public Image Image
        {
            get
            {
                return privateImage;
            }
            set
            {
                privateImage = value;
                Refresh();
            }
        }

        private Size privateImageSize = new Size(24, 24);

        [Category("CuoreUI")]
        public Size ImageSize
        {
            get
            {
                return privateImageSize;
            }
            set
            {
                privateImageSize = value;
                Refresh();
            }
        }

        private Color privateImageColor = Color.Gray;

        [Category("CuoreUI")]
        public Color ImageTint
        {
            get
            {
                return privateImageColor;
            }
            set
            {
                privateImageColor = value;
                Refresh();
            }
        }

        private int privateImagePadding = 2;

        [Category("CuoreUI")]
        public int ImagePadding
        {
            get
            {
                return privateImagePadding;
            }
            set
            {
                privateImagePadding = value;
                Refresh();
            }
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);
            hover = false;
            Refresh();

            object Data = drgevent.Data.GetData(DataFormats.FileDrop);
            if (Data is string[] fileList)
            {
                if (fileList != null && fileList.Length > 0)
                {
                    string[] AllowedFileExtensions = GetExtensionsFromFilter();

                    var validFiles = (AllowedFileExtensions.Length > 0
                        ? fileList.Where(f => AllowedFileExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                        : fileList);

                    // cuiFileDropper specific
                    validFiles = validFiles.Where(f => !f.EndsWith("\\") && File.Exists(f));

                    FileNames = validFiles.ToArray();
                    FileName = FileNames[0];

                    if (FileNames.Length > 1)
                    {
                        FileDropped?.Invoke(this, new FileDroppedEventArgs(FileNames));
                    }
                    else
                    {
                        FileDropped?.Invoke(this, new FileDroppedEventArgs(FileName));
                    }
                }
            }
        }

        public string[] GetExtensionsFromFilter()
        {
            if (string.IsNullOrWhiteSpace(Filter))
                return Array.Empty<string>();

            var extensions = Filter
                .Split('|')
                .Where((_, index) => index % 2 == 1)
                .SelectMany(part => part.Split(';'))
                .Select(ext => ext.TrimStart('*').ToLowerInvariant())
                .Where(ext => !string.IsNullOrWhiteSpace(ext))
                .Distinct()
                .ToArray();

            return extensions.Contains(".*") ? Array.Empty<string>() : extensions;
        }

        public string FileName { get; private set; }
        public string[] FileNames { get; private set; }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (UploadWithClick)
            {
                bool MultiselectNow = Multiselect;
                using (OpenFileDialog ofd = new OpenFileDialog() { Filter = Filter, Multiselect = MultiselectNow })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        if (MultiselectNow)
                        {
                            FileNames = ofd.FileNames;
                            FileDropped?.Invoke(this, new FileDroppedEventArgs(FileNames));
                        }
                        else
                        {
                            FileName = ofd.FileName;
                            FileDropped?.Invoke(this, new FileDroppedEventArgs(FileName));
                        }
                    }
                }
            }
        }

        [Category("CuoreUI")]
        public event EventHandler<FileDroppedEventArgs> FileDropped;
    }

    public class FileDroppedEventArgs : EventArgs
    {
        public FileDroppedEventArgs(string fileName)
        {
            FileName = fileName;
            OneFileDropped = true;
        }

        public FileDroppedEventArgs(string[] fileNames)
        {
            FileNames = fileNames;

            if (fileNames.Length == 1)
            {
                FileName = fileNames[0];
                OneFileDropped = true;
            }
        }

        public bool OneFileDropped { get; private set; } = false;
        public string FileName;
        public string[] FileNames { get; } = Array.Empty<string>();
    }
}

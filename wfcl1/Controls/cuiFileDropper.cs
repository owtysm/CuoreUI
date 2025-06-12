using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

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
                        FileDropped?.Invoke(null, new FileDroppedEventArgs(FileNames));
                    }
                    else
                    {
                        FileDropped?.Invoke(null, new FileDroppedEventArgs(FileName));
                    }

                }
            }
        }

        public string[] GetExtensionsFromFilter()
        {
            var extensions = Filter
                .Split('|')
                .Where((_, index) => index % 2 == 1) // pick only extensions
                .Select(ext => ext.StartsWith("*") ? ext.Substring(1) : ext)
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
                            FileDropped?.Invoke(null, new FileDroppedEventArgs(FileNames));
                        }
                        else
                        {
                            FileName = ofd.FileName;
                            FileDropped?.Invoke(null, new FileDroppedEventArgs(FileName));
                        }
                    }
                }
            }
        }

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

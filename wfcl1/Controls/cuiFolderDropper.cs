using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CuoreUI.Controls
{
    [DefaultEvent("FolderDropped")]
    public partial class cuiFolderDropper : Control
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

        private float privateOutlineThickness = 1.5f;
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

        private int privateDashLength = 4;
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

        public cuiFolderDropper()
        {
            InitializeComponent();
            AllowDrop = true;
            ForeColor = Color.Gray;
            Cursor = Cursors.Hand;
        }

        [Category("CuoreUI")]
        public string NormalContent { get; set; } = "Drop folder here";

        [Category("CuoreUI")]
        public string HoverContent { get; set; } = "Release to drop";

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
                    // cuiFolderDropper specific
                    var validFiles = fileList.Where(f => Directory.Exists(f));

                    FolderNames = validFiles.ToArray();
                    FolderName = FolderNames[0];

                    if (FolderNames.Length > 1)
                    {
                        FolderDropped?.Invoke(null, new FolderDroppedEventArgs(FolderNames));
                    }
                    else
                    {
                        FolderDropped?.Invoke(null, new FolderDroppedEventArgs(FolderName));
                    }

                }
            }
        }

        [Category("CuoreUI")]
        public string FolderName { get; private set; }

        [Category("CuoreUI")]
        public string[] FolderNames { get; private set; }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (UploadWithClick)
            {
                bool MultiselectNow = Multiselect;

                OpenFolderDialog ofd = new OpenFolderDialog() { Multiselect = MultiselectNow };

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (MultiselectNow)
                    {
                        FolderNames = ofd.ResultPaths.ToArray();
                        FolderDropped?.Invoke(null, new FolderDroppedEventArgs(FolderNames));
                    }
                    else
                    {
                        FolderName = ofd.ResultPath;
                        FolderDropped?.Invoke(null, new FolderDroppedEventArgs(FolderName));
                    }
                }

            }
        }

        [Category("CuoreUI")]
        public event EventHandler<FolderDroppedEventArgs> FolderDropped;
    }

    public class FolderDroppedEventArgs : EventArgs
    {
        public FolderDroppedEventArgs(string folderName)
        {
            FolderName = folderName;
            OneFolderDropped = true;
        }

        public FolderDroppedEventArgs(string[] folderNames)
        {
            FolderNames = folderNames;

            if (folderNames.Length == 1)
            {
                FolderName = folderNames[0];
                OneFolderDropped = true;
            }
        }

        public bool OneFolderDropped { get; private set; } = false;
        public string FolderName;
        public string[] FolderNames { get; } = Array.Empty<string>();
    }
}

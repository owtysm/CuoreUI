using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace CuoreUI.Components
{
    [Description("Shows a badge next to your form's taskbar icon")]
    public partial class cuiTaskbarBadgeController : Component
    {
        public cuiTaskbarBadgeController()
        {
            InitializeComponent();
        }

        private Form privateTargetForm;

        [Category("CuoreUI")]
        public Form TargetForm
        {
            get => privateTargetForm;
            set
            {
                if (privateTargetForm != null)
                {
                    privateTargetForm.Shown -= PrivateTargetForm_Shown;
                }

                privateTargetForm = value;

                if (privateTargetForm != null)
                {
                    privateTargetForm.Shown += PrivateTargetForm_Shown;
                }
            }
        }

        private void PrivateTargetForm_Shown(object sender, EventArgs e)
        {
            if (Image != null && !DesignMode)
            {
                SetOverlayIcon(privateImage, privateDescription);
            }
        }

        private Image privateImage;

        [Category("CuoreUI")]
        public Image Image
        {
            get => privateImage;
            set
            {
                if (privateImage != null)
                {
                    privateImage.Dispose();
                    privateImage = null;
                }

                if (value == null || (value.Width == 16 && value.Height == 16))
                {
                    privateImage = value;

                    if (Image != null && !DesignMode)
                    {
                        SetOverlayIcon(privateImage, privateDescription);
                    }
                    else if (Image == null)
                    {
                        ClearOverlayIcon();
                    }
                }
                else
                {
                    Image resizedBitmap = new Bitmap(16, 16);

                    using (Graphics g = Graphics.FromImage(resizedBitmap))
                    {
                        // Set the interpolation mode to high quality for better resizing
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                        g.DrawImage(value, 0, 0, 16, 16);
                    }

                    value?.Dispose();
                    privateImage = resizedBitmap;

                    if (Image != null && !DesignMode)
                    {
                        SetOverlayIcon(privateImage, privateDescription);
                    }
                }
            }
        }

        private string privateDescription = "";

        [Category("CuoreUI")]
        public string Description
        {
            get
            {
                return privateDescription;
            }
            set
            {
                privateDescription = value;

                if (Image != null && !DesignMode)
                {
                    SetOverlayIcon(privateImage, privateDescription);
                }
                else if (Image == null)
                {
                    ClearOverlayIcon();
                }
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private void SetOverlayIcon(Image inputImage, string description)
        {
            if (TaskbarManager.IsPlatformSupported && TargetForm != null && TargetForm.IsDisposed == false)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    inputImage.Save(ms, ImageFormat.Png);
                    using (var iconBitmap = new Bitmap(ms))
                    {
                        IntPtr hIcon = iconBitmap.GetHicon();
                        using (var icon = Icon.FromHandle(hIcon))
                        {
                            var clonedIcon = (Icon)icon.Clone();
                            TaskbarManager.Instance.SetOverlayIcon(TargetForm.Handle, clonedIcon, description);
                            clonedIcon.Dispose();
                        }
                        DestroyIcon(hIcon);
                    }
                }
            }
        }

        private void ClearOverlayIcon()
        {
            if (TaskbarManager.IsPlatformSupported && TargetForm != null && TargetForm.IsDisposed == false)
            {
                TaskbarManager.Instance.SetOverlayIcon(TargetForm.Handle, null, null);
            }
        }

        private int privateNumericValue = 0;

        [Category("CuoreUI")]

        [Description("0 or <0 will NOT show in the Taskbar.")]
        public int NumericValue
        {
            get
            {
                return privateNumericValue;
            }
            set
            {
                privateNumericValue = value;
                if (value <= 0 && Image != null)
                {
                    SetOverlayIcon(Image, Description);
                }
                else if (value <= 0 && Image == null)
                {
                    ClearOverlayIcon();
                }
                else
                {
                    using (Image numericImage = GenerateDynamicNumericBadge())
                    {
                        SetOverlayIcon(numericImage, Description);
                    }
                }
            }
        }

        [Category("CuoreUI")]
        public Color NumericBackColor { get; set; } = Color.Red;

        [Category("CuoreUI")]
        public Color NumericForeColor { get; set; } = Color.White;

        [Category("CuoreUI")]
        public Font NumericFont { get; set; } = new Font("Microsoft YaHei UI", 8, FontStyle.Bold);

        private Image GenerateDynamicNumericBadge()
        {
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Draw background circle
                using (Brush back = new SolidBrush(NumericBackColor))
                {
                    g.FillEllipse(back, 0, 0, 16, 16);
                }

                // Draw centered number
                using (Brush fore = new SolidBrush(NumericForeColor))
                using (StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    // 16x16 but more horizontal space to avoid drawstring issues
                    RectangleF rect = new RectangleF(-16, 0, 48, 16);
                    if (NumericValue > 9)
                    {
                        using (Font moreNotificationsFont = new Font(NumericFont.FontFamily, 7, FontStyle.Bold))
                        {
                            rect.Offset(1, -1);

                            // smaller font to fit the '9+'
                            g.DrawString("9+", moreNotificationsFont, fore, rect, sf);
                            g.DrawString("9+", moreNotificationsFont, fore, rect, sf);
                        }
                    }
                    else
                    {
                        g.DrawString(NumericValue.ToString(), NumericFont, fore, rect, sf);
                    }
                }
            }

            return bmp;
        }
    }
}
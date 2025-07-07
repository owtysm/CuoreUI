﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using CuoreUI.Controls.Forms;
using CuoreUI.Properties;
using static CuoreUI.Controls.Forms.DatePicker;

namespace CuoreUI.Controls
{
    [Description("Allows the user to select a date with the custom CuoreUI date picker form")]
    public partial class cuiCalendarDatePicker : UserControl
    {
        public cuiCalendarDatePicker()
        {
            InitializeComponent();
            ForeColor = Color.Gray;
            Font = new Font(Font.FontFamily, 9.75f);
            Size = new Size(153, 45);
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }

        private Themes privateTheme = Themes.Light;

        [Category("CuoreUI")]
        public Themes Theme
        {
            get
            {
                return privateTheme;
            }
            set
            {
                privateTheme = value;
            }
        }

        private bool privateEnableThemeChangeButton = true;

        [Category("CuoreUI")]
        [Description("Lets the USER toggle the theme between Light and Dark with a button.")]
        public bool EnableThemeChangeButton
        {
            get
            {
                return privateEnableThemeChangeButton;
            }
            set
            {
                privateEnableThemeChangeButton = value;
                _PickerForm?.ToggleThemeSwitchButton(value);
            }
        }

        DatePicker _PickerForm;
        public bool isDialogVisible = false;

        [Category("CuoreUI")]
        [Description("Where the picker should show up, relative to where the control's bounds.")]
        public enum Position
        {
            Top = 1,
            Left = 2,
            Bottom = 3,
            Right = 4,

            TopLeft = 5,
            TopRight = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        [Category("CuoreUI")]
        public Position PickerPosition { get; set; } = Position.Bottom;

        public void ShowDialog()
        {
            if (isDialogVisible)
            {
                return;
            }

            isDialogVisible = true;
            DatePicker PickerForm = new DatePicker(Content);
            _PickerForm = PickerForm;

            PickerForm.Theme = Theme;
            PickerForm?.ToggleThemeSwitchButton(privateEnableThemeChangeButton);

            Point basePoint = FindForm().Location + ((Size)Location);
            int rounding = PickerForm.cuiFormRounder1.Rounding;
            Size pickerSize = PickerForm.Size;

            Point location;

            switch (PickerPosition)
            {
                case Position.Top:
                    location = basePoint + new Size(Width / 2 + rounding, rounding * 4) - new Size(pickerSize.Width / 2, pickerSize.Height);
                    break;

                case Position.Left:
                    location = basePoint + new Size(rounding, Height / 2 + rounding * 4) - new Size(pickerSize.Width, pickerSize.Height / 2);
                    break;

                case Position.Bottom:
                    location = basePoint + new Size(Width / 2 + rounding, Height + rounding * 4) - new Size(pickerSize.Width / 2, 0);
                    break;

                case Position.Right:
                    location = basePoint + new Size(Width + rounding, Height / 2 + rounding * 4) - new Size(0, pickerSize.Height / 2);
                    break;

                case Position.TopLeft:
                    location = basePoint + new Size(rounding, rounding * 4) - pickerSize;
                    break;

                case Position.TopRight:
                    location = basePoint + new Size(Width + rounding, rounding * 4) - new Size(0, pickerSize.Height);
                    break;

                case Position.BottomLeft:
                    location = basePoint + new Size(rounding, Height + rounding * 4) - new Size(pickerSize.Width, 0);
                    break;

                case Position.BottomRight:
                    location = basePoint + new Size(Width + rounding, Height + rounding * 4);
                    break;

                default: // same as bottom
                    location = basePoint + new Size(Width / 2 + rounding, Height + rounding * 4) - new Size(pickerSize.Width / 2, 0);
                    break;
            }

            PickerForm.Location = location;

            PickerForm.Show();

            PickerForm.FormClosing += (s, e) =>
            {
                _PickerForm = null;
                if (PickerForm.DialogResult == DialogResult.OK)
                {
                    Content = PickerForm.Value;
                }
                isDialogVisible = false;
            };
        }

        protected override void OnClick(EventArgs e)
        {
            Focus();
            ShowDialog();
            base.OnClick(e);
        }

        private int privateRounding = 8;

        [Category("CuoreUI")]
        public int Rounding
        {
            get
            {
                return privateRounding;
            }
            set
            {
                privateRounding = value;
                Refresh();
            }
        }

        private Color privateBackgroundColor = Color.FromArgb(32, 128, 128, 128);

        [Category("CuoreUI")]
        public Color NormalBackground
        {
            get
            {
                return privateBackgroundColor;
            }
            set
            {
                privateBackgroundColor = value;
                Refresh();
            }
        }

        private Color privateHoverBackground = Color.FromArgb(50, 128, 128, 128);

        [Category("CuoreUI")]
        public Color HoverBackground
        {
            get => privateHoverBackground;
            set
            {
                privateHoverBackground = value;
                Refresh();
            }
        }

        private Color privatePressedBackground = Color.FromArgb(80, 128, 128, 128);

        [Category("CuoreUI")]
        public Color PressedBackground
        {
            get => privatePressedBackground;
            set
            {
                privatePressedBackground = value;
                Refresh();
            }
        }

        private Color privateNormalOutline = Color.FromArgb(150, 128, 128, 128);

        [Category("CuoreUI")]
        public Color NormalOutline
        {
            get => privateNormalOutline;
            set
            {
                privateNormalOutline = value;
                Refresh();
            }
        }

        private Color privateHoverOutline = Color.FromArgb(180, 128, 128, 128);

        [Category("CuoreUI")]
        public Color HoverOutline
        {
            get => privateHoverOutline;
            set
            {
                privateHoverOutline = value;
                Refresh();
            }
        }

        private Color privatePressedOutline = Color.FromArgb(210, 128, 128, 128);

        [Category("CuoreUI")]
        public Color PressedOutline
        {
            get => privatePressedOutline;
            set
            {
                privatePressedOutline = value;
                Refresh();
            }
        }

        private float privateOutlineThickness = 1.5f;

        [Category("CuoreUI")]
        public float OutlineThickness
        {
            get => privateOutlineThickness;
            set
            {
                privateOutlineThickness = Math.Max(0, value);
                Refresh();
            }
        }

        private bool privateShowIcon = true;

        [Category("CuoreUI")]
        public bool ShowIcon
        {
            get
            {
                return privateShowIcon;
            }
            set
            {
                privateShowIcon = value;
                Refresh();
            }
        }

        private Image privateIcon = Resources.calendar;

        [Category("CuoreUI")]
        public Image Icon
        {
            get
            {
                return privateIcon;
            }
            set
            {
                privateIcon = value;
                Refresh();
            }
        }

        private Color privateImageTint = Color.Gray;

        [Category("CuoreUI")]
        public Color IconTint
        {
            get
            {
                return privateImageTint;
            }
            set
            {
                privateImageTint = value;
                Refresh();
            }
        }

        private Image TintIcon(Image icon)
        {
            if (icon == null)
                return null;

            Bitmap tintedBitmap = new Bitmap(icon.Width, icon.Height);

            using (Graphics graphics = Graphics.FromImage(tintedBitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                float r = IconTint.R / 255f;
                float g = IconTint.G / 255f;
                float b = IconTint.B / 255f;
                float a = IconTint.A / 255f;

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

                graphics.DrawImage(icon, new Rectangle(0, 0, icon.Width, icon.Height),
                            0, 0, icon.Width, icon.Height,
                            GraphicsUnit.Pixel, imageAttributes);
            }

            return tintedBitmap;
        }

        private DateTime privateValue = DateTime.Now.Date;

        [Category("CuoreUI")]
        public DateTime Content
        {
            get
            {
                return privateValue;
            }
            set
            {
                privateValue = new DateTime(value.Year, value.Month, value.Day);
                DateChanged?.Invoke(this, EventArgs.Empty);
                Refresh();
            }
        }

        [Category("CuoreUI")]
        public event EventHandler DateChanged;

        StringFormat stringFormat = new StringFormat() { Alignment = StringAlignment.Center };

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            int halfHeight = Height / 2;
            int quartHeight = halfHeight / 2;
            int iconYOffset = -1 + halfHeight - (Font.Height / 2);
            int textStartX = -1 + quartHeight + halfHeight;
            int textStartY = iconYOffset;

            Rectangle fixedCR = ClientRectangle;
            fixedCR.Width -= 1;
            fixedCR.Height -= 1;

            Color currentBackground = isPressed ? PressedBackground : (isHovered ? HoverBackground : NormalBackground);
            Color currentOutline = isPressed ? PressedOutline : (isHovered ? HoverOutline : NormalOutline);

            using (GraphicsPath bgPath = Helper.RoundRect(fixedCR, privateRounding))
            using (SolidBrush br = new SolidBrush(currentBackground))
            using (Pen outlinePen = new Pen(currentOutline, OutlineThickness))
            {
                e.Graphics.FillPath(br, bgPath);
                e.Graphics.DrawPath(outlinePen, bgPath);
            }

            if (ShowIcon && privateIcon != null)
            {
                Rectangle textRectangle = new Rectangle(1 + (textStartX / 4) * 3, textStartY, Width - textStartX, Font.Height);
                using (SolidBrush textBrush = new SolidBrush(ForeColor))
                {
                    e.Graphics.DrawString(Content.ToShortDateString(), Font, textBrush, textRectangle, stringFormat);
                }

                int textWidth = e.Graphics.MeasureString(Content.ToShortDateString(), Font).ToSize().Width;
                Rectangle iconRect = new Rectangle(Width / 2 - textWidth / 2 - textWidth / 8, iconYOffset, Font.Height, Font.Height);
                using (Image tintedIcon = TintIcon(privateIcon))
                {
                    e.Graphics.DrawImage(tintedIcon, iconRect);
                }
            }
            else
            {
                Rectangle textRectangle = new Rectangle(0, textStartY, Width, Height);
                using (SolidBrush textBrush = new SolidBrush(ForeColor))
                {
                    e.Graphics.DrawString(Content.ToShortDateString(), Font, textBrush, textRectangle, stringFormat);
                }
            }

            base.OnPaint(e);
        }

        private bool isHovered = false;
        private bool isPressed = false;

        protected override void OnMouseEnter(EventArgs e)
        {
            isHovered = true;
            Refresh();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            isHovered = false;
            isPressed = false;
            Refresh();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            isPressed = true;
            Refresh();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            isPressed = false;
            Refresh();
            base.OnMouseUp(e);
        }
    }
}

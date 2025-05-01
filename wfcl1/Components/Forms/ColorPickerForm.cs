using System;
using System.Drawing;
using System.Windows.Forms;
using CuoreUI.Controls;
using static CuoreUI.Helper.Win32;

namespace CuoreUI.Components.Forms
{
    public partial class ColorPickerForm : Form
    {
        public ColorPickerForm()
        {
            InitializeComponent();
            initTimer();
        }

        private void initTimer()
        {
            Timer timer = new Timer();
            timer.Interval = 16; // wont bother with refresh rates for this (for now)
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private bool currentlyChangingColor = false;
        private bool updateHexTextbox = true;

        private Color privateColorVal = Color.Empty;
        public Color ColorVal
        {
            get
            {
                return privateColorVal;
            }
            set
            {
                currentlyChangingColor = true;

                privateColorVal = value;
                cuiBorder1.PanelColor = value;

                cuiTextBox1.Content = value.A + "";
                cuiTextBox2.Content = value.R + "";
                cuiTextBox3.Content = value.G + "";
                cuiTextBox4.Content = value.B + "";

                if (updateHexTextbox)
                {
                    cuiTextBox5.Content = ColorToHex(value);
                }

                colorPickerWheel1.UpdatePos();

                currentlyChangingColor = false;
            }
        }

        private string ColorToHex(Color value)
        {
            return $"#{value.A:X2}{value.R:X2}{value.G:X2}{value.B:X2}";
        }

        private bool isInsideColorPicker()
        {
            return colorPickerWheel1.ClientRectangle.Contains(colorPickerWheel1.PointToClient(Cursor.Position));
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (isInsideColorPicker() && isClickingLeftMouse())
            {
                ExtractColorFromClickPoint(sender, new MouseEventArgs(MouseButtons.Left, 1, Cursor.Position.X, Cursor.Position.Y, 1));
            }
        }

        static Color GetColorAtCursor()
        {
            // win32 directly instead of making bitmaps for *speed*
            IntPtr hdc = GetDC(IntPtr.Zero);

            GetCursorPos(out POINT p);

            uint pixel = GetPixel(hdc, p.X, p.Y);

            ReleaseDC(IntPtr.Zero, hdc);

            Color color = Color.FromArgb((int)(pixel & 0x000000FF),   // r
                                         (int)(pixel & 0x0000FF00) >> 8,  // g
                                         (int)(pixel & 0x00FF0000) >> 16); // b

            return color;
        }

        private void ExtractColorFromClickPoint(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SetColor(GetColorAtCursor());
            }
        }

        internal void SetColor(Color target, bool arg_updatehex = true)
        {
            updateHexTextbox = arg_updatehex;
            ColorVal = target;
            updateHexTextbox = true;
        }

        private void cuiButton1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ContentChanged(object sender, EventArgs e)
        {
            if ((sender is cuiTextBox) == false || currentlyChangingColor)
            {
                return;
            }

            cuiTextBox textbox = sender as cuiTextBox;

            if (textbox.Content.Trim() == string.Empty)
            {
                return;
            }

            int tempValue;

            int.TryParse(textbox.Content, out tempValue);

            tempValue = Math.Min(255, tempValue);
            tempValue = Math.Max(0, tempValue);

            if (textbox == cuiTextBox1) //alpha
            {
                ColorVal = Color.FromArgb(tempValue, ColorVal.R, ColorVal.G, ColorVal.B);
            }

            if (textbox == cuiTextBox2) //red
            {
                ColorVal = Color.FromArgb(ColorVal.A, tempValue, ColorVal.G, ColorVal.B);
            }

            if (textbox == cuiTextBox3) //green
            {
                ColorVal = Color.FromArgb(ColorVal.A, ColorVal.R, tempValue, ColorVal.B);
            }

            if (textbox == cuiTextBox4) //blue
            {
                ColorVal = Color.FromArgb(ColorVal.A, ColorVal.R, ColorVal.G, tempValue);
            }
        }

        private void hexInput_ContentChanged(object sender, EventArgs e)
        {
            if ((sender is cuiTextBox) == false)
            {
                return;
            }

            try
            {
                Color gotColor = ColorTranslator.FromHtml(cuiTextBox5.Content);
                if (gotColor != Color.Empty)
                {
                    SetColor(gotColor, false);
                }
            }
            catch //not a color, ignore
            {

            }
        }

        private void cuiButton3_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cuiButton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        public enum Themes
        {
            Dark,
            Light
        }

        private Themes privateTheme = Themes.Dark;
        public Themes Theme // i'll handle themes like this for now
        {
            get
            {
                return privateTheme;
            }
            set
            {
                privateTheme = value;
                colorPickerWheel1.Theme = value;
                switch (value)
                {
                    case Themes.Light:
                        BackColor = SystemColors.Control;
                        foreach (Control ct in Controls)
                        {
                            if (ct is cuiTextBox ctb)
                            {
                                ctb.BackColor = BackColor;
                                ctb.ForeColor = Color.Black;
                                ctb.BackgroundColor = Color.White;
                                ctb.FocusBackgroundColor = ctb.BackgroundColor;
                            }
                            else if (ct is cuiLabel cl && cl != cuiLabel3)
                            {
                                cl.ForeColor = Color.Black;
                            }
                        }
                        cuiButton3.NormalOutline = Color.FromArgb(20, 0, 0, 0);

                        cuiButton2.NormalBackground = Color.FromArgb(20, 0, 0, 0);
                        cuiButton2.ForeColor = Color.Black;
                        cuiBorder1.PanelOutlineColor = Color.FromArgb(30, 0, 0, 0);

                        cuiButton1.NormalImageTint = Color.Black;

                        cuiFormRounder1.OutlineColor = Color.FromArgb(30, 0, 0, 0);
                        cuiLabel3.ForeColor = Color.FromArgb(84, 84, 84);
                        break;

                    case Themes.Dark:
                        BackColor = Color.Black;
                        foreach (Control ct in Controls)
                        {
                            if (ct is cuiTextBox ctb)
                            {
                                ctb.BackColor = BackColor;
                                ctb.ForeColor = Color.White;
                                ctb.BackgroundColor = Color.FromArgb(16, 16, 16);
                                ctb.FocusBackgroundColor = ctb.BackgroundColor;
                            }
                            else if (ct is cuiLabel cl && cl != cuiLabel3)
                            {
                                cl.ForeColor = Color.White;
                            }
                        }
                        cuiButton3.NormalOutline = Color.FromArgb(20, 255, 255, 255);

                        cuiButton2.NormalBackground = Color.FromArgb(20, 255, 255, 255);
                        cuiButton2.ForeColor = Color.White;
                        cuiBorder1.PanelOutlineColor = Color.FromArgb(30, 255, 255, 255);

                        cuiButton1.NormalImageTint = Color.White;

                        cuiFormRounder1.OutlineColor = Color.FromArgb(30, 255, 255, 255);
                        cuiLabel3.ForeColor = Color.FromArgb(171, 171, 171);
                        break;
                }
            }
        }

        internal void ToggleThemeSwitchButton(bool value)
        {
            cuiButton4.Visible = value;
        }

        private void cuiButton2_ForeColorChanged(object sender, EventArgs e)
        {
            cuiButton2.HoverForeColor = cuiButton2.ForeColor;
            cuiButton2.PressedForeColor = cuiButton2.ForeColor;

            cuiButton3.ForeColor = cuiButton2.ForeColor;

            cuiButton3.HoverForeColor = cuiButton2.ForeColor;
            cuiButton3.PressedForeColor = cuiButton2.ForeColor;

            cuiButton2.NormalImageTint = cuiButton2.ForeColor;
            cuiButton3.NormalImageTint = cuiButton2.ForeColor;
            cuiButton2.HoveredImageTint = cuiButton2.ForeColor;
            cuiButton3.HoveredImageTint = cuiButton2.ForeColor;
            cuiButton2.PressedImageTint = cuiButton2.ForeColor;
            cuiButton3.PressedImageTint = cuiButton2.ForeColor;
        }

        private void cuiButton4_Click(object sender, EventArgs e)
        {
            if (Theme == Themes.Light)
            {
                Theme = Themes.Dark;
            }
            else
            {
                Theme = Themes.Light;
            }
        }
    }
}

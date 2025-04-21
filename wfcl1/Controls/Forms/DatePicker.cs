using System;
using System.Drawing;
using System.Windows.Forms;
using CuoreUI.Controls.Forms.DatePickerPages;

namespace CuoreUI.Controls.Forms
{
    public partial class DatePicker : Form
    {
        private DateTime privateValue;
        public DateTime Value
        {
            get
            {
                return privateValue;
            }
            set
            {
                privateValue = value;
                cuiLabel3.Content = value.ToString("D");
                yearPickerControl?.UpdateYearButtons();
                monthDayPickerControl?.UpdateDayButtons();
            }
        }

        internal void ToggleThemeSwitchButton(bool value)
        {
            cuiButton4.Visible = value;
        }

        YearDatePicker yearPickerControl;
        MonthDatePicker monthDayPickerControl;

        public DatePicker(DateTime startWithDateTime)
        {
            InitializeComponent();

            yearPickerControl = new YearDatePicker(this);
            monthDayPickerControl = new MonthDatePicker(this);

            Value = startWithDateTime;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            pagePanel.Controls.Add(monthDayPickerControl);
        }

        void SetPage(UserControl pageControl)
        {
            pagePanel.Controls.Clear();
            pagePanel.Controls.Add(pageControl);
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

        internal void SetYear(int year, bool returnToChoosingDay = true)
        {
            int fixedDay = Math.Min(Value.Day, DateTime.DaysInMonth(year, Value.Month));
            Value = new DateTime(year, Value.Month, fixedDay);

            if (returnToChoosingDay)
            {
                SetPage(monthDayPickerControl);
            }
        }

        internal void SetDayMonth(int day, int month)
        {
            int fixedDay = Math.Min(day, DateTime.DaysInMonth(Value.Year, month));
            Value = new DateTime(Value.Year, month, fixedDay);
        }

        private void cuiButton1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void cuiButton5_Click(object sender, EventArgs e)
        {
            if (pagePanel.Controls[0] == monthDayPickerControl)
            {
                SetPage(yearPickerControl);
            }
            else
            {
                SetPage(monthDayPickerControl);
            }
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

                SuspendLayout();

                switch (value)
                {
                    case Themes.Light:
                        BackColor = SystemColors.Control;
                        foreach (Control ct in Controls)
                        {
                            if (ct is cuiTextBox ctb)
                            {
                                ctb.ForeColor = Color.Black;
                                ctb.BackColor = SystemColors.Control;
                                ctb.BorderColor = Color.FromArgb(221, 221, 221);
                            }
                            else if (ct is cuiLabel cl && cl != cuiLabel3)
                            {
                                cl.ForeColor = Color.Black;
                            }
                        }
                        cuiButton3.NormalOutline = Color.FromArgb(20, 0, 0, 0);

                        cuiButton2.NormalBackground = Color.FromArgb(20, 0, 0, 0);
                        cuiButton2.ForeColor = Color.Black;

                        cuiButton1.NormalBackground = Color.FromArgb(20, 0, 0, 0);
                        cuiButton1.NormalImageTint = Color.Black;
                        cuiButton1.HoveredImageTint = cuiButton1.NormalImageTint;
                        cuiButton1.PressedImageTint = cuiButton1.NormalImageTint;

                        cuiFormRounder1.OutlineColor = Color.FromArgb(30, 0, 0, 0);
                        cuiLabel3.ForeColor = Color.FromArgb(84, 84, 84);
                        break;

                    case Themes.Dark:
                        BackColor = Color.Black;
                        foreach (Control ct in Controls)
                        {
                            if (ct is cuiTextBox ctb)
                            {
                                ctb.ForeColor = SystemColors.ButtonFace;
                                ctb.BackColor = Color.Black;
                                ctb.BorderColor = Color.FromArgb(34, 34, 34);
                            }
                            else if (ct is cuiLabel cl && cl != cuiLabel3)
                            {
                                cl.ForeColor = Color.White;
                            }
                        }
                        cuiButton3.NormalOutline = Color.FromArgb(20, 255, 255, 255);

                        cuiButton2.NormalBackground = Color.FromArgb(20, 255, 255, 255);
                        cuiButton2.ForeColor = Color.White;

                        cuiButton1.NormalBackground = Color.FromArgb(20, 255, 255, 255);
                        cuiButton1.NormalImageTint = Color.White;
                        cuiButton1.HoveredImageTint = cuiButton1.NormalImageTint;
                        cuiButton1.PressedImageTint = cuiButton1.NormalImageTint;

                        cuiFormRounder1.OutlineColor = Color.FromArgb(30, 255, 255, 255);
                        cuiLabel3.ForeColor = Color.FromArgb(171, 171, 171);
                        break;
                }

                ResumeLayout();
            }
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
    }
}

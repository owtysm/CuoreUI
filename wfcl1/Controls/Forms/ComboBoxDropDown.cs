using System;
using System.Drawing;
using System.Windows.Forms;

namespace CuoreUI.Controls.Forms
{
    public partial class ComboBoxDropDown : Form
    {
        public cuiComboBox caller;
        public Cursor ButtonCursor = Cursors.Arrow;

        private string[] privateItems = new string[0];
        public string[] Items
        {
            get
            {
                return privateItems;
            }
            set
            {
                privateItems = value;
                parseItems();
            }
        }

        public Color NoItemsForeColor
        {
            get; set;
        } = Color.Gray;

        void parseItems()
        {
            cuiButton[] options = new cuiButton[Items.Length];

            int i = 0;
            int firstItem = 0;
            int lastItem = Items.Length - 1;
            int defaultHeight = 0;
            int defaultWidth = 0;
            foreach (string item in Items)
            {
                if (item.Trim() == string.Empty)
                {
                    lastItem--;
                    continue;
                }

                cuiButton cuiButton = new cuiButton();
                cuiButton.Name = item;
                if (caller == null)
                {
                    cuiButton.Width = Width + cuiFormRounder1.Rounding * 2;
                }
                else
                {
                    cuiButton.Width = Width - 4;
                }

                cuiButton.Cursor = ButtonCursor;
                cuiButton.Content = item;
                cuiButton.Location = new Point(1, 1 + i * cuiButton.Height);
                defaultHeight = cuiButton.Height;
                defaultWidth = cuiButton.Width;

                cuiFormRounder1.Rounding = Rounding.All;

                if (i == firstItem)
                {
                    cuiButton.Rounding = new Padding(Rounding.All, Rounding.All, 0, 0);
                }
                else if (i == lastItem)
                {
                    cuiButton.Rounding = new Padding(0, 0, Rounding.All, Rounding.All);
                }
                else
                {
                    cuiButton.Rounding = new Padding(0);
                }

                if (i == firstItem && i == lastItem)
                {
                    cuiButton.Rounding = Rounding;
                }

                cuiButton.Click += (e, s) =>
                {
                    SelectedItem = item;
                    Visible = false;
                    SelectedIndexChanged?.Invoke(this, EventArgs.Empty);

                    Close();
                };

                options[i] = cuiButton;
                i++;
            }

            SuspendLayout();
            Opacity = 0;
            if (caller == null)
            {
                Size = new Size(Width, 3 + i * defaultHeight);
            }
            else
            {
                Size = new Size(Width, 1 + i * defaultHeight);
            }

            Controls.Clear();

            if (i < 1)
            {
                cuiLabel label = new cuiLabel();
                label.Content = NoSelectionDropdownText;
                label.ForeColor = NoItemsForeColor;
                label.Font = new Font("Segoe UI", 9, FontStyle.Regular, GraphicsUnit.Point);
                label.Width = Width;

                Controls.Add(label);

                Visible = true;
                Opacity = 1;
                ResumeLayout();

                label.Location = new Point(0, -2 + (Height / 2) - (Font.Height / 2));
                label.HorizontalAlignment = cuiLabel.HorizontalAlignments.Center;
            }
            else
            {
                Controls.AddRange(options);
                Visible = true;
                Opacity = 1;
                ResumeLayout();
            }

            Invalidate();
        }

        public void updateButtons()
        {
            foreach (Control ctrl in Controls)
            {
                if (ctrl is cuiButton cb)
                {
                    cb.NormalBackground = NormalBackground;
                    cb.HoverBackground = HoverBackground;
                    cb.PressedBackground = PressedBackground;

                    cb.NormalOutline = NormalOutline;
                    cb.HoverOutline = HoverOutline;
                    cb.PressedOutline = PressedOutline;

                    cb.ForeColor = NormalForeColor;
                    cb.HoverForeColor = HoverForeColor;
                    cb.PressedForeColor = PressedForeColor;
                }
            }
        }

        public event EventHandler SelectedIndexChanged;

        internal void GoTo(Point position)
        {
            Location = position;
        }

        public void SetWidth(int userWidth)
        {
            Width = userWidth - (cuiFormRounder1.Rounding * 2);
        }

        public ComboBoxDropDown(int x, int y)
        {
            InitializeComponent();
            Location = new Point(x, y);
            ShowInTaskbar = false;
        }

        public string NoSelectionDropdownText { get; set; } = "Empty";

        public ComboBoxDropDown(string[] userItems, int userWidth, Color bg, Color outline, cuiComboBox userCaller, int roundingArg, Cursor cursorForButtons, string textWhenNothingSelected, bool visible = true)
        {
            InitializeComponent();

            ButtonCursor = cursorForButtons;
            NoSelectionDropdownText = textWhenNothingSelected;

            Rounding = new Padding(roundingArg, roundingArg, roundingArg, roundingArg);
            cuiFormRounder1.Rounding = Rounding.All;
            Width = userWidth - 3;

            cuiFormRounder1.OutlineColor = outline;
            BackColor = Color.FromArgb(255, bg.R, bg.G, bg.B);
            caller = userCaller;
            Items = userItems;
            ShowInTaskbar = false;

            if (caller == null || !visible)
            {
                Opacity = 0;
                Width -= 4;
            }

            updateButtons();
        }

        public ComboBoxDropDown()
        {
            InitializeComponent();
            ShowInTaskbar = false;
        }

        public int SelectedIndex = 0;
        public string SelectedItem = string.Empty;

        // button properties

        private Padding privateRounding = new Padding(8);
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

        private Color privateHoverBackground = Color.FromArgb(200, 123, 104, 238);
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

        private Color privateNormalForeColor = Color.White;
        public Color NormalForeColor
        {
            get
            {
                return privateNormalForeColor;
            }
            set
            {
                privateNormalForeColor = value;
                Invalidate();
            }
        }


        private Color privateHoverForeColor = Color.White;
        public Color HoverForeColor
        {
            get
            {
                return privateHoverForeColor;
            }
            set
            {
                privateHoverForeColor = value;
                Invalidate();
            }
        }

        private Color privatePressedForeColor = Color.White;
        public Color PressedForeColor
        {
            get
            {
                return privatePressedForeColor;
            }
            set
            {
                privatePressedForeColor = value;
                Invalidate();
            }
        }
    }
}

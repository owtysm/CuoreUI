using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CuoreUI.Controls.Forms;

namespace CuoreUI.Controls
{
    [DefaultEvent("SelectedIndexChanged")]
    [ToolboxBitmap(typeof(ComboBox))]
    public partial class cuiComboBox : UserControl
    {
        private string privateSelectedItem = string.Empty;
        private string[] privateItems = new string[0];

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string[] Items
        {
            get => privateItems;
            set
            {
                privateItems = value;
                Invalidate();
            }
        }

        DateTime lastClosed = DateTime.MinValue;

        public event EventHandler SelectedIndexChanged;
        public Cursor ButtonCursor { get; set; } = Cursors.Arrow;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedItem
        {
            get => privateSelectedItem;
            set
            {
                value = value.Trim();
                if (Items.Contains(value) && privateSelectedItem != value)
                {
                    privateSelectedItem = value;
                    SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                    Refresh();
                }
                else
                {
                    privateSelectedItem = string.Empty;
                    Refresh();
                }
            }
        }

        public cuiComboBox()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);

            ForeColor = Color.Gray;

            Timer timer = new Timer();
            timer.Interval = 100;
            timer.Tick += Timer_Tick;
            timer.Start();

            Drawing.FrameDrawn += dropdownmove;

            GlobalMouseHook.OnGlobalMouseClick += HandleGlobalMouseClick; // Subscribe to global mouse clicks
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
        }

        private void HandleGlobalMouseClick()
        {
            if (DesignMode || tempdropdown == null)
            {
                return;
            }

            // Check if the mouse click is outside this control
            Point mousePosition = Control.MousePosition;
            bool flag1 = this.Bounds.Contains(mousePosition) == false;
            bool flag2 = tempdropdown?.Bounds.Contains(mousePosition) == false;
            if (flag1 && flag2)
            {
                CloseDropDown(null, EventArgs.Empty);
                lastClosed = DateTime.Now;
                Refresh();
            }

            if (GlobalMouseHook.isHooked)
            {
                GlobalMouseHook.Stop();
            }

            isBrowsingOptions = false;
            lastClosed = DateTime.Now;
            Refresh();
            if (GlobalMouseHook.isHooked)
            {
                GlobalMouseHook.Stop();
            }
        }

        private void dropdownmove(object sender, EventArgs e)
        {
            if (tempdropdown != null)
            {
                Point LocationScreen = PointToScreen(Point.Empty);
                int dropdownTop = LocationScreen.Y + Height + 3;
                int dropdownLeft = LocationScreen.X + 3;
                tempdropdown.Location = new Point(dropdownLeft, dropdownTop);
            }
        }

        int timerCountdown = 0;
        int maxCountdown = 50;

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (DesignMode || tempdropdown == null || tempdropdown.IsDisposed)
            {
                return;
            }

            Point pos = Cursor.Position;
            Rectangle dropdownRect = RectangleToScreen(ClientRectangle);
            dropdownRect.Height += Items.Length * 45;

            // Check if the cursor is within the dropdown rectangle
            bool cursorInRectangle = dropdownRect.Contains(pos);

            if (cursorInRectangle)
            {
                // Reset the countdown if the cursor is inside the rectangle
                timerCountdown = 0;
            }
            else
            {
                // Increment the countdown if the cursor is outside the rectangle
                timerCountdown++;
            }

            if (timerCountdown >= maxCountdown)
            {
                // Close if the countdown reaches the maximum value
                timerCountdown = 0;
                IndexChanged(null, EventArgs.Empty);
                CloseDropDown(tempdropdown, EventArgs.Empty);
            }
        }

        private Color privateBackgroundColor = Color.FromArgb(255, 255, 255);
        public Color BackgroundColor
        {
            get
            {
                return privateBackgroundColor;
            }
            set
            {
                privateBackgroundColor = value;
                Invalidate();
            }
        }

        private Color privateOutlineColor = Color.FromArgb(64, 128, 128, 128);
        public Color OutlineColor
        {
            get
            {
                return privateOutlineColor;
            }
            set
            {
                privateOutlineColor = value;
                Invalidate();
            }
        }

        private Color privateDropDownBackgroundColor = Color.White;
        public Color DropDownBackgroundColor
        {
            get
            {
                return privateDropDownBackgroundColor;
            }
            set
            {
                privateDropDownBackgroundColor = value;
                Invalidate();
            }
        }

        private Color privateDropDownOutlineColor = Color.FromArgb(30, 255, 255, 255);
        public Color DropDownOutlineColor
        {
            get
            {
                return privateDropDownOutlineColor;
            }
            set
            {
                privateDropDownOutlineColor = value;
                Invalidate();
            }
        }

        private float privateOutlineThickness = 1;
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

        public bool isBrowsingOptions = false;

        private string privateNoSelectionText = "None";
        public string NoSelectionText
        {
            get
            {
                return privateNoSelectionText;
            }
            set
            {
                privateNoSelectionText = value;
                if (SelectedIndex == -1)
                {
                    Refresh();
                }
            }
        }

        private string privateNoSelectionDropdownText = "Empty";
        public string NoSelectionDropdownText
        {
            get
            {
                return privateNoSelectionDropdownText;
            }
            set
            {
                privateNoSelectionDropdownText = value;
                if (SelectedIndex == -1)
                {
                    Refresh();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (SelectedIndex == -1 && privateSelectedItem != string.Empty)
            {
                privateSelectedItem = string.Empty;
                Refresh();
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            Rectangle cr = ClientRectangle;
            cr.Inflate(-1, -1);

            GraphicsPath roundBackground = Helper.RoundRect(cr, Rounding);

            using (SolidBrush brush = new SolidBrush(BackgroundColor))
            using (Pen pen = new Pen(OutlineColor, OutlineThickness))
            {
                e.Graphics.FillPath(brush, roundBackground);
                e.Graphics.DrawPath(pen, roundBackground);
            }

            StringFormat centerText = new StringFormat();
            centerText.Alignment = StringAlignment.Center;

            string tempItemString = privateSelectedItem;

            if (SelectedItem == "")
            {
                tempItemString = NoSelectionText;
            }

            e.Graphics.DrawString(tempItemString, Font, new SolidBrush(ForeColor), new Point(Width / 2, (Height / 2) - (Font.Height / 2)), centerText);

            //e.Graphics.DrawString(isBrowsingOptions.ToString(), Font, new SolidBrush(ForeColor), new Point(Width / 2, 0), centerText);

            Rectangle expandRect = ClientRectangle;
            expandRect.Width = Height / 2;
            expandRect.X = ClientRectangle.Right - Height / 2;
            expandRect.Height = expandRect.Width;
            expandRect.Offset(-expandRect.Width / 2, expandRect.Height / 2);

            expandRect.Width = expandRect.Width / 2;
            expandRect.X = ClientRectangle.Right - Height / 2;
            expandRect.Height = expandRect.Width;
            expandRect.Offset(-expandRect.Width / 2, (expandRect.Height / 2));

            GraphicsPath expandAvailable;
            if (isBrowsingOptions)
            {
                expandRect.Height -= 2;
                expandRect.Y += 1;
                expandAvailable = Helper.DownArrow(expandRect);
            }
            else
            {
                expandRect.Width -= 2;
                expandAvailable = Helper.LeftArrowtest(expandRect);
            }
            e.Graphics.FillPath(new SolidBrush(ExpandArrowColor), expandAvailable);
            //e.Graphics.DrawRectangle(new Pen(Color.Red), expandRect);
            //e.Graphics.DrawRectangle(new Pen(Color.Green), ClientRectangle);

            base.OnPaint(e);
        }

        private Color privateExpandColor = Color.Gray;
        public Color ExpandArrowColor
        {
            get
            {
                return privateExpandColor;
            }
            set
            {
                privateExpandColor = value;
                Invalidate();
            }
        }

        public void AddItem(string itemToAdd)
        {
            int newLength = privateItems.Length + 1;
            string[] newItemsArray = new string[newLength];

            Array.Copy(privateItems, newItemsArray, privateItems.Length);
            newItemsArray[newLength - 1] = itemToAdd;

            Items = newItemsArray;
        }

        public void RemoveItem(string itemToRemove)
        {
            int indexToRemove = Array.IndexOf(privateItems, itemToRemove);

            if (indexToRemove != -1)
            {
                string[] newItemsArray = new string[privateItems.Length - 1];

                Array.Copy(privateItems, 0, newItemsArray, 0, indexToRemove);
                Array.Copy(privateItems, indexToRemove + 1, newItemsArray, indexToRemove, privateItems.Length - indexToRemove - 1);

                Items = newItemsArray;
            }
        }

        public int SelectedIndex => Array.IndexOf(privateItems, SelectedItem);

        private void cuiComboBox_Click(object sender, EventArgs e)
        {
            if (isBrowsingOptions)
            {
                IndexChanged(null, EventArgs.Empty);
                return;
            }

            if (tempdropdown != null)
            {
                tempdropdown?.Close();
            }

            if (IsDisposed == false)
            {
                try
                {

                    ComboBoxDropDown DropDown = new ComboBoxDropDown(Items, Width, DropDownBackgroundColor, DropDownOutlineColor, this, Rounding, ButtonCursor, NoSelectionDropdownText);
                    DropDown.ButtonCursor = ButtonCursor;

                    DropDown.NormalBackground = ButtonNormalBackground;
                    DropDown.HoverBackground = ButtonHoverBackground;
                    DropDown.PressedBackground = ButtonPressedBackground;

                    DropDown.NormalOutline = ButtonNormalOutline;
                    DropDown.HoverOutline = ButtonHoverOutline;
                    DropDown.PressedOutline = ButtonPressedOutline;

                    DropDown.Rounding = new Padding(Rounding, Rounding, Rounding, Rounding);
                    DropDown?.updateButtons();

                    DropDown?.Invalidate();

                    isBrowsingOptions = true;
                    Refresh();

                    int a = Items.Length * 45;
                    Rectangle clientrect = ClientRectangle;
                    clientrect.Offset(0, clientrect.Height);
                    clientrect.Height = a;

                    // Convert client rectangle to screen coordinates
                    clientrect = RectangleToScreen(clientrect);

                    Point LocationScreen = PointToScreen(Point.Empty);
                    int dropdownTop = LocationScreen.Y + Height + 2;
                    int dropdownLeft = LocationScreen.X + DropDown.cuiFormRounder1.Rounding;
                    DropDown.Location = new Point(dropdownLeft - 1, dropdownTop - 1);
                    DropDown.Size = DropDown.Size + new Size(2, 2);

                    tempdropdown = DropDown;
                    tempdropdown.Rounding = new Padding(Rounding, Rounding, Rounding, Rounding);

                    DropDown.cuiFormRounder1.roundedFormObj.Location = DropDown.Location;
                    DropDown.Width = Width - 4;
                    DropDown?.cuiFormRounder1.roundedFormObj.Invalidate();
                    DropDown.SelectedIndexChanged += IndexChanged;

                    DropDown?.cuiFormRounder1.roundedFormObj.Show();
                    DropDown?.Show();

                    DropDown?.cuiFormRounder1.TargetForm.Invalidate();

                    GlobalMouseHook.Start();
                }
                catch
                {
                    GlobalMouseHook.Stop();
                }
            }
        }

        private void CloseDropDown(object sender, EventArgs e)
        {

            if (tempdropdown != null)
            {
                tempdropdown?.Close();
            }
            if (sender is ComboBoxDropDown dropdown)
            {
                dropdown?.Close();

                isBrowsingOptions = false;
                Refresh();
            }
            else if (sender is null)
            // null sender means either something REALLY wants to close it
            // or the user had clicked off of the dropdown menu and/or control
            {
                if ((lastClosed - DateTime.Now).Seconds < 1)
                {
                    return;
                }
                isBrowsingOptions = false;
                Refresh();
            }
            else
            {
                throw new Exception($"Invalid sender\n{sender}");
            }
            GlobalMouseHook.Stop();
        }

        ComboBoxDropDown tempdropdown;

        private void IndexChanged(object sender, EventArgs e)
        {
            if (tempdropdown != null)
            {
                tempdropdown?.Close();
            }
            if (sender is ComboBoxDropDown dropdown)
            {
                SelectedItem = dropdown?.SelectedItem;

                dropdown?.Close();
                tempdropdown?.Close();

                isBrowsingOptions = false;
                Refresh();

                dropdown = null;
            }
            else if (tempdropdown != null)
            {
                tempdropdown?.cuiFormRounder1.roundedFormObj.Close();
                tempdropdown.cuiFormRounder1.TargetForm = null;
                tempdropdown?.cuiFormRounder1.Dispose();
                tempdropdown?.Dispose();

                tempdropdown?.Close();

                isBrowsingOptions = false;
                Refresh();

                tempdropdown = null;
            }
            else
            {
                throw new Exception($"Invalid sender\n{sender}");
            }
        }

        // dropdown buttons

        public int Rounding
        {
            get; set;
        } = 8;

        public Color ButtonNormalBackground
        {
            get; set;
        } = CuoreUI.Drawing.PrimaryColor;

        public Color ButtonHoverBackground
        {
            get; set;
        } = CuoreUI.Drawing.TranslucentPrimaryColor;

        public Color ButtonPressedBackground
        {
            get; set;
        } = CuoreUI.Drawing.PrimaryColor;

        public Color ButtonNormalOutline
        {
            get; set;
        } = Color.Empty;

        public Color ButtonHoverOutline
        {
            get; set;
        } = Color.Empty;

        public Color ButtonPressedOutline
        {
            get; set;
        } = Color.Empty;
    }
}

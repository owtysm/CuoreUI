using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

// THIS FILE CONTAINS A MODIFIED VERSION OF A TEXTBOX TAKEN FROM:
// https://github.com/RJCodeAdvance/Custom-TextBox-2--Rounded-Placeholder

// ORIGINAL AUTHOR: RJCodeAdvance
// LICENSE: Unlicense (https://unlicense.org/)

// MODIFICATIONS HAVE BEEN MADE TO THE ORIGINAL CODE TO SUIT CuoreUI
// LIKE ADDING CERTAIN PROPERTIES, OR SLIGHTLY MODIFYING HOW THE CONTROL IS DRAWN

namespace CuoreUI.Controls
{
    [ToolboxBitmap(typeof(TextBox))]
    [DefaultEvent("ContentChanged")]
    public partial class cuiTextBox2 : UserControl
    {
        private Color privateBackgroundColor = Color.FromArgb(10, 10, 10);
        private Color privateFocusBackgroundColor = Color.FromArgb(10, 10, 10);

        private Color privateBorderColor = Color.FromArgb(64, 64, 64);
        private Color privateFocusBorderColor = Color.FromArgb(255, 106, 0);
        private int privateBorderSize = 1;
        private bool privateUnderlinedStyle = false;
        private bool privateIsFocused = false;

        private Padding privateBorderRadius = new System.Windows.Forms.Padding(8, 8, 8, 8);
        private string privatePlaceholderText = "";
        private bool privateIsPlaceholder = false;
        private bool privateIsPasswordChar = false;

        public event EventHandler ContentChanged;

        public cuiTextBox2()
        {
            InitializeComponent();
            base.BackColor = BackgroundColor;
            ForeColor = Color.Gray;
            Multiline = false;
            Load += OnLoad;
            GotFocus += OnLoad;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            this.privateIsFocused = false;
        }

        [Category("CuoreUI")]
        public Color BackgroundColor
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

        [Category("CuoreUI")]
        public Color FocusBackgroundColor
        {
            get
            {
                return privateFocusBackgroundColor;
            }
            set
            {
                privateFocusBackgroundColor = value;
            }
        }

        [Category("CuoreUI")]
        public Color BorderColor
        {
            get
            {
                return privateBorderColor;
            }
            set
            {
                privateBorderColor = value;
                Invalidate();
            }
        }

        [Category("CuoreUI")]
        public Color FocusBorderColor
        {
            get
            {
                return privateFocusBorderColor;
            }
            set
            {
                privateFocusBorderColor = value;
            }
        }

        [Category("CuoreUI")]
        public int BorderSize
        {
            get
            {
                return privateBorderSize;
            }
            set
            {
                if (value >= 1)
                {
                    privateBorderSize = value;
                    Invalidate();
                }
            }
        }

        [Category("CuoreUI")]
        public bool UnderlinedStyle
        {
            get
            {
                return privateUnderlinedStyle;
            }
            set
            {
                privateUnderlinedStyle = value;
                Invalidate();
            }
        }

        [Category("CuoreUI")]
        public bool PasswordChar
        {
            get
            {
                return privateIsPasswordChar;
            }
            set
            {
                privateIsPasswordChar = value;
                if (!privateIsPlaceholder)
                    contentTextField.UseSystemPasswordChar = value;
            }
        }

        [Category("CuoreUI")]
        public bool Multiline
        {
            get
            {
                return contentTextField.Multiline;
            }
            set
            {
                contentTextField.Multiline = value;
                placeholderTextField.Multiline = value;
            }
        }

        [Category("CuoreUI")]
        private new Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                value = Color.FromArgb(255, value); // prevent transparency crashes

                base.BackColor = value;
                contentTextField.BackColor = value;
                placeholderTextField.BackColor = value;
            }
        }

        [Category("CuoreUI")]
        public override Color ForeColor
        {
            get
            {
                return contentTextField.ForeColor;
            }
            set
            {
                base.ForeColor = value;
                contentTextField.ForeColor = value;
                contentTextField.Refresh();
            }
        }

        [Category("CuoreUI")]
        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
                contentTextField.Font = value;
                placeholderTextField.Font = value;
            }
        }

        protected string actualText = "";

        [Category("CuoreUI")]
        public string Content
        {
            get
            {
                return actualText;
            }
            set
            {
                actualText = value;
                contentTextField.Text = value;

                SetPlaceholder();
            }
        }

        [Category("CuoreUI")]
        public Padding Rounding
        {
            get
            {
                return privateBorderRadius;
            }
            set
            {
                if (value.All >= 0 || value.All == -1)
                {
                    privateBorderRadius = value;
                    Invalidate();//Redraw control
                }
            }
        }

        [Category("CuoreUI")]
        public Color PlaceholderColor
        {
            get
            {
                return placeholderTextField.ForeColor;
            }
            set
            {
                placeholderTextField.ForeColor = value;
            }
        }

        [Category("CuoreUI")]
        public string PlaceholderText
        {
            get
            {
                return privatePlaceholderText;
            }
            set
            {
                privatePlaceholderText = value;
                SetPlaceholder();
            }
        }

        private Size privateTextOffset = new Size(0, 0);
        [Category("CuoreUI")]
        public Size TextOffset
        {
            get
            {
                return privateTextOffset;
            }
            set
            {
                privateTextOffset = value;
                Refresh();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            placeholderTextField.Visible = privateIsPlaceholder;
            Graphics graph = e.Graphics;
            Padding newPadding;

            if (Multiline)
            {
                int b = (Rounding.All / 2) + (Font.Height / 8);
                newPadding = new Padding(Font.Height, b, Font.Height, b);
            }
            else
            {
                int newTextboxY = (Height / 2) - (Font.Height / 2);
                if (newTextboxY < 0)
                {
                    newTextboxY = -newTextboxY;
                }
                newPadding = new Padding(Font.Height, newTextboxY, Font.Height, 0);
            }

            newPadding.Left += TextOffset.Width;
            newPadding.Right += TextOffset.Width;
            newPadding.Top += TextOffset.Height;
            newPadding.Bottom += TextOffset.Height;

            Padding = newPadding;

            if (privateBorderRadius.All > 1 || privateBorderRadius.All == -1)//Rounded TextBox
            {
                //-Fields
                var rectBorderSmooth = ClientRectangle;
                var rectBorder = Rectangle.Inflate(rectBorderSmooth, -BorderSize, -BorderSize);
                int smoothSize = privateBorderSize > 0 ? privateBorderSize : 1;

                using (GraphicsPath pathBorderSmooth = Helper.RoundRect(rectBorderSmooth, Rounding))
                using (GraphicsPath pathBorder = Helper.RoundRect(rectBorder, Rounding - new Padding(BorderSize, BorderSize, BorderSize, BorderSize) - new Padding(1, 1, 1, 1)))
                using (Pen penBorderSmooth = new Pen(Parent.BackColor, smoothSize))
                using (Pen penBorder = new Pen(BorderColor, BorderSize))
                {
                    //-Drawing
                    Region = new Region(pathBorderSmooth);

                    /* { //Old way
                        //Set the rounded region of UserControl
                        if (BorderRadius > 15)
                        SetTextBoxRoundedRegion();//Set the rounded region of TextBox component
                    } */

                    graph.SmoothingMode = SmoothingMode.AntiAlias;
                    penBorder.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;
                    if (privateIsFocused)
                    {
                        BackColor = FocusBackgroundColor;
                        penBorder.Color = FocusBorderColor;
                    }
                    else
                    {
                        BackColor = BackgroundColor;
                    }

                    if (UnderlinedStyle) //Line Style
                    {
                        //Draw border smoothing
                        graph.DrawPath(penBorderSmooth, pathBorderSmooth);
                        //Draw border
                        graph.SmoothingMode = SmoothingMode.None;
                        graph.DrawLine(penBorder, 0, Height - 1, Width, Height - 1);
                    }
                    else //Normal Style
                    {
                        //Draw border smoothing
                        graph.DrawPath(penBorderSmooth, pathBorderSmooth);
                        //Draw border
                        graph.DrawPath(penBorder, pathBorder);
                    }
                }
            }
            else //Square/Normal TextBox
            {
                //Draw border
                using (Pen penBorder = new Pen(BorderColor, BorderSize))
                {
                    Region = new Region(ClientRectangle);
                    penBorder.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
                    if (privateIsFocused)
                        penBorder.Color = FocusBorderColor;

                    if (UnderlinedStyle) //Line Style
                        graph.DrawLine(penBorder, 0, Height - 1, Width, Height - 1);
                    else //Normal Style
                        graph.DrawRectangle(penBorder, 0, 0, Width - 0.5F, Height - 0.5F);
                }
            }

            base.OnPaint(e);
        }

        protected void SetPlaceholder()
        {
            placeholderTextField.Text = PlaceholderText;
            if (privateIsPasswordChar)
            {
                placeholderTextField.UseSystemPasswordChar = false;
            }

            if (actualText == "")
            {
                placeholderTextField.Visible = true;
                privateIsPlaceholder = true;
            }
            else
            {
                privateIsPlaceholder = false;
                Refresh();
            }
        }
        private void RemovePlaceholder()
        {
            if (actualText == "")
            {
                placeholderTextField.Visible = false;
                privateIsPlaceholder = false;
                //textBox2.ForeColor = ForeColor;
                if (privateIsPasswordChar)
                    placeholderTextField.UseSystemPasswordChar = true;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            actualText = contentTextField.Text;
            ContentChanged?.Invoke(this, e);
        }
        private void textBox1_Click(object sender, EventArgs e)
        {
            OnClick(e);
        }

        private void textBox1_MouseEnter(object sender, EventArgs e)
        {
            OnMouseEnter(e);
        }
        private void textBox1_MouseLeave(object sender, EventArgs e)
        {
            OnMouseLeave(e);
        }
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            privateIsFocused = true;
            Refresh();
            RemovePlaceholder();
        }
        private void textBox1_Leave(object sender, EventArgs e)
        {
            privateIsFocused = false;
            Refresh();
            SetPlaceholder();
        }

        private void cuiTextBox2_Click(object sender, EventArgs e)
        {
            contentTextField.Focus();
            Refresh();
        }

        private void textBox2_MouseDown(object sender, MouseEventArgs e)
        {
            cuiTextBox2_Click(sender, e);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (placeholderTextField.Text != PlaceholderText)
            {
                placeholderTextField.Text = PlaceholderText;
            }
        }
    }
}

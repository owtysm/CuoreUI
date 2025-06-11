using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CuoreUI.Controls
{
    [ToolboxBitmap(typeof(ListBox))]
    public partial class cuiListbox : ListBox
    {
        public cuiListbox()
        {
            InitializeComponent();
            DoubleBuffered = true;
            //SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);

            UpdateStyles();
            DrawMode = DrawMode.OwnerDrawFixed;
            BorderStyle = BorderStyle.None;
            ItemHeight = 34;
            ForeColor = Color.FromArgb(84, 84, 84);
            SelectionMode = SelectionMode.One;
            Font = new Font("Microsoft YaHei UI", 9, FontStyle.Regular);

            var parentForm = FindForm();
            if (parentForm != null)
            {
                BackColor = parentForm.BackColor;
            }
        }

        public override Color BackColor
        {
            get
            {
                return base.BackColor;
            }
            set
            {
                if (value == Color.Empty || value == Color.Transparent)
                {
                    var a = FindForm();
                    if (a != null)
                    {
                        base.BackColor = a.BackColor;
                    }
                }
                else
                {
                    base.BackColor = value;
                }
            }
        }

        private Padding privateRounding = new Padding(8);

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

        private Color privateOutlineColor = Color.FromArgb(128, 128, 128, 128);

        [Category("CuoreUI")]
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

        private int privateOutlineThickness = 1;

        [Category("CuoreUI")]
        private int OutlineThickness
        {
            get
            {
                return privateOutlineThickness;
            }
            set
            {
                if (value >= 1)
                {
                    privateOutlineThickness = value;
                    Invalidate();
                }
            }
        }

        private int privateItemRounding = 8;

        [Category("CuoreUI")]
        public int ItemRounding
        {
            get
            {
                return privateItemRounding;
            }
            set
            {
                if (value > 0)
                {
                    if (value > (ItemHeight / 2))
                    {
                        privateItemRounding = (ItemHeight / 2) + 1;
                        ItemRounding = privateItemRounding;
                    }
                    else
                    {
                        privateItemRounding = value;
                    }
                }
                else
                {
                    throw new Exception("ItemRounding cannot be greater than half of Item Height");
                }
                Invalidate();
            }
        }

        private Color privateBackgroundColor = Color.White;

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
                Invalidate();
            }
        }

        private Color privateItemHoveredBackgroundColor = Color.FromArgb(32, 128, 128, 128);

        [Category("CuoreUI")]
        public Color ItemHoverBackgroundColor
        {
            get
            {
                return privateItemHoveredBackgroundColor;
            }
            set
            {
                privateItemHoveredBackgroundColor = value;
                Invalidate();
            }
        }

        private Color privateItemHoveredForegroundColor = Color.DimGray;

        [Category("CuoreUI")]
        public Color ItemHoverForegroundColor
        {
            get
            {
                return privateItemHoveredForegroundColor;
            }
            set
            {
                privateItemHoveredForegroundColor = value;
                Invalidate();
            }
        }

        private Color privateForegroundColor = Color.DimGray;

        [Category("CuoreUI")]
        public Color ForegroundColor
        {
            get
            {
                return privateForegroundColor;
            }
            set
            {
                privateForegroundColor = value;
                Invalidate();
            }
        }

        private Color privateItemBackgroundColor = Color.Empty;

        [Category("CuoreUI")]
        public Color ItemBackgroundColor
        {
            get
            {
                return privateItemBackgroundColor;
            }
            set
            {
                privateItemBackgroundColor = value;
                Invalidate();
            }
        }

        private Color privateItemSelectedBackgroundColor = CuoreUI.Drawing.PrimaryColor;

        [Category("CuoreUI")]
        public Color ItemSelectedBackgroundColor
        {
            get
            {
                return privateItemSelectedBackgroundColor;
            }
            set
            {
                privateItemSelectedBackgroundColor = value;
                Invalidate();
            }
        }

        private Color privateSelectedForegroundColor = Color.White;

        [Category("CuoreUI")]
        public Color SelectedForegroundColor
        {
            get
            {
                return privateSelectedForegroundColor;
            }
            set
            {
                privateSelectedForegroundColor = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            SelectionMode = SelectionMode.One;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Rectangle cr = ClientRectangle;
            Rectangle backgroundRect = cr;
            backgroundRect.Inflate(5, 5);
            backgroundRect.Offset(-1, -1);
            cr.Width -= 1;
            cr.Height -= 1;

            using (Brush bgBrush = new SolidBrush(BackColor))
            {
                g.FillRectangle(bgBrush, backgroundRect);
            }


            using (GraphicsPath path2 = Helper.RoundRect(cr, Rounding))
            using (Brush itemBrush = new SolidBrush(BackgroundColor))
            using (Pen bgPen = new Pen(OutlineColor, OutlineThickness))
            {
                e.Graphics.FillPath(itemBrush, path2);
                e.Graphics.DrawPath(bgPen, path2);

                // Clip to path2 to prevent overflow
                g.SetClip(path2, CombineMode.Intersect);

                // Now draw items inside clipped area
                using (Brush selectedBrush = new SolidBrush(ItemSelectedBackgroundColor))
                using (Brush selectedTextBrush = new SolidBrush(SelectedForegroundColor))
                using (Brush hoverBrush = new SolidBrush(ItemHoverBackgroundColor))
                using (Brush hoverTextBrush = new SolidBrush(ItemHoverForegroundColor))
                using (Brush normalBrush = new SolidBrush(ItemBackgroundColor))
                using (Brush normalTextBrush = new SolidBrush(ForegroundColor))
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        Rectangle itemRect = GetItemRectangle(i);
                        itemRect.Inflate(-4, -2);
                        itemRect.Offset(0, 2);
                        int yCenterString = itemRect.Y + (ItemHeight / 2) - (Font.Height) + 6;

                        using (GraphicsPath path = Helper.RoundRect(itemRect, ItemRounding))
                        {
                            string itemText = Items[i].ToString();

                            if (SelectedIndex == i)
                            {
                                g.FillPath(selectedBrush, path);
                                g.DrawString(itemText, Font, selectedTextBrush, itemRect.X + 6, yCenterString);
                            }
                            else if (HoveredIndex == i)
                            {
                                g.FillPath(hoverBrush, path);
                                g.DrawString(itemText, Font, hoverTextBrush, itemRect.X + 6, yCenterString);
                            }
                            else
                            {
                                g.FillPath(normalBrush, path);
                                g.DrawString(itemText, Font, normalTextBrush, itemRect.X + 6, yCenterString);
                            }
                        }
                    }
                }

                // Reset the clip after drawing
                g.ResetClip();
            }


            base.OnPaint(e);
        }

        private int privateHoveredIndex = -1;
        public int HoveredIndex
        {
            get
            {
                return privateHoveredIndex;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            int temporaryHoveredIndex = IndexFromPoint(e.Location);

            if (temporaryHoveredIndex >= 0 && temporaryHoveredIndex < Items.Count)
            {
                SelectedIndex = temporaryHoveredIndex;
                SuspendLayout();
                Refresh();
                ResumeLayout(true);
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            privateHoveredIndex = IndexFromPoint(e.Location);
            if (e.Button == MouseButtons.Left)
            {
                OnMouseDown(e);
            }

            Refresh();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Refresh();
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            SuspendLayout();
            Refresh();
            ResumeLayout(true);
            base.OnSelectedIndexChanged(e);
        }

        //wndproc generated by gpt to refresh listbox on scroll
        private const int WM_VSCROLL = 0x115;
        private const int WM_MSCROLL = 0x20A;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_VSCROLL || m.Msg == WM_MSCROLL)
            {
                SuspendLayout();
                Refresh();
                ResumeLayout(true);
            }
        }

        private void cuiListbox_MouseLeave(object sender, EventArgs e)
        {
            if (ClientRectangle.Contains(Cursor.Position) == false)
            {
                privateHoveredIndex = -1;
            }
        }
    }
}

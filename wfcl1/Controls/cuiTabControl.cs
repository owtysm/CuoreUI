using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CuoreUI.Controls
{
    [ToolboxBitmap(typeof(TabControl))]
    public partial class cuiTabControl : UserControl
    {
        private bool hoveringInteractive = false;
        public List<TabPage> Pages = new List<TabPage>();
        private int _selectedIndex = -1;

        private bool privateShowPlus = true;
        public bool ShowPlus
        {
            get
            {
                return privateShowPlus;
            }
            set
            {
                privateShowPlus = value;
                Invalidate();
            }
        }

        private bool privateShowDelete = true;
        public bool ShowDelete
        {
            get
            {
                return privateShowDelete;
            }
            set
            {
                privateShowDelete = value;
                Invalidate();
            }
        }

        private int privateRounding = 8;
        public int Rounding
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

        public cuiTabControl()
        {
            InitializeComponent();
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            MouseWheel += CustomTabControl_MouseWheel;
            MouseDown += tb_MouseDown;
            MouseUp += tb_MouseUp;
            MouseMove += tb_MouseMove;
        }

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                SelectTab(value);
            }
        }

        public TabPage SelectedTab
        {
            get
            {
                if (_selectedIndex < 0 || _selectedIndex + 1 < Pages.Count)
                {
                    return null;
                }

                return Pages[_selectedIndex];
            }
            set
            {
                SelectTab(value);
            }
        }

        private int scrollOffset = 0;
        private int tabHeight = 42;

        [Browsable(true)]
        [Category("CuoreUI")]
        [Description("Height of the tab headers.")]
        public int TabHeight
        {
            get => tabHeight;
            set
            {
                tabHeight = Math.Max(16, value); // sane lower bound
                Invalidate();
            }
        }

        private int tabWidth = 104;

        [Browsable(true)]
        [Category("CuoreUI")]
        [Description("Height of the tab headers.")]
        public int TabWidth
        {
            get => tabWidth;
            set
            {
                tabWidth = Math.Max(16, value); // sane lower bound
                Invalidate();
            }
        }

        private const int ScrollSpeed = 80;
        private const int CloseBoxSize = 16;
        private const int TabPadding = 6;

        public int ScrollbarHeight { get; set; } = 8;
        public Rectangle scrollbarThumbRect;
        private bool draggingThumb = false;
        private int dragOffsetX;

        private string namingConvention = "TabPage";

        [Category("CuoreUI")]
        public int ScrollOffset
        {
            get => scrollOffset;
            set
            {
                scrollOffset = Math.Max(0, value);
                Invalidate();
            }
        }

        [Category("CuoreUI")]
        public string NamingConvention
        {
            get => namingConvention;
            set => namingConvention = value ?? "TabPage";
        }

        public string GetUniqueTabName()
        {
            int i = 1;
            while (true)
            {
                string name = $"{namingConvention}{i}";
                if (!Pages.Exists(p => p.Title == name))
                {
                    return name;
                }
                i++;
            }
        }

        public TabPage AddTab(Bitmap image, bool DisposeImageOnDisposal = false) => AddTab(GetUniqueTabName(), image, DisposeImageOnDisposal);
        public TabPage AddTab(bool DisposeImageOnDisposal) => AddTab(GetUniqueTabName(), null, DisposeImageOnDisposal);
        public TabPage AddTab() => AddTab(GetUniqueTabName());

        public TabPage AddTab(string name, Bitmap image = null, bool DisposeImageOnDisposal = false)
        {
            TabPage page = new TabPage();
            page.DisposeImageOnDisposal = DisposeImageOnDisposal;
            page.Image = image;
            page.Title = name;
            page.Dock = DockStyle.Bottom;
            page.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            page.Height = Height - TabHeight - ScrollbarHeight;
            page.Width = Width;
            page.Top = TabHeight + ScrollbarHeight;
            page.BackColor = BackColor;
            Pages.Add(page);
            TabAdded?.Invoke(this, new TabAddedEventArgs(page));

            int totalWidth = Pages.Count * (TabWidth + TabPadding) + TabHeight;
            scrollOffset = Math.Max(0, totalWidth - Width);

            if (_selectedIndex == -1)
            {
                SelectTab(0);
            }

            scrollAlpha = 255;

            Invalidate();
            return page;
        }

        public event EventHandler<TabAddedEventArgs> TabAdded;
        public event EventHandler SelectedTabChanged;
        public event EventHandler<int> TabRemoved;

        public void RemoveTab(int index)
        {
            if (index < 0 || index >= Pages.Count)
            {
                return;
            }

            Pages.RemoveAt(index);
            TabRemoved?.Invoke(this, index);
            if (_selectedIndex >= Pages.Count)
            {
                _selectedIndex = Pages.Count - 1;
            }

            Invalidate();
        }

        public void SelectTab(int index)
        {
            if (index < 0 || index >= Pages.Count || index == _selectedIndex)
            {
                return;
            }

            Controls.Clear();
            var page = Pages[index];
            Controls.Add(page);
            _selectedIndex = index;
            SelectedTabChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        public void SelectTab(TabPage page)
        {
            if (!Pages.Contains(page))
            {
                return;
            }

            SelectTab(Pages.IndexOf(page));
        }

        public Size ImageExpand { get; set; } = new Size(-4, -4);
        public Size TextOffset { get; set; } = new Size(0, 0);

        public Color SelectedImageTint { get; set; } = Color.Black;
        public Color UnselectedImageTint { get; set; } = Color.FromArgb(64, 64, 64);
        public Color HoverImageTint { get; set; } = Color.FromArgb(32, 32, 32);

        public Color UnselectedBackgroundColor { get; set; } = Color.FromArgb(244, 244, 244);
        public Color SelectedBackgroundColor { get; set; } = Color.White;
        public Color HoverBackgroundColor { get; set; } = Color.FromArgb(252, 252, 252);

        public Color SelectedTextColor { get; set; } = Color.Black;
        public Color UnselectedTextColor { get; set; } = Color.FromArgb(64, 64, 64);
        public Color HoverTextColor { get; set; } = Color.FromArgb(32, 32, 32);

        public Color PlusColor { get; set; } = Color.Gray;

        public Color UnselectedDeleteColor { get; set; } = Color.Gray;
        public Color SelectedDeleteColor { get; set; } = Color.Crimson;
        public Color HoverDeleteColor { get; set; } = Color.FromArgb(32, 32, 32);

        public Color ScrollbarColor { get; set; } = Color.FromArgb(128, 128, 128);

        private SizeF cachedTextSize = SizeF.Empty;

        // measure only once for font height
        private void MeasureTextSize(Graphics g)
        {
            if (cachedTextSize == SizeF.Empty)
                cachedTextSize = g.MeasureString("A", Font);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            int totalWidth = (Pages.Count) * (TabWidth + TabPadding) + TabHeight;
            scrollOffset = Math.Max(0, Math.Min(scrollOffset, totalWidth - Width));
            MeasureTextSize(g);

            using (var unselectedBackgroundBrush = new SolidBrush(UnselectedBackgroundColor))
            using (var selectedBackgroundBrush = new SolidBrush(SelectedBackgroundColor))
            using (var hoverBackgroundBrush = new SolidBrush(HoverBackgroundColor))
            using (var selectedTextBrush = new SolidBrush(SelectedTextColor))
            using (var unselectedTextBrush = new SolidBrush(UnselectedTextColor))
            using (var hoverTextBrush = new SolidBrush(HoverTextColor))
            {
                for (int i = 0; i < Pages.Count; i++)
                {
                    int x = i * (TabWidth + TabPadding) - scrollOffset;
                    if (x + TabWidth < 0 || x > Width)
                    {
                        continue;
                    }

                    var currentPage = Pages[i];
                    var tabRect = new Rectangle(x, 0, TabWidth, TabHeight);
                    bool isSelected = i == _selectedIndex;
                    bool isHover = i == _hoverIndex && !isSelected;

                    using (var path = Helper.RoundRect(tabRect, Rounding))
                    {
                        g.FillPath(isSelected ? selectedBackgroundBrush :
                                      isHover ? hoverBackgroundBrush :
                                      unselectedBackgroundBrush, path);
                    }

                    int textHorizontalOffset = 0;

                    if (currentPage.Image != null)
                    {
                        using (Bitmap currentImage = Drawing.Imaging.TintBitmap(currentPage.Image, isSelected ? SelectedImageTint :
                                      isHover ? HoverImageTint :
                                      UnselectedImageTint))
                        {
                            Rectangle imageRect = tabRect;
                            imageRect.Width = imageRect.Height;

                            int targetSize = 24;
                            int dx = (imageRect.Width - targetSize) / 2;
                            int dy = (imageRect.Height - targetSize) / 2;
                            imageRect.Inflate(ImageExpand.Width - dx, ImageExpand.Height - dy);

                            textHorizontalOffset = imageRect.Width;

                            g.DrawImage(currentImage, imageRect);
                        }
                    }

                    g.DrawString(
                        currentPage.Title,
                        Font,
                        isSelected ? selectedTextBrush : isHover ? hoverTextBrush : unselectedTextBrush,
                        tabRect.Left + tabHeight / 2 - Font.Height / 2 + textHorizontalOffset - TextOffset.Width,
                        2 + tabRect.Top + (TabHeight - cachedTextSize.Height) / 2 - TextOffset.Height
                    );

                    if (ShowDelete)
                    {
                        var closeRect = new Rectangle(
                            tabRect.Right - CloseBoxSize - 10,
                            tabRect.Top + (TabHeight - CloseBoxSize) / 2 + 1,
                            CloseBoxSize - 4,
                            CloseBoxSize - 4
                        );

                        using (Pen ClosePen = new Pen(isSelected ? SelectedDeleteColor : isHover ? HoverDeleteColor : UnselectedDeleteColor) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                        using (var closePath = Helper.Crossmark(closeRect))
                        {
                            g.DrawPath(ClosePen, closePath);
                        }
                    }
                }
            }

            void DrawGradient(Rectangle rect, bool reverse = false)
            {
                using (var brush = new LinearGradientBrush(
                    rect,
                    reverse ? BackColor : Color.Transparent, // start
                    reverse ? Color.Transparent : BackColor, // end
                    LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(brush, rect);
                }
            }

            bool isOverfilled = totalWidth > Width;
            if (isOverfilled)
            {
                float visibleRatio = Width / (float)totalWidth;
                int thumbWidth = Math.Max(30, (int)(Width * visibleRatio));
                int maxThumbPos = Width - thumbWidth;
                int thumbX = (int)(scrollOffset / (float)(totalWidth - Width) * maxThumbPos);
                scrollbarThumbRect = new Rectangle(thumbX, TabHeight + ScrollbarHeight / 2 - 3, thumbWidth - 1, ScrollbarHeight);

                // scroll and scroll arrows
                using (var scrollBrush = new SolidBrush(Color.FromArgb(scrollAlpha, ScrollbarColor)))
                using (var scrollPath = Helper.RoundRect(scrollbarThumbRect, ScrollbarHeight / 2 - 1))
                {
                    g.FillPath(scrollBrush, scrollPath);

                    using (SolidBrush UnselectedTextBrush = new SolidBrush(UnselectedTextColor))
                        if (scrollOffset < totalWidth - Width)
                        {
                            DrawGradient(new Rectangle(Width - 30, 0, 32, TabHeight + ScrollbarHeight));
                            using (GraphicsPath rightScrollArrow = Helper.LeftArrowtest(new Rectangle(Width - 6, TabHeight + ScrollbarHeight / 2 - 3, 6, 7)))
                            using (Matrix matrix = new Matrix())
                            {
                                RectangleF bounds = rightScrollArrow.GetBounds();
                                matrix.Translate(-bounds.X - bounds.Width / 2, 0, MatrixOrder.Append);
                                matrix.Scale(-1, 1, MatrixOrder.Append);
                                matrix.Translate(bounds.X + bounds.Width / 2, 0, MatrixOrder.Append);
                                rightScrollArrow.Transform(matrix);

                                g.FillPath(UnselectedTextBrush, rightScrollArrow);
                            }
                        }

                    if (scrollOffset > 0)
                    {
                        DrawGradient(new Rectangle(-2, 0, 33, TabHeight + ScrollbarHeight), true);

                        using (SolidBrush UnselectedTextBrush = new SolidBrush(UnselectedTextColor))
                        using (GraphicsPath leftScrollArrow = Helper.LeftArrowtest(new Rectangle(0, TabHeight + ScrollbarHeight / 2 - 3, 6, 7)))
                        {
                            g.FillPath(UnselectedTextBrush, leftScrollArrow);
                        }
                    }
                }
            }

            if (ShowPlus)
            {
                int addX = Pages.Count * (TabWidth + TabPadding) - scrollOffset;
                var addRect = new Rectangle(addX, 0, TabHeight, TabHeight);

                using (Pen plusPen = new Pen(PlusColor))
                using (GraphicsPath plus = Helper.Plus(new Rectangle(addRect.Left + addRect.Width / 2 - 6, (int)(addRect.Top + (TabHeight - cachedTextSize.Height) / 2), 12, 12)))
                {
                    g.DrawPath(plusPen, plus);
                }
            }
        }

        private int _hoverIndex = -1;

        private void CustomTabControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (Pages.Count * (TabWidth + TabPadding) - TabPadding <= Width)
            {
                return;
            }

            scrollOffset -= e.Delta > 0 ? ScrollSpeed : -ScrollSpeed;
            Invalidate();
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return keyData is Keys.Left || keyData is Keys.Right || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // scroll with keys
            if (e.KeyCode == Keys.Left)
            {
                scrollOffset -= ScrollSpeed;
                Invalidate();
            }
            else if (e.KeyCode == Keys.Right)
            {
                scrollOffset += ScrollSpeed;
                Invalidate();
            }
            base.OnKeyDown(e);
        }

        private void tb_MouseDown(object sender, MouseEventArgs e)
        {
            int totalWidth = (Pages.Count) * (TabWidth + TabPadding) + TabHeight;

            // check if cursor over tab
            for (int i = 0; i < Pages.Count; i++)
            {
                int x = i * (TabWidth + TabPadding) - scrollOffset;
                var tabRect = new Rectangle(x, 0, TabWidth, TabHeight);

                if (!tabRect.Contains(e.Location))
                    continue;

                if (ShowDelete)
                {
                    var closeRect = new Rectangle(
                               tabRect.Right - CloseBoxSize - 10,
                               tabRect.Top + (TabHeight - CloseBoxSize) / 2 + 1,
                               CloseBoxSize - 4, CloseBoxSize - 4);

                    if (closeRect.Contains(e.Location))
                    {
                        RemoveTab(i);
                        if (_selectedIndex == i)
                            SelectTab(Math.Min(i, Pages.Count - 1));
                        return;
                    }
                }

                SelectTab(i);
                return;
            }

            if (ShowPlus)
            {
                int addX = Pages.Count * (TabWidth + TabPadding) - scrollOffset;
                var addRect = new Rectangle(addX, 0, TabHeight, TabHeight);
                if (addRect.Contains(e.Location))
                {
                    SelectTab(AddTab());
                    return;
                }
            }

            // scroll start
            Rectangle expandedThumbRect = scrollbarThumbRect;
            expandedThumbRect.Inflate(2, 2);
            if (expandedThumbRect.Contains(e.Location))
            {
                draggingThumb = true;
                dragOffsetX = e.X - scrollbarThumbRect.X;
            }
        }

        private void tb_MouseUp(object sender, MouseEventArgs e)
        {
            draggingThumb = false;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoverIndex != -1)
            {
                _hoverIndex = -1;
                Invalidate();
            }
        }

        private void tb_MouseMove(object sender, MouseEventArgs e)
        {
            int newHover = -1;
            bool newHoveringInteractive = false;

            for (int i = 0; i < Pages.Count; i++)
            {
                int x = i * (TabWidth + TabPadding) - scrollOffset;
                var tabRect = new Rectangle(x, 0, TabWidth, TabHeight);
                if (tabRect.Contains(e.Location))
                {
                    newHover = i;
                    if (ShowDelete)
                    {
                        // check if hovering over close box
                        var closeRect = new Rectangle(
                        tabRect.Right - CloseBoxSize - 10,
                        tabRect.Top + (TabHeight - CloseBoxSize) / 2 + 1,
                        CloseBoxSize - 4, CloseBoxSize - 4);

                        if (closeRect.Contains(e.Location))
                            newHoveringInteractive = true;
                    }
                    break;
                }
            }

            if (ShowPlus)
            {
                // check add tab button hover
                int addX = Pages.Count * (TabWidth + TabPadding) - scrollOffset;
                var addRect = new Rectangle(addX, 0, TabHeight, TabHeight);
                if (addRect.Contains(e.Location))
                {
                    newHover = Pages.Count;
                    newHoveringInteractive = true;
                }
            }

            // update hover index
            if (newHover != _hoverIndex)
            {
                _hoverIndex = newHover;
                Invalidate();
            }

            // update cursor
            if (newHoveringInteractive != hoveringInteractive)
            {
                hoveringInteractive = newHoveringInteractive;
                Cursor = hoveringInteractive ? Cursors.Hand : Cursors.Default;
            }

            // handle thumb dragging
            if (draggingThumb)
            {
                int totalWidth = (Pages.Count) * (TabWidth + TabPadding) + TabHeight;
                int thumbWidth = scrollbarThumbRect.Width;
                int maxThumbX = Width - thumbWidth;

                int newThumbX = Math.Max(0, Math.Min(e.X - dragOffsetX, maxThumbX));
                float ratio = newThumbX / (float)maxThumbX;
                scrollOffset = (int)((totalWidth - Width) * ratio);

                Invalidate();
            }
            else
            {
                Rectangle expandedScrollRect = scrollbarThumbRect;
                expandedScrollRect.Inflate(2, 2);
                if (expandedScrollRect.Contains(e.Location))
                    scrollAlpha = 255;
                Invalidate(scrollbarThumbRect);
            }
        }

        bool scrollbarUntouchable = false;
        byte scrollAlpha = 0;

        // 32ms delay is unnoticable imo
        private void scrollbarTimer_Tick(object sender, EventArgs e)
        {
            if (scrollAlpha == 0)
            {
                return;
            }

            if (draggingThumb || scrollbarUntouchable || scrollbarThumbRect.Contains(PointToClient(Cursor.Position)))
            {
                scrollAlpha = 255;
            }
            else
            {
                scrollAlpha = (byte)(((scrollAlpha * 80) + 128) / 85);
            }

            Rectangle expandedThumbRect = scrollbarThumbRect;
            expandedThumbRect.Inflate(2, 2);
            Invalidate(expandedThumbRect);
        }
    }

    [ToolboxItem(false)]
    public class TabPage : Panel
    {
        public bool DisposeImageOnDisposal = false;
        public string Title { get; set; }
        public Bitmap Image { get; set; }

        public TabPage()
        {
            BackColor = Color.White;
        }

        protected override void Dispose(bool disposing)
        {
            if (DisposeImageOnDisposal)
            {
                Image?.Dispose();
                Image = null;
            }

            base.Dispose(disposing);
        }
    }

    public class TabAddedEventArgs : EventArgs
    {
        public TabAddedEventArgs(TabPage addedPage)
        {
            Tab = addedPage;
        }

        public TabPage Tab { get; set; }
    }
}

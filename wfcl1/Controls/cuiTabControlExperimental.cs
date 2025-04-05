using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CuoreUI.TabControlStuff;
using static System.Windows.Forms.TabControl;

namespace CuoreUI.Controls
{
    [ToolboxBitmap(typeof(TabControl))]
    public partial class cuiTabControlExperimental : UserControl
    {
        private readonly cuiTabControl tabControl = new cuiTabControl();
        private readonly Panel panel = new Panel();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TabPageCollection TabPages => tabControl.TabPages;

        public cuiTabControlExperimental()
        {
            InitializeComponent();

            Controls.Add(panel);
            Controls.Add(tabControl);

            tabControl.ControlAdded += CountChanged;
            tabControl.ControlRemoved += CountChanged;
            if (!DesignMode)
            {
                panel.Paint += panelPaint;
                panel.MouseClick += panelClick;
            }

            tabControl.TabAdded += TabControl_TabAdded;
        }

        public string tabNamingConvention
        {
            get
            {
                return tabControl.tabNamingConvention;
            }
            set
            {
                tabControl.tabNamingConvention = value;
            }
        }

        public string GetUniqueTabName()
        {
            int i = 1;
            string finalUniqueName = string.Empty;
            bool foundUniqueName = false;

            while (!foundUniqueName)
            {
                string generatedTabName = tabNamingConvention + i;
                bool nameExists = false;

                foreach (TabPage tb in TabPages)
                {
                    if (generatedTabName == tb.Name || generatedTabName == tb.Text)
                    {
                        nameExists = true;
                        break;
                    }
                }

                if (!nameExists)
                {
                    finalUniqueName = generatedTabName;
                    foundUniqueName = true;
                }

                i++;
            }

            return finalUniqueName;
        }

        private Color privateOverflowArrowsColor = Color.White;
        public Color OverflowArrowsColor
        {
            get
            {
                return privateOverflowArrowsColor;
            }
            set
            {
                privateOverflowArrowsColor = value;
                Invalidate();
            }
        }


        public void AddTab()
        {
            tabControl.AddTab();
        }

        public void AddTab(string tabName)
        {
            tabControl.AddTab(tabName);
        }

        public void AddTab(TabPage tabPage)
        {
            tabControl.AddTab(tabPage);
        }

        public void AddTab(cuiTabPage tabPage)
        {
            tabControl.AddTab(tabPage);
        }

        private void TabControl_TabAdded(object sender, EventArgs e)
        {
            TabAdded?.Invoke(sender, EventArgs.Empty);
        }


        [Description("sender is the added tab!")]
        [Browsable(true)]
        public event EventHandler TabAdded;

        #region VisualProperties
        public object HoveredTab_ => null;

        public object SelectedTab_ => null;

        public object UnselectedTab_ => null;
        #endregion

        public Color BackgroundColor
        {
            get => tabControl.BackgroundColor;
            set => tabControl.BackgroundColor = value;
        }

        public int Rounding
        {
            get => tabControl.Rounding;
            set => tabControl.Rounding = value;
        }

        public Color UnselectedTabBackColor
        {
            get => tabControl.UnselectedTabBackColor;
            set => tabControl.UnselectedTabBackColor = value;
        }

        public Color SelectedTabBackColor
        {
            get => tabControl.SelectedTabBackColor;
            set => tabControl.SelectedTabBackColor = value;
        }

        public Color HoveredTabBackColor
        {
            get => tabControl.HoveredTabBackColor;
            set => tabControl.HoveredTabBackColor = value;
        }

        public Color UnselectedTabTextBackColor
        {
            get => tabControl.UnselectedTabTextBackColor;
            set => tabControl.UnselectedTabTextBackColor = value;
        }

        public Color SelectedTabTextBackColor
        {
            get => tabControl.SelectedTabTextBackColor;
            set => tabControl.SelectedTabTextBackColor = value;
        }

        public Color HoveredTabTextBackColor
        {
            get => tabControl.HoveredTabTextBackColor;
            set => tabControl.HoveredTabTextBackColor = value;
        }

        public Color DeletionTabBackgroundColor
        {
            get => tabControl.DeletionTabBackgroundColor;
            set => tabControl.DeletionTabBackgroundColor = value;
        }

        public Color DeletionColor
        {
            get => tabControl.DeletionColor;
            set => tabControl.DeletionColor = value;
        }

        public Color AddButtonBackgroundColor
        {
            get => tabControl.AddButtonBackgroundColor;
            set => tabControl.AddButtonBackgroundColor = value;
        }

        public bool ShowAddTabButton
        {
            get => tabControl.ShowAddTabButton;
            set => tabControl.ShowAddTabButton = value;
        }

        private void panelClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (leftRect.Contains(e.Location))
                {
                    int nextIndex = tabControl.SelectedIndex - 1;
                    if (tabControl.TabPages.Count >= nextIndex && tabControl.TabPages.Count > 0 && nextIndex > -1)
                    {
                        tabControl.SelectedIndex = nextIndex;
                    }
                }

                if (rightRect.Contains(e.Location))
                {
                    int nextIndex = tabControl.SelectedIndex + 1;
                    if (tabControl.TabPages.Count >= nextIndex)
                    {
                        tabControl.SelectedIndex = nextIndex;
                    }
                }
            }
        }

        public bool IsTabControlOverfilled
        {
            get
            {
                int items = tabControl.TabPages.Count;
                int itemsTimesItemWidth = items * (tabControl.ItemSize.Width + 3);
                return itemsTimesItemWidth > Width;
            }
        }

        public int TabPagesWidth
        {
            get
            {
                int items = tabControl.TabPages.Count;
                return items * (tabControl.ItemSize.Width + 3);
            }
        }

        private void CountChanged(object sender, EventArgs e)
        {

            Invalidate();
        }

        Rectangle leftRect = Rectangle.Empty;
        Rectangle rightRect = Rectangle.Empty;


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            if (Controls.Count != 2)
            {
                Controls.Add(panel);
                Controls.Add(tabControl);
            }
            panel.Width = 40;
            panel.Height = tabControl.ItemSize.Height + 4;
            panel.Location = new Point(Width - panel.Width, 0);
            panel.BackColor = tabControl.BackgroundColor;

            if (IsTabControlOverfilled)
            {
                panel.Visible = true;
            }
            else
            {
                panel.Visible = false;
            }

            panel.Margin = Padding.Empty;
            panel.Padding = Padding.Empty;
            tabControl.Dock = DockStyle.Fill;
        }

        public bool AllowNoTabs => tabControl.AllowNoTabs;

        public object GetSelectedTab()
        {
            return tabControl.SelectedTab;
        }

        public int SelectedIndex => tabControl.SelectedIndex;
        public object SelectedTab => tabControl.SelectedTab;

        private void panelPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int halfPanelWidth = panel.Width / 2;
            int panelHeight = panel.Height;

            leftRect = new Rectangle(0, 0, halfPanelWidth, panelHeight);
            rightRect = new Rectangle(halfPanelWidth, 0, halfPanelWidth, panelHeight);

            Pen arrowsPen = new Pen(OverflowArrowsColor, 3);
            arrowsPen.StartCap = LineCap.Round;
            arrowsPen.EndCap = LineCap.Round;
            arrowsPen.LineJoin = LineJoin.Round;

            GraphicsPath arrowLeft = Helper.LeftArrow(leftRect);
            e.Graphics.DrawPath(arrowsPen, arrowLeft);

            GraphicsPath arrowRight = Helper.RightArrow(rightRect);
            e.Graphics.DrawPath(arrowsPen, arrowRight);
        }
    }
}

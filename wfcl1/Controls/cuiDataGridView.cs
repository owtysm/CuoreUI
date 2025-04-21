using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CuoreUI.Controls
{
    public class cuiDataGridView : ScrollableControl
    {
        private DataGridView hiddenDataGridView;
        private int rowHeight = 25;
        private int headerHeight = 30;
        private int columnCount = 3;
        private int visibleRows;
        private int scrollOffset = 0;

        private int horizontalScrollOffset = 0;
        private int columnWidth = 100;
        private int hoveredRow = -1; private int hoveredColumn = -1;
        private Pen borderPen;

        private Color bgColorCell = Color.White;
        private Color bgColorCell2 = Color.LightGray;
        private Color bgColorCellHovered = Color.Gray;
        private Color bgColorCellSelected = CuoreUI.Drawing.PrimaryColor;
        private Color borderColor = Color.Black;

        [Browsable(true)]
        [Category("CuoreUI")]
        [Description("Background color for regular cells.")]
        public Color Cell
        {
            get => bgColorCell;
            set
            {
                bgColorCell = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("CuoreUI")]
        [Description("Background color for alternating cells.")]
        public Color Cell2
        {
            get => bgColorCell2;
            set
            {
                bgColorCell2 = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("CuoreUI")]
        [Description("Background color for hovered cells.")]
        public Color CellHover
        {
            get => bgColorCellHovered;
            set
            {
                bgColorCellHovered = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("CuoreUI")]
        [Description("Background color for selected cells.")]
        public Color CellSelect
        {
            get => bgColorCellSelected;
            set
            {
                bgColorCellSelected = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("CuoreUI")]
        [Description("Border color for the cells.")]
        public Color CellBorder
        {
            get => borderColor;
            set
            {
                borderColor = value;
                borderPen.Color = borderColor;
                Invalidate();
            }
        }

        private Padding rightColumnRounding;
        private Padding leftColumnRounding;
        private Padding columnRounding = new Padding(0, 0, 0, 0);

        private Padding topLeftCorner;
        private Padding bottomLeftCorner;
        private Padding topRightCorner;
        private Padding bottomRightCorner;

        private int privateRounding = 8;

        [Browsable(true)]
        [Category("CuoreUI")]
        [Description("Corner radius of the cells that support it.")]
        public int Rounding
        {
            get
            {
                return privateRounding;
            }
            set
            {
                privateRounding = value;
                leftColumnRounding = new Padding(value, 0, 0, 0);
                rightColumnRounding = new Padding(0, value, 0, 0);

                topLeftCorner = new Padding(0, 0, 0, 0);
                bottomLeftCorner = new Padding(0, 0, value, 0);

                topRightCorner = new Padding(0, 0, 0, 0);
                bottomRightCorner = new Padding(0, 0, 0, value);

                Refresh();
            }
        }

        public cuiDataGridView()
        {
            hiddenDataGridView = new DataGridView();
            hiddenDataGridView.Visible = false;
            hiddenDataGridView.DataSourceChanged += DataSourceChanged;
            hiddenDataGridView.CellValueChanged += HiddenDataGridView_CellValueChanged;
            hiddenDataGridView.SelectionChanged += HiddenDataGridView_SelectionChanged;
            hiddenDataGridView.ColumnHeaderMouseClick += HiddenDataGridView_ColumnHeaderMouseClick;
            Controls.Add(hiddenDataGridView);

            borderPen = new Pen(borderColor);

            this.DoubleBuffered = true;
        }
        public DataGridViewSelectedCellCollection SelectedCells // this is so annoying but i'd rather do this than make a datagridview from zero
        {
            get
            {
                hiddenDataGridView.ClearSelection();

                foreach (var cell in selectedCells)
                {
                    if (cell.RowId >= 0 && cell.RowId < hiddenDataGridView.Rows.Count &&
                        cell.ColumnId >= 0 && cell.ColumnId < hiddenDataGridView.Columns.Count)
                    {
                        hiddenDataGridView.Rows[cell.RowId].Cells[cell.ColumnId].Selected = true;
                    }
                }

                return hiddenDataGridView.SelectedCells;
            }
        }

        private string[] headers;

        private void DataSourceChanged(object sender, EventArgs e)
        {
            if (hiddenDataGridView.DataSource is System.Data.DataTable dataTable)
            {
                columnCount = dataTable.Columns.Count;
                string[] headersPreParsed = dataTable.Columns.Cast<System.Data.DataColumn>().Select(c => c.ColumnName).ToArray();
                int index = 0;
                foreach (string header in headersPreParsed)
                {
                    if (header.StartsWith("Column"))
                    {
                        string split = header.Substring(6);
                        if (int.TryParse(split, out _))
                        {
                            headersPreParsed[index] = "";
                        }
                    }
                    index++;
                }
                headers = headersPreParsed;
            }
            else if (hiddenDataGridView.DataSource is System.Collections.IEnumerable enumerable)
            {
                var enumerator = enumerable.GetEnumerator();
                if (enumerator.MoveNext() && enumerator.Current is var firstRow)
                {
                    var properties = firstRow.GetType().GetProperties();
                    columnCount = properties.Length;
                    headers = properties.Select(p => p.Name).ToArray();
                }
            }
            Refresh();
        }

        private void HiddenDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            Refresh();
        }

        private void HiddenDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            Refresh();
        }

        private void HiddenDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            hiddenDataGridView.Sort(hiddenDataGridView.Columns[e.ColumnIndex], System.ComponentModel.ListSortDirection.Ascending);
            Refresh();
        }

        public object DataSource
        {
            get => hiddenDataGridView.DataSource;
            set
            {
                hiddenDataGridView.DataSource = value;
                Refresh();
            }
        }

        public void Sort(DataGridViewColumn column, System.ComponentModel.ListSortDirection direction)
        {
            hiddenDataGridView.Sort(column, direction);
            Refresh();
        }

        cuiScrollbar scrollBar = null;
        bool showVerticalScrollbar = false;

        private int WidthConsideringScrollbar
        {
            get
            {
                if (scrollBar == null || showVerticalScrollbar == false)
                {
                    return Width;
                }
                return Width - scrollBar.Width;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (privateRounding != Rounding)
            {
                Rounding = Rounding;
            }

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            DrawHeader(g);

            DrawRows(g);

            if (Controls.Count > 0 && Controls[0] is cuiScrollbar sBar)
            {
                if (DesignMode)
                {
                    sBar.Visible = true;
                }
                else
                {
                    sBar.Visible = false;
                }
                scrollBar = sBar;
                showVerticalScrollbar = true;
            }
            else
            {
                scrollBar = null;
                showVerticalScrollbar = false;
            }

            base.OnPaint(e);
        }

        private Color privateHeaderColor = Color.Gray;

        [Browsable(true)]
        [Category("CuoreUI")]
        [Description("Color of the headers (top columns)")]
        public Color HeaderColor
        {
            get
            {
                return privateHeaderColor;
            }
            set
            {
                privateHeaderColor = value;
                Refresh();
            }
        }

        private void DrawHeader(Graphics g)
        {
            for (int col = 0; col < columnCount; col++)
            {
                Rectangle headerRect = new Rectangle(col * ((WidthConsideringScrollbar - 1) / columnCount), 0, (WidthConsideringScrollbar - 1) / columnCount, headerHeight);
                GraphicsPath roundedBackgroundColumn;
                if (col == 0)
                {
                    roundedBackgroundColumn = Helper.RoundRect(headerRect, rightColumnRounding);
                }
                else if (col == columnCount - 1)
                {
                    roundedBackgroundColumn = Helper.RoundRect(headerRect, leftColumnRounding);
                }
                else
                {
                    roundedBackgroundColumn = Helper.RoundRect(headerRect, columnRounding);
                }

                g.FillPath(new SolidBrush(HeaderColor), roundedBackgroundColumn);
                g.DrawPath(Pens.Black, roundedBackgroundColumn);

                string headerText = headers != null && headers.Length > col ? headers[col] : $"Header {col + 1}";
                g.DrawString(headerText, Font, Brushes.White, headerRect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                if (headerRect.Right > this.Width)
                {
                    break;
                }
            }
        }

        public class CellClass
        {
            public int RowId
            {
                get; set;
            }
            public int ColumnId
            {
                get; set;
            }

            public CellClass(int rowId, int columnId)
            {
                RowId = rowId;
                ColumnId = columnId;
            }

            public override bool Equals(object obj)
            {
                if (obj is CellClass other)
                {
                    return RowId == other.RowId && ColumnId == other.ColumnId;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return (RowId, ColumnId).GetHashCode();
            }
        }

        private void DrawRows(Graphics g)
        {
            visibleRows = (Height - headerHeight) / rowHeight;

            for (int row = 0; row < visibleRows; row++)
            {
                int dataRowIndex = row + scrollOffset;
                if (dataRowIndex >= hiddenDataGridView.Rows.Count)
                    break;

                for (int col = 0; col < columnCount; col++)
                {
                    Rectangle cellRect = new Rectangle(col * ((WidthConsideringScrollbar - 1) / columnCount), headerHeight + row * rowHeight, (WidthConsideringScrollbar - 1) / columnCount, rowHeight);

                    if (cellRect.Right > this.Width || cellRect.Bottom > this.Height)
                    {
                        continue;
                    }

                    GraphicsPath roundedBackgroundColumn = null;

                    bool shouldRound = true;

                    if (row == 0 && col == 0)
                    {
                        roundedBackgroundColumn = Helper.RoundRect(cellRect, topLeftCorner);
                    }
                    else if (row == 0 && col == columnCount - 1)
                    {
                        roundedBackgroundColumn = Helper.RoundRect(cellRect, topRightCorner);
                    }
                    else if (row == visibleRows - 1 && col == columnCount - 1)
                    {
                        roundedBackgroundColumn = Helper.RoundRect(cellRect, bottomRightCorner);
                    }
                    else if (row == visibleRows - 1 && col == 0)
                    {
                        roundedBackgroundColumn = Helper.RoundRect(cellRect, bottomLeftCorner);
                    }
                    else
                    {
                        shouldRound = false;
                    }

                    SolidBrush bgBrush;
                    if (selectedCells.Any(c => c.RowId == dataRowIndex && c.ColumnId == col))
                    {
                        bgBrush = new SolidBrush(CellSelect);
                    }
                    else if (dataRowIndex == hoveredRow && col == hoveredColumn)
                    {
                        bgBrush = new SolidBrush(CellHover);
                    }
                    else if (dataRowIndex % 2 == 0)
                    {
                        bgBrush = new SolidBrush(Cell);
                    }
                    else
                    {
                        bgBrush = new SolidBrush(Cell2);
                    }

                    if (shouldRound)
                    {
                        g.FillPath(bgBrush, roundedBackgroundColumn);
                        g.DrawPath(Pens.Black, roundedBackgroundColumn);
                    }
                    else
                    {
                        g.FillRectangle(bgBrush, cellRect);
                        g.DrawRectangle(Pens.Black, cellRect);
                    }

                    var cellValue = hiddenDataGridView.Rows[dataRowIndex].Cells[col].Value?.ToString() ?? string.Empty;
                    g.DrawString(cellValue, Font, new SolidBrush(ForeColor), cellRect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                    if (cellRect.Right > this.Width)
                    {
                        break;
                    }
                }
            }
        }

        private List<CellClass> selectedCells = new List<CellClass>();

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int clickedRow = (e.Y - headerHeight) / rowHeight;
            int dataRowIndex = clickedRow + scrollOffset;

            if (dataRowIndex >= 0 && dataRowIndex < hiddenDataGridView.Rows.Count)
            {
                var cell = new CellClass(dataRowIndex, hoveredColumn);
                bool containsCellOriginally = selectedCells.Contains(cell);

                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    if (containsCellOriginally)
                    {
                        selectedCells.Remove(cell);
                    }
                    else
                    {
                        selectedCells.Add(cell);
                    }
                }
                else
                {
                    if (containsCellOriginally)
                    {
                        if (selectedCells.Count >= 1)
                        {
                            selectedCells.Clear();
                            selectedCells.Add(cell);
                        }
                        selectedCells.Remove(cell);
                    }
                    else
                    {
                        selectedCells.Clear();
                        selectedCells.Add(cell);
                    }
                }

                Refresh();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            int hoveredRowIndex = (e.Y - headerHeight) / rowHeight + scrollOffset;
            int hoveredColumnIndex = e.X / ((WidthConsideringScrollbar - 1) / columnCount);

            if (hoveredRowIndex >= 0 && hoveredRowIndex < hiddenDataGridView.Rows.Count)
            {
                hoveredRow = hoveredRowIndex;
                hoveredColumn = hoveredColumnIndex;
            }
            else
            {
                hoveredRow = -1;
                hoveredColumn = -1;
            }

            Refresh();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (ModifierKeys == Keys.Shift)
            {
                int columnsInView = WidthConsideringScrollbar / columnWidth;

                if (e.Delta > 0 && horizontalScrollOffset > 0)
                {
                    horizontalScrollOffset--;
                }
                else if (e.Delta < 0 && horizontalScrollOffset + columnsInView < hiddenDataGridView.ColumnCount)
                {
                    horizontalScrollOffset++;
                }

                ;
            }
            else
            {
                int rowsInView = (Height - headerHeight) / rowHeight;

                if (e.Delta > 0 && scrollOffset > 0)
                {
                    scrollOffset--;
                }
                else if (e.Delta < 0 && scrollOffset + rowsInView < hiddenDataGridView.Rows.Count)
                {
                    scrollOffset++;
                }
            }

            Refresh();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hoveredColumn = -1;
            hoveredRow = -1;
            Refresh();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Refresh();
        }
    }

}

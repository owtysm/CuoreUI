using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CuoreUI.Controls.Charts
{
    public partial class cuiChartPie : UserControl
    {
        public cuiChartPie()
        {
            InitializeComponent();
            this.MouseMove += new MouseEventHandler(OnMouseMove);
            DoubleBuffered = true;
            ForeColor = Color.White;
            Font = new Font("Microsoft Yahei UI", 8.25f);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        // Private fields for properties
        private string[] _dataPoints = { "data1_100", "data2_90", "data3_50", "data4_50", "data5_300" };

        private List<float> privateDataValuePoints = new List<float>();
        private List<string> privateDataNamePoints = new List<string>();

        private Color _segmentColor = Color.FromArgb(64, CuoreUI.Drawing.PrimaryColor);
        private Color _segmentBorderColor = CuoreUI.Drawing.PrimaryColor;
        private int privateChartPadding = 30;
        private bool privateShowPopup = true;

        private float privateTotalValue;
        private bool showPopup = false;
        private PointF popupLocation;
        private string popupText;

        // Exposed properties with get/set and Invalidate for refresh
        public string[] DataPoints
        {
            get => _dataPoints;
            set
            {
                int dParseIndex = 0;

                List<float> localValues = new List<float>();
                List<string> localNames = new List<string>();

                foreach (string dataPoint in value)
                {
                    string[] splitData = dataPoint.Split('_');
                    string dataString = splitData[0];
                    float dataValue = Convert.ToSingle(splitData[1]);

                    localValues.Add(dataValue);
                    localNames.Add(dataString);

                    dParseIndex++;
                }

                privateDataValuePoints.Clear();
                privateDataNamePoints.Clear();

                privateDataValuePoints.AddRange(localValues);
                privateDataNamePoints.AddRange(localNames);

                _dataPoints = value;

                GC.Collect();

                Invalidate(); // Refresh the control when property changes
            }
        }

        public Color SegmentColor
        {
            get => _segmentColor;
            set
            {
                _segmentColor = value;
                Invalidate();
            }
        }

        public Color SegmentBorderColor
        {
            get => _segmentBorderColor;
            set
            {
                _segmentBorderColor = value;
                Invalidate();
            }
        }

        private Color GradientEndColor
        {
            get => Color.FromArgb(48, ChartBorderColor);
        }

        private Color PopupBackground
        {
            get
            {
                if (BackColor.R + BackColor.G + BackColor.B > 128)
                {
                    return Color.FromArgb(64, Color.Black);
                }
                else
                {
                    return Color.FromArgb(64, Color.White);
                }
            }
        }

        private Color PopupText
        {
            get
            {
                if (BackColor.R + BackColor.G + BackColor.B > 128)
                {
                    return Color.White;
                }
                else
                {
                    return Color.Black;
                }
            }
        }

        public int ChartPadding
        {
            get => privateChartPadding;
            set
            {
                privateChartPadding = value;
                MinimumSize = new Size(value * 4, value * 4);
                Invalidate();
            }
        }

        public bool ShowPopup
        {
            get => privateShowPopup;
            set
            {
                privateShowPopup = value;
                Invalidate();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Refresh();
        }

        // Properties to expose
        public Color SliceBorderColor { get; set; } = Color.FromArgb(64, 255, 255, 255);  // Default color for slice borders
        public Color ChartBorderColor { get; set; } = CuoreUI.Drawing.PrimaryColor;   // Default color for chart border

        public float SliceBorderThickness { get; set; } = 1;         // Thickness of the slice border
        public float ChartBorderThickness { get; set; } = 1.6f;         // Thickness of the chart border

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            int chartSize = Math.Min(Width, Height) - 2 * ChartPadding;
            int centerX = Width / 2;
            int centerY = Height / 2;

            // Radial gradient for the pie chart background (optional)
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(centerX - chartSize / 2, centerY - chartSize / 2, chartSize, chartSize);
                using (PathGradientBrush brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = Color.Transparent;
                    brush.SurroundColors = new[] { GradientEndColor };
                    brush.FocusScales = new PointF(0.25f, 0.25f);  // Controls the gradient's spread
                    g.FillEllipse(brush, centerX - chartSize / 2, centerY - chartSize / 2, chartSize, chartSize);
                }
            }

            // Calculate total value for proportional pie slices
            privateTotalValue = privateDataValuePoints.Sum();

            float currentAngle = 0;

            // gather all slices of the pie chart in one graphicspath and draw at once
            using (GraphicsPath piesPath = new GraphicsPath())
            {
                try
                {

                    // Draw pie chart segment edges (slice borders)
                    for (int i = 0; i < DataPoints.Length; i++)
                    {
                        float sweepAngle = (privateDataValuePoints[i] / privateTotalValue) * 360f;

                        piesPath.AddPie(centerX - chartSize / 2, centerY - chartSize / 2, chartSize, chartSize, currentAngle, sweepAngle);

                        // Calculate midpoint angle of the current slice
                        float midAngle = currentAngle + sweepAngle / 2;

                        // Convert polar coordinates (midAngle) to Cartesian coordinates
                        float textX = centerX + (float)((chartSize / 4) * Math.Cos(midAngle * Math.PI / 180));
                        float textY = centerY + (float)((chartSize / 4) * Math.Sin(midAngle * Math.PI / 180));

                        // Draw the corresponding value from _dataNamePoints
                        string text = privateDataNamePoints[i];
                        SizeF textSize = g.MeasureString(text, Font);
                        g.DrawString(text, Font, new SolidBrush(ForeColor), textX - textSize.Width / 2, textY - textSize.Height / 2);

                        currentAngle += sweepAngle;
                    }
                }
                catch
                {
                    ChartPadding = ChartPadding;
                    DataPoints = DataPoints;
                    return;
                }

                // Draw pie slice edges
                using (Pen slicePen = new Pen(SliceBorderColor, SliceBorderThickness)) // Using SliceBorderColor
                {
                    slicePen.MiterLimit = 0;
                    slicePen.DashStyle = DashStyle.Dash;
                    slicePen.DashCap = DashCap.Round;

                    slicePen.EndCap = LineCap.Round;
                    slicePen.StartCap = LineCap.Round;
                    g.DrawPath(slicePen, piesPath);
                }

                // Draw the overall pie chart border
                using (Pen chartPen = new Pen(ChartBorderColor, ChartBorderThickness))  // Using ChartBorderColor
                {
                    g.DrawEllipse(chartPen, centerX - chartSize / 2, centerY - chartSize / 2, chartSize, chartSize);
                }

                // Draw the popup if active
                if (showPopup && ShowPopup && mouseIn)
                {
                    SizeF textSize = g.MeasureString(popupText, Font);
                    RectangleF popupRect = new RectangleF(popupLocation.X - textSize.Width / 2, popupLocation.Y - textSize.Height - 10, textSize.Width, textSize.Height);
                    using (GraphicsPath popupPath = Helper.RoundRect(popupRect, (int)(popupRect.Height / 4)))
                    {
                        g.FillPath(new SolidBrush(PopupBackground), popupPath);
                        g.DrawString(popupText, Font, new SolidBrush(PopupText), popupRect);
                    }
                }
            }

            base.OnPaint(e);
        }

        bool mouseIn = false;

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            mouseIn = false;
            Refresh();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            mouseIn = true;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            int centerX = Width / 2;
            int centerY = Height / 2;

            float currentAngle = 0;
            bool pointFound = false;

            for (int i = 0; i < DataPoints.Length; i++)
            {
                float sweepAngle = (privateDataValuePoints[i] / privateTotalValue) * 360f;

                // Check if mouse is within this slice's angle
                float mouseAngle = (float)(Math.Atan2(centerY - e.Y, centerX - e.X) * 180 / Math.PI + 180);

                if (mouseAngle >= currentAngle && mouseAngle <= currentAngle + sweepAngle)
                {
                    showPopup = true;
                    popupLocation = new PointF(e.X, e.Y);
                    popupText = $"{privateDataNamePoints[i]} ({(privateDataValuePoints[i] / privateTotalValue * 100):0.##}%)";
                    Invalidate();
                    pointFound = true;
                    break;
                }

                currentAngle += sweepAngle;
            }

            if (!pointFound)
            {
                showPopup = false;
                Invalidate();
            }
        }
    }
}

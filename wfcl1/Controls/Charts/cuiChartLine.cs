using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CuoreUI.Controls.Charts
{
    [Description("Simple line chart control")]
    public partial class cuiChartLine : UserControl
    {
        public cuiChartLine()
        {
            InitializeComponent();
            this.MouseMove += new MouseEventHandler(OnMouseMove);
            DoubleBuffered = true;
            Font = new Font("Microsoft Yahei UI", 8.25f);
        }

        private float[] privateDataPoints = { 100, 90, 80, 75, 70, 65, 60 };
        private bool usePercent = true;
        private float privateMaxValue = 100;
        private int chartPadding = 40;
        private bool showPopup = false;
        private PointF popupLocation;
        private string popupText;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Refresh();
        }

        public bool hasDuplicate(int[] nums)
        {
            Array.Sort(nums);
            for (int i = 0; i < nums.Length; i++)
            {
                if (nums[i] == nums[i - 1])
                {
                    return true;
                }
            }
            return false;
        }

        private bool privateGradientBackground = true;

        [Browsable(true)]
        [Category("CuoreUI Chart Colors")]
        [Description("Whether the background under the lines should be a gradient.")]
        public bool GradientBackground
        {
            get
            {
                return privateGradientBackground;
            }
            set
            {
                if (privateGradientBackground != value)
                {
                    privateGradientBackground = value;
                    Refresh();
                }
            }
        }

        private bool privateShowLines = false;

        [Browsable(true)]
        [Category("CuoreUI Chart Colors")]
        [Description("Whether the dashed lines should render.")]
        public bool ShowLines
        {
            get
            {
                return privateShowLines;
            }
            set
            {
                if (privateShowLines != value)
                {
                    privateShowLines = value;
                    Refresh();
                }
            }
        }

        private Color privatePointColor = CuoreUI.Drawing.PrimaryColor;

        [Browsable(true)]
        [Category("CuoreUI Chart Colors")]
        [Description("Color of the circular points.")]
        public Color PointColor
        {
            get
            {
                return privatePointColor;
            }
            set
            {
                if (privatePointColor != value)
                {
                    privatePointColor = value;
                    Refresh();
                }

            }
        }

        private Color privateAxisColor = Color.Gray;

        [Browsable(true)]
        [Category("CuoreUI Chart Colors")]
        [Description("Color of the axis (x and y lines).")]
        public Color AxisColor
        {
            get
            {
                return privateAxisColor;
            }
            set
            {
                privateAxisColor = value;
                Refresh();
            }
        }

        string[] xLabelsLong = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        string[] xLabelsShort = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

        [Browsable(true)]
        [Category("CuoreUI Chart Data")]
        [Description("The data points that will be plotted on the line chart.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public float[] DataPoints
        {
            get
            {
                return privateDataPoints;
            }
            set
            {
                privateDataPoints = value;

                if (privateAutoMaxValue)
                {
                    privateMaxValue = privateDataPoints.Max();
                }

                Invalidate();
            }
        }

        private string[] privateCustomXAxis = { };
        [Browsable(true)]
        [Category("CuoreUI Chart Data")]
        [Description("The X axis values, if you don't want to use the weekdays.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public string[] CustomXAxis
        {
            get
            {
                return privateCustomXAxis;
            }
            set
            {
                privateCustomXAxis = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("CuoreUI Chart Layout")]
        [Description("The padding around the chart area.")]
        public int ChartPadding
        {
            get
            {
                return chartPadding;
            }
            set
            {
                chartPadding = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("CuoreUI Chart Layout")]
        [Description("If true, the Y-axis will show percentages. If false, it will show absolute values.")]
        public bool UsePercent
        {
            get
            {
                return usePercent;
            }
            set
            {
                usePercent = value;
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("CuoreUI Chart Layout")]
        [Description("The maximum value for the Y-axis.")]
        public float MaxValue
        {
            get
            {
                return privateMaxValue;
            }
            set
            {
                AutoMaxValue = false;
                privateMaxValue = value;
                Invalidate();
            }
        }

        private Color privateChartLineColor = CuoreUI.Drawing.PrimaryColor;

        [Browsable(true)]
        [Category("CuoreUI Chart Colors")]
        [Description("Color of the line that connects points.")]
        public Color ChartLineColor
        {
            get
            {
                return privateChartLineColor;
            }
            set
            {
                privateChartLineColor = value;
                Refresh();
            }
        }

        private Color privateDayColor = Color.DarkGray;

        [Browsable(true)]
        [Category("CuoreUI Chart Colors")]
        [Description("Color of the day label.")]
        public Color DayColor
        {
            get
            {
                return privateDayColor;
            }
            set
            {
                privateDayColor = value;
                Refresh();
            }
        }

        private bool privateShortDates = true;

        [Browsable(true)]
        [Category("CuoreUI Chart Data")]
        [Description("Whether the dates should be minified.")]
        public bool ShortDates
        {
            get
            {
                return privateShortDates;
            }
            set
            {
                privateShortDates = value;
                Refresh();
            }
        }

        private bool privateAutoMaxValue = true;
        [Browsable(true)]
        [Category("CuoreUI Chart Layout")]
        [Description("Whether the MaxValue should update automatically to fit chart data.")]
        public bool AutoMaxValue
        {
            get
            {
                return privateAutoMaxValue;
            }
            set
            {
                privateAutoMaxValue = value;
                if (value)
                {
                    privateMaxValue = privateDataPoints.Max();
                }
                Invalidate();
            }
        }

        private bool privateUseBezier = false;

        [Browsable(true)]
        [Category("CuoreUI Chart Layout")]
        [Description("Smoothens the lines by using Bezier curves instead.")]
        public bool UseBezier
        {
            get
            {
                return privateUseBezier;
            }
            set
            {
                privateUseBezier = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            int chartWidth = this.Width - chartPadding * 2;
            int chartHeight = this.Height - chartPadding * 2;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            Pen axisPen = new Pen(AxisColor, 1);
            Pen dashedPen = new Pen(Color.FromArgb(64, AxisColor), 1) { DashStyle = DashStyle.Dash };
            Pen linePen = new Pen(ChartLineColor, 2);

            g.DrawLine(axisPen, chartPadding, chartPadding, chartPadding, chartHeight + chartPadding);
            g.DrawLine(axisPen, chartPadding, chartHeight + chartPadding, chartWidth + chartPadding, chartHeight + chartPadding);

            if (privateShowLines)
            {
                for (int i = 1; i <= 5; i++)
                {
                    float y = chartPadding + chartHeight - (i * chartHeight / 5);
                    g.DrawLine(dashedPen, chartPadding, y, chartWidth + chartPadding, y);
                }
            }

            float yScaleFactor = 100f / privateMaxValue;

            if (privateDataPoints.Length > 1)
            {
                using (GraphicsPath linePath = new GraphicsPath())
                {
                    float startX = chartPadding;
                    float startY = chartPadding + chartHeight - (privateDataPoints[0] * yScaleFactor / 100 * chartHeight);
                    linePath.StartFigure();
                    linePath.AddLine(startX, chartHeight + chartPadding, startX, startY);

                    for (int i = 0; i < privateDataPoints.Length - 1; i++)
                    {
                        float x1 = chartPadding + (i * chartWidth / (privateDataPoints.Length - 1));
                        float y1 = chartPadding + chartHeight - (privateDataPoints[i] * yScaleFactor / 100 * chartHeight);
                        float x2 = chartPadding + ((i + 1) * chartWidth / (privateDataPoints.Length - 1));
                        float y2 = chartPadding + chartHeight - (privateDataPoints[i + 1] * yScaleFactor / 100 * chartHeight);

                        if (privateUseBezier)
                        {
                            float cp1X = x1 + (x2 - x1) / 3;
                            float cp1Y = y1;
                            float cp2X = x2 - (x2 - x1) / 3;
                            float cp2Y = y2;

                            g.DrawBezier(linePen, x1, y1, cp1X, cp1Y, cp2X, cp2Y, x2, y2);
                            linePath.AddBezier(x1, y1, cp1X, cp1Y, cp2X, cp2Y, x2, y2);
                        }
                        else
                        {
                            g.DrawLine(linePen, x1, y1, x2, y2);
                            linePath.AddLine(x1, y1, x2, y2);
                        }
                    }

                    float endX = chartPadding + chartWidth;
                    float endY = chartPadding + chartHeight - (privateDataPoints[privateDataPoints.Length - 1] * yScaleFactor / 100 * chartHeight);
                    linePath.AddLine(endX, endY, endX, chartHeight + chartPadding);
                    linePath.CloseFigure();

                    if (GradientBackground)
                    {
                        if (ChartLineColor.A == 255)
                        {
                            using (LinearGradientBrush gradientBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), Color.FromArgb(64, ChartLineColor), Color.Transparent))
                            {
                                g.FillPath(gradientBrush, linePath);
                            }
                        }
                        else
                        {
                            int blend = (64 + ChartLineColor.A) / 2;
                            using (LinearGradientBrush gradientBrush = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), Color.FromArgb(blend, ChartLineColor), Color.Transparent))
                            {
                                g.FillPath(gradientBrush, linePath);
                            }
                        }
                    }
                    else
                    {
                        using (SolidBrush solidBrush = new SolidBrush(Color.FromArgb(32, ChartLineColor)))
                        {
                            g.FillPath(solidBrush, linePath);
                        }
                    }

                    using (SolidBrush pointBrush = new SolidBrush(PointColor))
                    {
                        foreach (var point in privateDataPoints)
                        {
                            float x = chartPadding + (Array.IndexOf(privateDataPoints, point) * chartWidth / (privateDataPoints.Length - 1));
                            float y = chartPadding + chartHeight - (point * yScaleFactor / 100 * chartHeight);
                            g.FillEllipse(pointBrush, x - 4, y - 4, 8, 8);
                        }
                    }

                    if (showPopup)
                    {
                        SizeF textSize = g.MeasureString(popupText, Font);
                        RectangleF popupRect = new RectangleF(popupLocation.X - textSize.Width / 2, popupLocation.Y - textSize.Height - 10, textSize.Width, textSize.Height);
                        using (GraphicsPath popupPath = Helper.RoundRect(popupRect, (int)(popupRect.Height / 4)))
                        {
                            using (SolidBrush popupBrush = new SolidBrush(PopupBackground))
                            {
                                g.FillPath(popupBrush, popupPath);
                            }
                            g.DrawString(popupText, Font, new SolidBrush(PopupText), popupRect.X + 1.5f, popupRect.Y + 1);
                        }
                    }
                }
            }

            axisPen.Dispose();
            dashedPen.Dispose();
            linePen.Dispose();

            DrawLabels(g, chartPadding, chartWidth, chartHeight);

            base.OnPaint(e);
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

        private void DrawLabels(Graphics g, int padding, int width, int height)
        {
            Brush labelBrush = new SolidBrush(DayColor);

            if (usePercent)
            {
                for (int i = 0; i <= 5; i++)
                {
                    float y = padding + height - (i * height / 5);
                    g.DrawString($"{Math.Round(i * 20 * privateMaxValue / 100, 2)}%", Font, labelBrush, padding - 35, y - 7);
                }
            }
            else
            {
                for (int i = 0; i <= 5; i++)
                {
                    float y = padding + height - (i * height / 5);
                    float value = i * privateMaxValue / 5;
                    g.DrawString($"{Math.Round(value, 2)}", Font, labelBrush, padding - 35, y - 7);
                }
            }

            if (privateCustomXAxis.Length > 0)
            {
                for (int i = 0; i < privateCustomXAxis.Length; i++)
                {
                    float x = padding + (i * width / (privateCustomXAxis.Length - 1));
                    g.DrawString(privateCustomXAxis[i], Font, labelBrush, x - 20, padding + height + 5);
                }
            }
            else
            {
                for (int i = 0; i < xLabelsLong.Length; i++)
                {
                    float x = padding + (i * width / (xLabelsLong.Length - 1));
                    if (ShortDates)
                    {
                        g.DrawString(xLabelsShort[i], Font, labelBrush, x - 20, padding + height + 5);
                    }
                    else
                    {
                        g.DrawString(xLabelsLong[i], Font, labelBrush, x - 20, padding + height + 5);
                    }
                }
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            int chartWidth = this.Width - chartPadding * 2;
            int chartHeight = this.Height - chartPadding * 2;
            float yScaleFactor = 100f / privateMaxValue;

            bool pointFound = false;

            for (int i = 0; i < privateDataPoints.Length; i++)
            {
                float x = chartPadding + (i * chartWidth / (privateDataPoints.Length - 1));
                float y = chartPadding + chartHeight - (privateDataPoints[i] * yScaleFactor / 100 * chartHeight);

                if (Math.Abs(e.X - x) < 10 && Math.Abs(e.Y - y) < 10)
                {
                    showPopup = true;

                    if (!popupLocation.Equals(new PointF(x, y)))
                    {
                        popupLocation = new PointF(x, y);
                        popupText = $"{privateDataPoints[i]}";

                        if (usePercent)
                        {
                            popupText += "%";
                        }

                        InvalidatePopupRegion();
                    }

                    pointFound = true;
                    return;
                }
                else
                {
                    InvalidatePopupRegion();
                    showPopup = false;
                }
            }

            if (!pointFound && showPopup)
            {
                showPopup = false;
            }
        }
        private void InvalidatePopupRegion()
        {
            SizeF textSize = CreateGraphics().MeasureString(popupText, Font);

            RectangleF popupRect = new RectangleF(popupLocation.X - textSize.Width / 2, popupLocation.Y - textSize.Height - 10, textSize.Width, textSize.Height + 1);
            using (GraphicsPath popupPath = Helper.RoundRect(popupRect, (int)(popupRect.Height / 4)))
            {
                Region roundedRegion = new Region(popupPath);
                Invalidate(roundedRegion);
            }
        }
    }
}

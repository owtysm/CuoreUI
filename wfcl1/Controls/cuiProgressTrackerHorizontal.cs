using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CuoreUI.Controls
{
    public partial class cuiProgressTrackerHorizontal : Control
    {
        public cuiProgressTrackerHorizontal()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Size = new Size(480, 36);
        }

        private string[] tasks = new string[] { "Task1", "Task2", "Task3", "Task4" };

        [Category("CuoreUI")]
        [Description("Tasks in text separated by new lines.")]
        public string[] Tasks
        {
            get => tasks;
            set
            {
                tasks = value;
                this.Invalidate();
            }
        }

        public int privateTasksProgress = 2;

        [Category("CuoreUI")]
        [Description("How many tasks are completed.")]
        public int TasksProgress
        {
            get
            {
                return privateTasksProgress;
            }
            set
            {
                privateTasksProgress = value;
                Invalidate();
            }
        }

        public int privateLineThickness = 4;

        [Category("CuoreUI")]
        public int LineThickness
        {
            get
            {
                return privateLineThickness;
            }
            set
            {
                privateLineThickness = value;
                Invalidate();
            }
        }

        private bool privateShowSymbols = true;

        [Category("CuoreUI")]
        [Description("Whether to show the checkmark symbol on the completed tasks.")]
        public bool ShowSymbols
        {
            get
            {
                return privateShowSymbols;
            }
            set
            {
                privateShowSymbols = value;
                Invalidate();
            }
        }

        private Color privateCompletedColor = CuoreUI.Drawing.PrimaryColor;

        [Category("CuoreUI")]
        [Description("The primary color of the control, the color of completed tasks and current task.")]
        public Color CompletedColor
        {
            get
            {
                return privateCompletedColor;
            }
            set
            {
                privateCompletedColor = value;
                Invalidate();
            }
        }

        private Color privateCurrentTaskForeColor = Color.FromArgb(128, 128, 128);

        [Category("CuoreUI")]
        [Description("The color of the text of the current task.")]
        public Color CurrentTaskForeColor
        {
            get
            {
                return privateCurrentTaskForeColor;
            }
            set
            {
                privateCurrentTaskForeColor = value;
                Invalidate();
            }
        }

        private Color privateTaskForeColor = Color.FromArgb(128, 128, 128);

        [Category("CuoreUI")]
        [Description("The color of the text for every task other than the current task.")]
        public Color TaskForeColor
        {
            get
            {
                return privateTaskForeColor;
            }
            set
            {
                privateTaskForeColor = value;
                Invalidate();
            }
        }

        private Color privateTrackColor = Color.FromArgb(64, 128, 128, 128);

        [Category("CuoreUI")]
        [Description("The color of the track of the uncompleted tasks.")]
        public Color TrackColor
        {
            get
            {
                return privateTrackColor;
            }
            set
            {
                privateTrackColor = value;
                Invalidate();
            }
        }

        int privateRounding = 10;

        [Category("CuoreUI")]
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

        bool privateAutoRounding = true;

        [Category("CuoreUI")]
        public bool AutoRounding
        {
            get
            {
                return privateAutoRounding;
            }
            set
            {
                privateAutoRounding = value;
                Invalidate();
            }
        }

        [Category("CuoreUI")]
        [Description("Read-only. Returns the name of the current task (as string).")]
        public string CurrentTask
        {
            get
            {
                try
                {
                    return Tasks[TasksProgress - 1];
                }
                catch
                {
                    return "";
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Tasks.Length < 2)
            {
                return;
            }

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            int WantedItemHeight = Height - 1 - Font.Height;

            int PenThicknessCompensation = WantedItemHeight / 8;
            int HalfPenThickness = PenThicknessCompensation / 2;

            int ActualItemHeight = WantedItemHeight - PenThicknessCompensation;

            int itemCount = Tasks.Length;
            int spacing = (Width - (ActualItemHeight * 2)) / (itemCount - 1); // Adjusted spacing

            Point currentItemPosition = new Point(ActualItemHeight, HalfPenThickness);
            Point currentTextRectangle = new Point(0, ActualItemHeight + PenThicknessCompensation + 1);

            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center };
            Brush trackBrush = new SolidBrush(CompletedColor);
            Brush todoBrush = new SolidBrush(TrackColor);

            GraphicsPath RoundedItemPath = null;

            int tempRounding;
            if (AutoRounding)
            {
                tempRounding = ActualItemHeight / 2;
            }
            else
            {
                tempRounding = Math.Min(ActualItemHeight / 2, privateRounding);
            }

            // draw tasks
            for (int i = 0; i < itemCount; i++)
            {
                currentItemPosition.X = ActualItemHeight + (i * spacing) - (i * WantedItemHeight / Tasks.Length) - PenThicknessCompensation;
                currentTextRectangle.X = currentItemPosition.X + ((ActualItemHeight + 1) / 2);

                // current step
                if (i == TasksProgress - 1)
                {
                    RoundedItemPath = Helper.RoundRect(new Rectangle(
                            currentItemPosition.X + (HalfPenThickness / 2) + 1,
                            PenThicknessCompensation + 1,
                            ActualItemHeight - HalfPenThickness - 2,
                            ActualItemHeight - HalfPenThickness - 2), tempRounding - PenThicknessCompensation / 2 - 1);

                    using (Pen p = new Pen(CompletedColor, (LineThickness / 2) - 1))
                    {
                        e.Graphics.DrawPath(p, RoundedItemPath);
                    }

                    using (SolidBrush textBrush = new SolidBrush(CurrentTaskForeColor))
                    {
                        e.Graphics.DrawString(Tasks[i], Font, textBrush, currentTextRectangle, sf);
                    }
                }
                // completed steps
                else if (i < TasksProgress)
                {
                    // save rect for later in case drawing symbols
                    Rectangle tempRect = new Rectangle(currentItemPosition.X, PenThicknessCompensation, ActualItemHeight, ActualItemHeight);

                    RoundedItemPath = Helper.RoundRect(tempRect, tempRounding);
                    e.Graphics.FillPath(trackBrush, RoundedItemPath);

                    // checkmark
                    if (ShowSymbols)
                    {
                        tempRect.Inflate(0, -1);
                        tempRect.Inflate(-(ActualItemHeight / 10), -(ActualItemHeight / 10));
                        using (GraphicsPath checkmarkGP = Helper.Checkmark(tempRect))
                        using (Pen p = new Pen(BackColor, ActualItemHeight / 8) { EndCap = LineCap.Round, StartCap = LineCap.Round })
                        {
                            e.Graphics.DrawPath(p, checkmarkGP);
                        }
                    }

                    using (SolidBrush textBrush = new SolidBrush(TaskForeColor))
                    {
                        e.Graphics.DrawString(Tasks[i], Font, textBrush, currentTextRectangle, sf);
                    }
                }
                // steps yet to be completed
                else
                {
                    RoundedItemPath = Helper.RoundRect(new Rectangle(
                        currentItemPosition.X,
                        PenThicknessCompensation,
                        ActualItemHeight,
                        ActualItemHeight), tempRounding);

                    e.Graphics.FillPath(todoBrush, RoundedItemPath);

                    using (SolidBrush textBrush = new SolidBrush(TaskForeColor))
                    {
                        e.Graphics.DrawString(Tasks[i], Font, textBrush, currentTextRectangle, sf);
                    }
                }

                // lines inbetween
                if (i != itemCount - 1)
                {
                    using (Pen p = new Pen(i < TasksProgress - 1 ? CompletedColor : TrackColor, (LineThickness / 2)) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                    {
                        Point ConnectPoint = currentItemPosition;
                        ConnectPoint.Y += ((ActualItemHeight + PenThicknessCompensation + 1) / 2);
                        ConnectPoint.X += PenThicknessCompensation + ActualItemHeight;

                        Point ConnectPoint2 = ConnectPoint;
                        ConnectPoint2.X = ConnectPoint.X + spacing - (WantedItemHeight / Tasks.Length) - WantedItemHeight - (PenThicknessCompensation * 2) + 1;

                        ConnectPoint.X += PenThicknessCompensation;
                        ConnectPoint2.X -= PenThicknessCompensation;

                        e.Graphics.DrawLine(p, ConnectPoint, ConnectPoint2);
                    }
                }
            }

            RoundedItemPath?.Dispose();

            base.OnPaint(e);
        }
    }
}

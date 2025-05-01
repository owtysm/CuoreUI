using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CuoreUI.Drawing;

namespace CuoreUI.Components
{
    public partial class cuiFormAnimator : Component
    {
        public cuiFormAnimator()
        {
            InitializeComponent();
        }

        public cuiFormAnimator(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        private Form privateTargetForm = null;
        [Category("Animating")]
        public Form TargetForm
        {
            get
            {
                return privateTargetForm;
            }
            set
            {
                if (privateTargetForm != null)
                {
                    privateTargetForm.HandleCreated -= Value_HandleCreated;
                }

                privateTargetForm = value;

                if (value != null)
                {
                    value.HandleCreated += Value_HandleCreated;
                }
            }
        }

        [Category("Animating")]
        public bool AnimateOnStart { get; set; } = true;

        private double privateStartOpacity = 0;

        [Category("Animating Opacity")]
        public double StartOpacity
        {
            get
            {
                return privateStartOpacity;
            }
            set
            {
                privateStartOpacity = ClampOpacity(value);
            }
        }

        private double privateTargetOpacity = 1;

        [Category("Animating Opacity")]
        public double TargetOpacity
        {
            get
            {
                return privateTargetOpacity;
            }
            set
            {
                privateTargetOpacity = ClampOpacity(value);
            }
        }

        private void Value_HandleCreated(object sender, EventArgs e)
        {
            if (AnimateOnStart)
            {
                TargetForm.Opacity = StartOpacity;
                lastKnownOpacity = StartOpacity;

                TargetForm.Shown += TargetForm_Shown;
                TargetForm.VisibleChanged += TargetForm_VisibleChanged;
            }
        }

        double lastKnownOpacity = 0;
        private void TargetForm_VisibleChanged(object sender, EventArgs e)
        {
            if (TargetForm?.Opacity != lastKnownOpacity)
            {
                cancelRequested = true;
            }
        }

        private double ClampOpacity(double targetOpacity)
        {
            return targetOpacity < 0 ? 0 : (targetOpacity > 1 ? 1 : targetOpacity);
        }

        [Category("Animating")]
        public int Duration { get; set; } = 400;

        [Category("Animating")]
        public EasingTypes EasingType
        {
            get; set;
        } = EasingTypes.QuadOut;

        public void PlayAnimation()
        {
            TargetForm.Opacity = StartOpacity;
            lastKnownOpacity = StartOpacity;

            TargetForm_Shown(this, EventArgs.Empty);
        }

        private byte animationsInQueue = 0;
        bool cancelRequested = false;

        private async void TargetForm_Shown(object sender, EventArgs e)
        {
            //Helper.Win32.REFRESH_RATE_OVERRIDE = true;
            //Helper.Win32.SPOOFED_REFRESH_RATE = 20;

            // stopwatch approach is not the best for performance, but it is more reliable than frames
            // im sure we'd rather wait the intended 3000ms rather than 9000ms
            if (TargetForm != null && TargetForm.IsHandleCreated)
            {
                double startOpacity = TargetForm.Opacity;
                double currentOpacity;

                int frameDeltaTime = LazyTimeDelta;
                double durationInMilliseconds = Duration;
                Stopwatch sw = Stopwatch.StartNew();

                animationsInQueue++;
                while (!cancelRequested && animationsInQueue == 1)
                {
                    double elapsedTime = sw.ElapsedMilliseconds;

                    double progress = Math.Min(1.0, elapsedTime / durationInMilliseconds);

                    currentOpacity = startOpacity + (TargetOpacity - startOpacity) * EasingFunctions.FromEasingType(EasingType, progress);

                    if (!cancelRequested && animationsInQueue == 1)
                    {
                        TargetForm.Opacity = currentOpacity;
                    }

                    if (progress >= 1.0)
                    {
                        if (!cancelRequested && animationsInQueue == 1)
                        {
                            TargetForm.Opacity = TargetOpacity;
                        }
                        break;
                    }

                    await Task.Delay(frameDeltaTime);
                }

                cancelRequested = false;
                animationsInQueue--;
                sw.Stop();

                //MessageBox.Show($"Duration: {Duration}ms\nReal Duration: {sw.ElapsedMilliseconds}ms");
            }
        }
    }
}

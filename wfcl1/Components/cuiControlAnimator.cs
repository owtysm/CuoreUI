using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CuoreUI.Controls;
using static CuoreUI.Drawing;

namespace CuoreUI.Components
{
    [Description("Animate your control's location and/or opacity with easing functions")]
    [ToolboxBitmap(typeof(TrackBar))]
    public partial class cuiControlAnimator : Component
    {
        private PaintEventHandler paintHandler;

        public cuiControlAnimator()
        {
            InitializeComponent();
            paintHandler = (sender, e) =>
            {
                if (animationFinished || !AnimateOpacity || currentControlOpacity > 254 || sender is cuiPictureBox)
                    return;

                Rectangle expandedRect = TargetControl.ClientRectangle;
                expandedRect.Inflate(2, 2);

                using (SolidBrush br = new SolidBrush(Color.FromArgb(currentControlOpacity, TargetControl.BackColor)))
                {
                    e.Graphics.FillRectangle(br, expandedRect);
                }
            };

        }

        private Control privateTargetControl = null;

        [Category("CuoreUI")]
        [Description("The control to animate.")]
        public Control TargetControl
        {
            get
            {
                return privateTargetControl;
            }
            set
            {
                privateTargetControl = value;

                if (value != null)
                {
                    if (value is Form && DesignMode)
                    {
                        MessageBox.Show("Hey, You are probably looking for 'cuiFormAnimator' to animate Form.\nYou can still use 'cuiControlAnimator' though!", "CuoreUI");
                        privateTargetControl = null;
                    }
                    else
                    {
                        privateTargetControl.HandleCreated += PrivateTargetControl_HandleCreated;
                    }
                }
            }
        }

        private void PrivateTargetControl_HandleCreated(object sender, EventArgs e)
        {
            if (AnimateOnStart)
            {
                _ = PlayAnimation();
            }
        }

        private int privateDuration = 1000;

        [Category("CuoreUI")]
        [Description("How long the animation should last in milliseconds. (ms)")]
        public int Duration
        {
            get
            {
                return privateDuration;
            }
            set
            {
                privateDuration = value;
            }
        }

        private bool privateAnimateOpacity = false;

        [Category("CuoreUI")]
        [Description("Animates 'opacity' of the control from 0 -> 1.")]
        public bool AnimateOpacity
        {
            get
            {
                return privateAnimateOpacity;
            }
            set
            {
                privateAnimateOpacity = value;
            }
        }

        [Category("CuoreUI")]
        [Description("Choose the easing type that suits the best.")]
        public EasingTypes EasingType
        {
            get;
            set;
        } = EasingTypes.QuadInOut;

        [Category("CuoreUI")]
        [Description("Where the TargetControl should be moved to.")]
        public Point TargetLocation
        {
            get;
            set;
        } = Point.Empty;

        [Category("CuoreUI")]
        [Description("Animate control when first shown on screen.")]
        public bool AnimateOnStart
        {
            get;
            set;
        } = true;

        [Category("CuoreUI")]
        [Description("Either move to TargetLocation or ignore animating location.")]
        public bool AnimateLocation
        {
            get;
            set;
        } = true;

        private OpacityEnum privateTargetOpacity = OpacityEnum.Visible;

        [Category("CuoreUI")]
        [Description("Target opacity (0 - 255) for the control when animation completes.")]
        public OpacityEnum TargetOpacity
        {
            get => privateTargetOpacity;
            set => privateTargetOpacity = value;
        }

        public enum OpacityEnum
        {
            Visible = 255,
            Transparent = 0
        }

        private int startX;
        private int startY;
        private double xDistance;
        private double yDistance;

        private double elapsedTime = 0;
        private bool animating = false;
        private bool animationFinished = true;

        byte currentControlOpacity = 255;

        public async Task PlayAnimation()
        {

            if (animating || TargetControl == null || TargetControl is cuiPictureBox || DesignMode)
                return;
            animating = true; 
            animationFinished = false;

            TargetControl.Paint += paintHandler;

            startX = TargetControl.Left;
            startY = TargetControl.Top;

            xDistance = -(startX - TargetLocation.X);
            yDistance = -(startY - TargetLocation.Y);

            DateTime lastFrameTime = DateTime.Now;

            double durationRatio = Duration / (double)1000;

            //MessageBox.Show(durationRatio.ToString() + $", d:{Duration}, recalc:{Duration/(double)1000}");

            // save now so if its changed mid loop we still use this value
            // later used in the while loop aswell
            bool shouldAnimateLocationNow = AnimateLocation;
            EmergencySetLocation(Duration, shouldAnimateLocationNow);

            bool animateTowardsVisible = TargetOpacity == OpacityEnum.Visible;
            AnimationStarted?.Invoke(this, EventArgs.Empty);

            while (true)
            {

                DateTime rightnow = DateTime.Now;
                double elapsedMilliseconds = (rightnow - lastFrameTime).TotalMilliseconds;

                // uhhhhh this is so weird but it works..
                elapsedTime += (elapsedMilliseconds / Duration);

                if (elapsedTime >= Duration || IsAnimationFinished())
                {
                    animating = false;
                    animationFinished = true;
                    elapsedTime = 0;
                    TargetControl.Paint -= paintHandler;
                    //MessageBox.Show($"{elapsedTime}, {privateDuration}");
                    return;
                }

                double progress = CuoreUI.Drawing.EasingFunctions.FromEasingType(EasingType, elapsedTime, Duration / (double)1000) * durationRatio;

                if (shouldAnimateLocationNow)
                {
                    TargetControl.Left = startX + (int)(xDistance * progress);
                    TargetControl.Top = startY + (int)(yDistance * progress);
                }

                if (AnimateOpacity)
                {
                    if (animateTowardsVisible)
                    {
                        currentControlOpacity = (byte)((1 - (progress * 100)) * 2.5d);
                    }
                    else
                    {
                        currentControlOpacity = (byte)((progress * 100) * 2.5d);
                    }
                    TargetControl.Invalidate();
                }

                lastFrameTime = rightnow;
                await Task.Delay(1000 / Drawing.GetHighestRefreshRate());
            }

        }

        public bool IsAnimationFinished()
        {
            return animationFinished;
        }

        private async void EmergencySetLocation(int Duration, bool shouldAnimateLocationNow)
        {
            animationFinished = false;
            await Task.Delay(Duration + (1000 / Drawing.GetHighestRefreshRate()));

            if (shouldAnimateLocationNow)
            {
                TargetControl.Left = startX + (int)(xDistance);
                TargetControl.Top = startY + (int)(yDistance);
            }

            animationFinished = true;
            animating = false;
            elapsedTime = 0;
            AnimationEnded?.Invoke(this, EventArgs.Empty);
        }

        [Category("CuoreUI")]
        public event EventHandler AnimationEnded;

        [Category("CuoreUI")]
        public event EventHandler AnimationStarted;
    }
}
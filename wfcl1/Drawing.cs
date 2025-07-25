﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace CuoreUI
{
    public static class Drawing
    {
        public static readonly Color PrimaryColor = Color.FromArgb(255, 106, 0);
        public static Color TranslucentPrimaryColor => Color.FromArgb(192, PrimaryColor);

        private static Timer RefreshRateTimer;
        public static event EventHandler FrameDrawn;
        public static event EventHandler TenFramesDrawn;

        static Drawing()
        {
            HandCursorFix.EnableModernCursor();
            Process.GetCurrentProcess().Exited += (e, s) =>
            {
                GlobalMouseHook.Stop();
            };

            RefreshRateTimer = new Timer();
            SetTimerRefreshRate();
        }

        public static int[] GetRefreshRates()
        {
            return Helper.GetRefreshRates();
        }

        public static int GetHighestRefreshRate()
        {
            // 1000 max, 1 minimum, a simple "1000 / 1001" or "1000 / 0" may crash the whole app
            return Math.Min(Math.Max(1, Helper.GetHighestRefreshRate()), 1000);
        }

        public class TimeDeltaInstance
        {
            private Stopwatch stopwatch = Stopwatch.StartNew();
            private long lastElapsedTicks;

            public float TimeDelta
            {
                get
                {
                    long currentElapsedTicks = stopwatch.ElapsedTicks;
                    long deltaTicks = currentElapsedTicks - lastElapsedTicks;
                    lastElapsedTicks = currentElapsedTicks;

                    float deltaSeconds = (float)deltaTicks / Stopwatch.Frequency;
                    return deltaSeconds * 100f;
                }
            }
        }

        public static int LazyTimeDelta
        {
            get
            {
                return 1000 / GetHighestRefreshRate();
            }
        }

        static Timer refreshRefresher = new Timer();
        static byte frame = 0;

        private static void SetTimerRefreshRate()
        {
            RefreshRateTimer.Interval = 1000 / GetHighestRefreshRate();

            if (!RefreshRateTimer.Enabled)
            {
                RefreshRateTimer.Start();
                RefreshRateTimer.Tick += (sender, args) =>
                {
                    FrameDrawn?.Invoke(null, EventArgs.Empty);
                    frame++;
                    if (frame >= 10)
                    {
                        frame = 0;
                        TenFramesDrawn?.Invoke(null, EventArgs.Empty);
                    }
                };
            }

            if (!refreshRefresher.Enabled)
            {
                refreshRefresher.Interval = 1000;
                refreshRefresher.Start();
                refreshRefresher.Tick += (sender, args) =>
                {
                    RefreshRateTimer.Interval = 1000 / GetHighestRefreshRate();
                };
            }
        }
        static int ClampColor(int i)
        {
            if (i < 0) return 0;
            if (i > 255) return 255;
            return i;
        }

        // https://stackoverflow.com/questions/1335426/is-there-a-built-in-c-net-system-api-for-hsv-to-rgb
        // https://stackoverflow.com/a/1626232
        // https://stackoverflow.com/users/12971/greg
        public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = ClampColor(Convert.ToInt32(value));
            int p = ClampColor(Convert.ToInt32(value * (1 - saturation)));
            int q = ClampColor(Convert.ToInt32(value * (1 - f * saturation)));
            int t = ClampColor(Convert.ToInt32(value * (1 - (1 - f) * saturation)));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        public static class Imaging
        {
            public static Bitmap TintBitmap(Bitmap originalBitmap, Color tintColor)
            {
                if (originalBitmap == null)
                {
                    return null;
                }

                Bitmap tintedBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);

                using (Graphics graphics = Graphics.FromImage(tintedBitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    float r = tintColor.R / 255f;
                    float g = tintColor.G / 255f;
                    float b = tintColor.B / 255f;
                    float a = tintColor.A / 255f;

                    ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                    {
                    new float[]{ r, 0, 0, 0, 0 },
                      new float[]{0, g, 0, 0, 0 },
                      new float[]{0, 0, b, 0, 0 },
                      new float[]{0, 0, 0, a, 0 },
                      new float[]{0, 0, 0, 0, 1 }
                    });

                    ImageAttributes imageAttributes = new ImageAttributes();
                    imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    graphics.DrawImage(originalBitmap, new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height),
                                0, 0, originalBitmap.Width, originalBitmap.Height,
                                GraphicsUnit.Pixel, imageAttributes);
                }

                return tintedBitmap;
            }

            public static class ImageBlurs
            {
                public static class GaussianBlur
                {
                    public unsafe static void Apply(ref Bitmap bitmap, float radius)
                    {
                        Blurs.GaussianBlur.Apply(ref bitmap, radius);
                    }
                }

                public static class QuadraticBlur
                {
                    public unsafe static void Apply(ref Bitmap bitmap, float radius)
                    {
                        Blurs.QuadraticBlur.Apply(ref bitmap, radius);
                    }
                }
            }
        }

        public enum EasingTypes
        {
            QuadIn,
            QuadOut,
            QuadInOut,
            QuartIn,
            QuartOut,
            QuartInOut,
            QuintIn,
            QuintOut,
            QuintInOut,
            ExpoIn,
            ExpoOut,
            ExpoInOut,
            BackIn,
            BackOut,
            BackInOut,
            SextIn,
            SextOut,
            SextInOut,
        }

        public static class EasingFunctions
        {
            public static double FromEasingType(EasingTypes easingType, double time, double duration = 1, double backOvershoot = 1.70158)
            {
                switch (easingType)
                {
                    case EasingTypes.QuadIn:
                        return Quad.In(time, duration);
                    case EasingTypes.QuadOut:
                        return Quad.Out(time, duration);
                    case EasingTypes.QuadInOut:
                        return Quad.InOut(time, duration);

                    case EasingTypes.QuartIn:
                        return Quart.In(time, duration);
                    case EasingTypes.QuartOut:
                        return Quart.Out(time, duration);
                    case EasingTypes.QuartInOut:
                        return Quart.InOut(time, duration);

                    case EasingTypes.QuintIn:
                        return Quint.In(time, duration);
                    case EasingTypes.QuintOut:
                        return Quint.Out(time, duration);
                    case EasingTypes.QuintInOut:
                        return Quint.InOut(time, duration);

                    case EasingTypes.ExpoIn:
                        return Expo.In(time, duration);
                    case EasingTypes.ExpoOut:
                        return Expo.Out(time, duration);
                    case EasingTypes.ExpoInOut:
                        return Expo.InOut(time, duration);

                    case EasingTypes.BackIn:
                        return Back.In(time, duration, backOvershoot);
                    case EasingTypes.BackOut:
                        return Back.Out(time, duration, backOvershoot);
                    case EasingTypes.BackInOut:
                        return Back.InOut(time, duration, backOvershoot);

                    case EasingTypes.SextIn:
                        return Sext.In(time, duration);
                    case EasingTypes.SextOut:
                        return Sext.Out(time, duration);
                    case EasingTypes.SextInOut:
                        return Sext.InOut(time, duration);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(easingType), easingType, null);
                }
            }

            public static class Quad
            {
                public static double In(double time, double duration = 1)
                {
                    return (time * time) / duration;
                }

                public static double Out(double time, double duration = 1)
                {
                    return (time * (2 - time)) / duration;
                }

                public static double InOut(double time, double duration = 1)
                {
                    if (time < 0.5)
                    {
                        return (2 * time * time) / duration;
                    }
                    return (-1 + (4 - 2 * time) * time) / duration;
                }
            }

            public static class Quart
            {
                public static double In(double time, double duration = 1)
                {
                    return (time * time * time * time) / duration;
                }

                public static double Out(double time, double duration = 1)
                {
                    return (1 - Math.Pow(1 - time, 4)) / duration;
                }

                public static double InOut(double time, double duration = 1)
                {
                    if (time < 0.5)
                    {
                        return (8 * time * time * time * time) / duration;
                    }
                    return (1 - Math.Pow(-2 * time + 2, 4) / 2) / duration;
                }
            }

            public static class Quint
            {
                public static double In(double time, double duration = 1)
                {
                    return (time * time * time * time * time) / duration;
                }

                public static double Out(double time, double duration = 1)
                {
                    return (1 - Math.Pow(1 - time, 5)) / duration;
                }

                public static double InOut(double time, double duration = 1)
                {
                    if (time < 0.5)
                    {
                        return (16 * time * time * time * time * time) / duration;
                    }
                    return (1 - Math.Pow(-2 * time + 2, 5) / 2) / duration;
                }
            }

            public static class Sext
            {
                public static double In(double time, double duration = 1)
                {
                    return Math.Pow(time, 6) / duration;
                }

                public static double Out(double time, double duration = 1)
                {
                    return (1 - Math.Pow(1 - time, 6)) / duration;
                }

                public static double InOut(double time, double duration = 1)
                {
                    if (time < 0.5)
                    {
                        return (32 * Math.Pow(time, 6)) / duration;
                    }
                    return (1 - Math.Pow(-2 * time + 2, 6) / 2) / duration;
                }
            }

            public static class Expo
            {
                public static double In(double time, double duration = 1)
                {
                    return (time == 0 ? 0 : Math.Pow(2, 10 * time - 10)) / duration;
                }

                public static double Out(double time, double duration = 1)
                {
                    return (time == 1 ? 1 : 1 - Math.Pow(2, -10 * time)) / duration;
                }

                public static double InOut(double time, double duration = 1)
                {
                    if (time == 0)
                    {
                        return 0;
                    }
                    if (time == 1)
                    {
                        return 1;
                    }
                    if (time < 0.5)
                    {
                        return (Math.Pow(2, 20 * time - 10) / 2) / duration;
                    }
                    return ((2 - Math.Pow(2, -20 * time + 10)) / 2) / duration;
                }
            }

            public static class Back
            {
                public static double c1 = 1.70158;
                private static double c2 = c1 * 1.525;
                private static double c3 = c1 + 1;

                private static void RecalculateOvershoot(double backOvershoot)
                {
                    if (c1 != backOvershoot)
                    {
                        c1 = backOvershoot;
                        c2 = c1 * 1.525;
                        c3 = c1 + 1;
                    }
                }

                public static double In(double time, double duration = 1, double backOvershoot = 1.70158)
                {
                    RecalculateOvershoot(backOvershoot);
                    return (c3 * time * time * time - c1 * time * time) / duration;
                }

                public static double Out(double time, double duration = 1, double backOvershoot = 1.70158)
                {
                    RecalculateOvershoot(backOvershoot);
                    return (1 + c3 * Math.Pow(time - 1, 3) + c1 * Math.Pow(time - 1, 2)) / duration;
                }

                public static double InOut(double time, double duration = 1, double backOvershoot = 1.70158)
                {
                    RecalculateOvershoot(backOvershoot);
                    if (time < 0.5)
                    {
                        return ((Math.Pow(2 * time, 2) * ((c2 + 1) * 2 * time - c2)) / 2) / duration;
                    }
                    return ((Math.Pow(2 * time - 2, 2) * ((c2 + 1) * (time * 2 - 2) + c2) + 2) / 2) / duration;
                }
            }
        }
    }
}

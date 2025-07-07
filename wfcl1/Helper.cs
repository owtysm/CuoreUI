﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static CuoreUI.Helper.Win32;

namespace CuoreUI
{
    public static class Helper
    {
        static Helper()
        {
            HandCursorFix.EnableModernCursor();
        }

        public static int[] GetRefreshRates()
        {
            return Win32.GetRefreshRates();
        }

        public static int GetHighestRefreshRate()
        {
            return Win32.GetRefreshRate();
        }

        public static GraphicsPath RoundHexagon(Rectangle bounds, float rounding)
        {
            GraphicsPath path = new GraphicsPath();

            // so it doesnt crash (negative values also dont crash so thats cool)
            rounding = Math.Min(rounding, Math.Min(bounds.Width, bounds.Height) / 4f);

            PointF[] points = new PointF[6];
            float width = bounds.Width;
            float height = bounds.Height;

            points[0] = new PointF(bounds.X + width / 2, bounds.Y); // t
            points[1] = new PointF(bounds.X + width, bounds.Y + height / 4); // tr
            points[2] = new PointF(bounds.X + width, bounds.Y + 3 * height / 4); // br
            points[3] = new PointF(bounds.X + width / 2, bounds.Y + height); // b
            points[4] = new PointF(bounds.X, bounds.Y + 3 * height / 4); // bl
            points[5] = new PointF(bounds.X, bounds.Y + height / 4); // tl

            for (int i = 0; i < points.Length; i++)
            {
                // i did Beziers becuase Arcs are PAINN

                PointF current = points[i];
                PointF previous = points[(i - 1 + points.Length) % points.Length];
                PointF next = points[(i + 1) % points.Length];

                PointF dirToPrev = Normalize(new PointF(previous.X - current.X, previous.Y - current.Y));
                PointF dirToNext = Normalize(new PointF(next.X - current.X, next.Y - current.Y));

                PointF arcStart = new PointF(current.X + dirToPrev.X * rounding, current.Y + dirToPrev.Y * rounding);
                PointF arcEnd = new PointF(current.X + dirToNext.X * rounding, current.Y + dirToNext.Y * rounding);

                PointF control1 = new PointF(current.X + dirToPrev.X * (rounding / 2), current.Y + dirToPrev.Y * (rounding / 2));
                PointF control2 = new PointF(current.X + dirToNext.X * (rounding / 2), current.Y + dirToNext.Y * (rounding / 2));

                if (i == 0)
                {
                    path.StartFigure();
                }

                path.AddBezier(arcStart, control1, control2, arcEnd);
            }

            path.CloseFigure();
            return path;
        }
        public static PointF Normalize(PointF point)
        {
            float length = (float)Math.Sqrt(point.X * point.X + point.Y * point.Y);
            return new PointF(point.X / length, point.Y / length);
        }

        public static GraphicsPath RoundRect(Rectangle rectangle, int borderRadius)
        {
            return RoundRect(rectangle, new Padding(borderRadius));
        }

        public static GraphicsPath RoundRect(RectangleF rectangle, int borderRadius)
        {
            return RoundRect(rectangle, new Padding(borderRadius));
        }

        public static GraphicsPath RoundRect(RectangleF rectangle, Padding borderRadius)
        {
            GraphicsPath path = new GraphicsPath();

            void AddArc(float x, float y, float diameter, float startAngle, float sweepAngle)
            {
                if (diameter > 0)
                {
                    RectangleF arc = new RectangleF(x, y, diameter, diameter);
                    path.AddArc(arc, startAngle, sweepAngle);
                }
                else
                {
                    float small = 0.000001f;
                    RectangleF arc = new RectangleF(x, y, small, small);
                    path.AddArc(arc, startAngle, sweepAngle);
                }
            }

            float topLeftDiameter = borderRadius.Top * 2;
            AddArc(rectangle.X, rectangle.Y, topLeftDiameter, 180, 90);

            float topRightDiameter = borderRadius.Left * 2;
            AddArc(rectangle.Right - topRightDiameter, rectangle.Y, topRightDiameter, 270, 90);

            float bottomRightDiameter = borderRadius.Bottom * 2;
            AddArc(rectangle.Right - bottomRightDiameter, rectangle.Bottom - bottomRightDiameter, bottomRightDiameter, 0, 90);

            float bottomLeftDiameter = borderRadius.Right * 2;
            AddArc(rectangle.X, rectangle.Bottom - bottomLeftDiameter, bottomLeftDiameter, 90, 90);

            path.CloseFigure();

            return path;
        }

        public static GraphicsPath Checkmark(Rectangle area)
        {
            GraphicsPath path = new GraphicsPath();

            Point[] points = new Point[]
            {
            new Point(area.Left + (int)(area.Width * 0.25), area.Top + (int)(area.Height * 0.5)),
            new Point(area.Left + (int)(area.Width * 0.45), area.Top + (int)(area.Height * 0.7)),
            new Point(area.Right - (int)(area.Width * 0.3), area.Top + (int)(area.Height * 0.3))
            };

            path.AddLines(points);

            return path;
        }

        public static GraphicsPath Checkmark(RectangleF area, Point symbolsOffset)
        {
            GraphicsPath path = new GraphicsPath();

            area.Offset(symbolsOffset);

            PointF[] points = new PointF[]
            {
            new PointF(area.Left + (int)(area.Width * 0.25), area.Top + (int)(area.Height * 0.5)),
            new PointF(area.Left + (int)(area.Width * 0.45), area.Top + (int)(area.Height * 0.7)),
            new PointF(area.Right - (int)(area.Width * 0.3), area.Top + (int)(area.Height * 0.3))
            };

            path.AddLines(points);

            return path;
        }

        public static GraphicsPath Crossmark(Rectangle rect)
        {
            Rectangle area = rect;
            int WidthBeforeScale = area.Width;
            area.Width = (int)Math.Round(area.Width * 0.7f, 0);
            area.Height = area.Width;

            int WidthAfterScale = area.Width;
            int WidthDifference = WidthBeforeScale - WidthAfterScale;

            area.Offset(WidthDifference / 2, 1 + (WidthDifference / 2));

            GraphicsPath path = new GraphicsPath();

            Point[] points = new Point[]
            {
            new Point(area.Left, area.Top),
            new Point(area.Right, area.Bottom)
            };

            path.AddLines(points);

            GraphicsPath path2 = new GraphicsPath();

            Point[] points2 = new Point[]
            {
            new Point(area.Left, area.Bottom),
            new Point(area.Right, area.Top)
            };

            path2.AddLines(points2);

            path.AddPath(path2, false);

            return path;
        }

        public static GraphicsPath Crossmark(RectangleF rect, Point symbolsOffset)
        {
            RectangleF area = rect;
            area.Offset(symbolsOffset);
            float WidthBeforeScale = area.Width;
            area.Width = (int)Math.Round(area.Width * 0.7f, 0);
            area.Height = area.Width;

            float WidthAfterScale = area.Width;
            float WidthDifference = WidthBeforeScale - WidthAfterScale;

            area.Offset(WidthDifference / 2, 1 + (WidthDifference / 2));

            GraphicsPath path = new GraphicsPath();

            PointF[] points = new PointF[]
            {
            new PointF(area.Left, area.Top),
            new PointF(area.Right, area.Bottom)
            };

            path.AddLines(points);

            GraphicsPath path2 = new GraphicsPath();

            PointF[] points2 = new PointF[]
            {
            new PointF(area.Left, area.Bottom),
            new PointF(area.Right, area.Top)
            };

            path2.AddLines(points2);

            path.AddPath(path2, false);

            return path;
        }

        public static GraphicsPath Plus(Rectangle rect)
        {
            Rectangle area = rect;
            int widthBeforeScale = area.Width;
            area.Width = (int)Math.Round(area.Width * 0.7f, 0);
            area.Height = area.Width;

            int widthAfterScale = area.Width;
            int widthDifference = widthBeforeScale - widthAfterScale;

            area.Offset(widthDifference / 2, 1 + (widthDifference / 2));

            GraphicsPath path = new GraphicsPath();

            Point[] horizontalPoints = new Point[]
{
        new Point(area.Left, area.Top + (area.Height / 2)),
        new Point(area.Right, area.Top + (area.Height / 2))
};

            path.AddLines(horizontalPoints);

            GraphicsPath path2 = new GraphicsPath();

            Point[] verticalPoints = new Point[]
{
        new Point(area.Left + (area.Width / 2), area.Top),
        new Point(area.Left + (area.Width / 2), area.Bottom)
};

            path2.AddLines(verticalPoints);
            path.AddPath(path2, false);

            return path;
        }

        private static int arrowThicknessHardcoded = 2;

        public static GraphicsPath LeftArrow(Rectangle rect)
        {
            rect.Height = rect.Width;
            rect.Width = rect.Width / 2;

            GraphicsPath path = new GraphicsPath();
            path.AddLine(rect.Right, rect.Top, rect.Left, rect.Bottom / 2);
            path.AddLine(rect.Left, rect.Bottom / 2, rect.Right, rect.Bottom);

            Matrix translateMatrix = new Matrix();
            translateMatrix.Translate(3 + (arrowThicknessHardcoded * 2), 4 + (arrowThicknessHardcoded + rect.Height / 2));
            translateMatrix.Scale(1, 0.9f);
            translateMatrix.Scale(0.6f, 0.6f);
            path.Transform(translateMatrix);

            return path;
        }

        public static GraphicsPath RightArrow(Rectangle rect)
        {
            rect.Height = rect.Width;
            rect.Width = rect.Width / 2;

            rect.Offset(rect.Width - arrowThicknessHardcoded, 0);

            GraphicsPath path = new GraphicsPath();
            path.AddLine(rect.Left, rect.Top, rect.Right, rect.Bottom / 2);
            path.AddLine(rect.Right, rect.Bottom / 2, rect.Left, rect.Bottom);

            Matrix translateMatrix = new Matrix();
            translateMatrix.Translate(13 + (-arrowThicknessHardcoded * 2), 4 + (arrowThicknessHardcoded + rect.Height / 2));
            translateMatrix.Scale(1, 0.9f);
            translateMatrix.Scale(0.6f, 0.6f);
            path.Transform(translateMatrix);

            return path;
        }

        public static GraphicsPath LeftArrowtest(Rectangle rectangle)
        {
            GraphicsPath path = new GraphicsPath();

            Point[] points =
            {
            new Point(rectangle.Right, rectangle.Top),
            new Point(rectangle.Left, rectangle.Top + rectangle.Height / 2),
            new Point(rectangle.Right, rectangle.Bottom)
        };

            path.AddPolygon(points);

            return path;
        }

        public static GraphicsPath DownArrow(Rectangle rect)
        {
            GraphicsPath path = new GraphicsPath();

            Point[] points =
            {
            new Point(rect.Left, rect.Top),
            new Point(rect.Left + rect.Width / 2, rect.Bottom),
            new Point(rect.Right, rect.Top)
        };

            path.AddPolygon(points);

            return path;
        }

        public static GraphicsPath Star(float centerX, float centerY, float outerRadius, float innerRadius, int numPoints)
        {
            if (numPoints % 2 == 0 || numPoints < 5)
            {
                throw new ArgumentException("Number of points must be an odd number and greater than or equal to 5.");
            }

            var path = new GraphicsPath();
            float angleIncrement = 360f / numPoints;
            float currentAngle = -90f;
            PointF[] points = new PointF[numPoints * 2];

            for (int i = 0; i < numPoints * 2; i += 2)
            {
                points[i] = PointOnCircle(centerX, centerY, outerRadius, currentAngle);
                points[i + 1] = PointOnCircle(centerX, centerY, innerRadius, currentAngle + angleIncrement / 2);
                currentAngle += angleIncrement;
            }

            path.AddPolygon(points);

            return path;
        }

        private static PointF PointOnCircle(float centerX, float centerY, float radius, float angleInDegrees)
        {
            float angleInRadians = (float)(angleInDegrees * Math.PI / 180.0);
            float x = centerX + radius * (float)Math.Cos(angleInRadians);
            float y = centerY + radius * (float)Math.Sin(angleInRadians);
            return new PointF(x, y);
        }

        internal static GraphicsPath UpArrow(Rectangle rect)
        {
            GraphicsPath path = new GraphicsPath();

            Point[] points =
            {
            new Point(rect.Left, rect.Bottom),
            new Point(rect.Left + rect.Width / 2, rect.Top),
            new Point(rect.Right, rect.Bottom)
        };

            path.AddPolygon(points);

            return path;
        }

        public static PointF ClosestPointOnSegment(PointF p, PointF a, PointF b)
        {
            var ap = new PointF(p.X - a.X, p.Y - a.Y);
            var ab = new PointF(b.X - a.X, b.Y - a.Y);
            float ab2 = ab.X * ab.X + ab.Y * ab.Y;
            float dot = ap.X * ab.X + ap.Y * ab.Y;
            float t = Math.Max(0, Math.Min(1, ab2 == 0 ? 0 : dot / ab2));
            return new PointF(a.X + ab.X * t, a.Y + ab.Y * t);
        }

        public static PointF ClosestPointOnTriangle(PointF p, PointF a, PointF b, PointF c)
        {
            var ab = ClosestPointOnSegment(p, a, b);
            var bc = ClosestPointOnSegment(p, b, c);
            var ca = ClosestPointOnSegment(p, c, a);

            float d1 = DistanceSquared(p, ab);
            float d2 = DistanceSquared(p, bc);
            float d3 = DistanceSquared(p, ca);

            return d1 < d2 && d1 < d3 ? ab : (d2 < d3 ? bc : ca);
        }
        public static float DistanceSquared(PointF p1, PointF p2)
        {
            float dx = p1.X - p2.X;
            float dy = p1.Y - p2.Y;
            return dx * dx + dy * dy;
        }


        public static PointF RotatePoint(PointF origin, PointF point, float angleDegrees)
        {
            double angleRadians = angleDegrees * Math.PI / 180.0;
            double cosA = Math.Cos(angleRadians);
            double sinA = Math.Sin(angleRadians);

            float dx = point.X - origin.X;
            float dy = point.Y - origin.Y;

            float xNew = (float)(dx * cosA - dy * sinA) + origin.X;
            float yNew = (float)(dx * sinA + dy * cosA) + origin.Y;

            return new PointF(xNew, yNew);
        }

        public static bool PointInTriangle(PointF p, PointF p0, PointF p1, PointF p2)
        {
            float s = p0.Y * p2.X - p0.X * p2.Y + (p2.Y - p0.Y) * p.X + (p0.X - p2.X) * p.Y;
            float t = p0.X * p1.Y - p0.Y * p1.X + (p0.Y - p1.Y) * p.X + (p1.X - p0.X) * p.Y;

            if ((s < 0) != (t < 0))
                return false;

            float A = -p1.Y * p2.X + p0.Y * (p2.X - p1.X) + p0.X * (p1.Y - p2.Y) + p1.X * p2.Y;
            return A < 0 ? (s <= 0 && s + t >= A) : (s >= 0 && s + t <= A);
        }

        public static (double X, double Y, double Z) BarycentricCoords(PointF p, PointF a, PointF b, PointF c)
        {
            double denom = (b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y);
            double w1 = ((b.Y - c.Y) * (p.X - c.X) + (c.X - b.X) * (p.Y - c.Y)) / denom;
            double w2 = ((c.Y - a.Y) * (p.X - c.X) + (a.X - c.X) * (p.Y - c.Y)) / denom;
            double w3 = 1 - w1 - w2;
            return (w1, w2, w3);
        }

        public static void ToggleFormVisibilityWithoutActivating(Form form, bool show)
        {
            if (form == null || form.IsDisposed)
                return;

            if (show)
            {
                if (!form.Visible)
                    NativeMethods.ShowWindow(form.Handle, NativeMethods.SW_SHOWNOACTIVATE);
            }
            else
            {
                if (form.Visible)
                    NativeMethods.ShowWindow(form.Handle, NativeMethods.SW_HIDE);
            }
        }

        public static class Win32
        {
            [StructLayout(LayoutKind.Sequential)]
            struct OSVERSIONINFOEX
            {
                public int dwOSVersionInfoSize;
                public int dwMajorVersion;
                public int dwMinorVersion;
                public int dwBuildNumber;
                public int dwPlatformId;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
                public string szCSDVersion;
                public ushort wServicePackMajor;
                public ushort wServicePackMinor;
                public ushort wSuiteMask;
                public byte wProductType;
                public byte wReserved;
            }

            [DllImport("ntdll.dll", SetLastError = true)]
            static extern int RtlGetVersion(ref OSVERSIONINFOEX versionInfo);

            public static bool IsWindows11()
            {
                var osVersion = new OSVERSIONINFOEX();
                osVersion.dwOSVersionInfoSize = Marshal.SizeOf(osVersion);
                int status = RtlGetVersion(ref osVersion);

                if (status == 0)
                {
                    return osVersion.dwMajorVersion == 10 && osVersion.dwBuildNumber >= 22000;
                }
                else
                {
                    return false;
                }
            }

            public const uint FLASHW_ALL = 1 | 2;
            public const uint FLASHW_TIMERNOFG = 12;

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

            [StructLayout(LayoutKind.Sequential)]
            public struct FLASHWINFO
            {
                public uint cbSize;
                public IntPtr hwnd;
                public uint dwFlags;
                public uint uCount;
                public uint dwTimeout;
            }

            [DllImport("user32.dll")]
            public static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

            public const int WM_SYSCOMMAND = 0x0112;
            public const int SC_CLOSE = 0xF060;

            public const int GWL_EXSTYLE = -20;
            public const int WS_EX_TRANSPARENT = 0x00000020;
            public const int WS_EX_LAYERED = 0x00080000;
            public const int WS_EX_NOACTIVATE = 0x08000000;

            [StructLayout(LayoutKind.Sequential)]
            public struct DEVMODE
            {
                private const int CCHDEVICENAME = 32;
                private const int CCHFORMNAME = 32;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
                public string dmDeviceName;
                public short dmSpecVersion;
                public short dmDriverVersion;
                public short dmSize;
                public short dmDriverExtra;
                public int dmFields;
                public int dmPositionX;
                public int dmPositionY;
                public int dmDisplayOrientation;
                public int dmDisplayFixedOutput;
                public short dmColor;
                public short dmDuplex;
                public short dmYResolution;
                public short dmTTOption;
                public short dmCollate;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
                public string dmFormName;
                public short dmLogPixels;
                public int dmBitsPerPel;
                public int dmPelsWidth;
                public int dmPelsHeight;
                public int dmDisplayFlags;
                public int dmDisplayFrequency;
                public int dmICMMethod;
                public int dmICMIntent;
                public int dmMediaType;
                public int dmDitherType;
                public int dmReserved1;
                public int dmReserved2;
                public int dmPanningWidth;
                public int dmPanningHeight;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct DISPLAY_DEVICE
            {
                public int cb;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
                public string DeviceName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
                public string DeviceString;
                public int StateFlags;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
                public string DeviceID;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
                public string DeviceKey;
            }

            [DllImport("user32.dll")]
            private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

            [DllImport("user32.dll")]
            private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

            public static bool REFRESH_RATE_OVERRIDE = false;
            public static int SPOOFED_REFRESH_RATE = 60;

            public static int GetRefreshRate()
            {
                if (REFRESH_RATE_OVERRIDE)
                {
                    return SPOOFED_REFRESH_RATE;
                }

                DISPLAY_DEVICE d = new DISPLAY_DEVICE();
                d.cb = Marshal.SizeOf(d);
                DEVMODE vDevMode = new DEVMODE();
                vDevMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

                uint deviceIndex = 0;
                int maxRefreshRate = 1;

                while (EnumDisplayDevices(null, deviceIndex, ref d, 0))
                {
                    if (EnumDisplaySettings(d.DeviceName, -1, ref vDevMode))
                    {
                        int refreshRate = vDevMode.dmDisplayFrequency;
                        if (refreshRate > maxRefreshRate)
                        {
                            maxRefreshRate = refreshRate;
                        }
                    }
                    deviceIndex++;
                }

                return maxRefreshRate;
            }

            public static int[] GetRefreshRates()
            {
                List<int> refreshRates = new List<int>();
                DISPLAY_DEVICE d = new DISPLAY_DEVICE();
                d.cb = Marshal.SizeOf(d);
                DEVMODE vDevMode = new DEVMODE();
                vDevMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));

                uint deviceIndex = 0;
                while (EnumDisplayDevices(null, deviceIndex, ref d, 0))
                {
                    if (EnumDisplaySettings(d.DeviceName, -1, ref vDevMode))
                    {
                        refreshRates.Add(vDevMode.dmDisplayFrequency);
                    }
                    deviceIndex++;
                }

                return refreshRates.ToArray();
            }

            internal static class NativeMethods
            {
                public const int GWL_EXSTYLE = -20;
                public const int WS_EX_LAYERED = 0x80000;

                [System.Runtime.InteropServices.DllImport("user32.dll")]
                public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

                [System.Runtime.InteropServices.DllImport("user32.dll")]
                public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

                [DllImport("user32.dll")]
                public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

                public const int SW_SHOWNOACTIVATE = 4;
                public const int SW_HIDE = 0;
            }

            internal static class PerPixelAlphaBlend
            {
                public unsafe static void SetBitmap(Bitmap bitmap, byte opacity, int left, int top, IntPtr handle)
                {
                    if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                        throw new ApplicationException("The bitmap must be 32ppp with alpha-channel.");

                    IntPtr screenDc = Win32.GetDC(IntPtr.Zero);
                    IntPtr memDc = Win32.CreateCompatibleDC(screenDc);
                    IntPtr hBitmap = IntPtr.Zero;
                    IntPtr oldBitmap = IntPtr.Zero;

                    try
                    {
                        hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                        oldBitmap = Win32.SelectObject(memDc, hBitmap);

                        Win32.Size size = new Win32.Size(bitmap.Width, bitmap.Height);
                        Win32.Point topPos = new Win32.Point(left, top);
                        Win32.Point pointSource = new Win32.Point(0, 0);
                        Win32.BLENDFUNCTION blend = new Win32.BLENDFUNCTION
                        {
                            BlendOp = Win32.AC_SRC_OVER,
                            BlendFlags = 0,
                            SourceConstantAlpha = opacity,
                            AlphaFormat = Win32.AC_SRC_ALPHA
                        };

                        Win32.UpdateLayeredWindow(handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, Win32.ULW_ALPHA);

                    }
                    finally
                    {
                        Win32.ReleaseDC(IntPtr.Zero, screenDc);
                        if (hBitmap != IntPtr.Zero)
                        {
                            Win32.SelectObject(memDc, oldBitmap);
                            Win32.DeleteObject(hBitmap);
                        }

                        Win32.DeleteDC(memDc);
                    }
                }

                internal static class Win32
                {
                    public enum Bool
                    {
                        False = 0,
                        True
                    };

                    [StructLayout(LayoutKind.Sequential)]
                    public struct Point
                    {
                        public Int32 x;
                        public Int32 y;

                        public Point(Int32 x, Int32 y)
                        {
                            this.x = x;
                            this.y = y;
                        }
                    }

                    [StructLayout(LayoutKind.Sequential)]
                    public struct Size
                    {
                        public Int32 cx;
                        public Int32 cy;

                        public Size(Int32 cx, Int32 cy)
                        {
                            this.cx = cx;
                            this.cy = cy;
                        }
                    }

                    [StructLayout(LayoutKind.Sequential, Pack = 1)]
                    public struct BLENDFUNCTION
                    {
                        public byte BlendOp;
                        public byte BlendFlags;
                        public byte SourceConstantAlpha;
                        public byte AlphaFormat;
                    }

                    public const Int32 ULW_ALPHA = 0x00000002;

                    public const byte AC_SRC_OVER = 0x00;
                    public const byte AC_SRC_ALPHA = 0x01;

                    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
                    public static extern Bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pprSrc, Int32 crKey, ref BLENDFUNCTION pblend, Int32 dwFlags);

                    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
                    public static extern IntPtr GetDC(IntPtr hWnd);

                    [DllImport("user32.dll", ExactSpelling = true)]
                    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

                    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
                    public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

                    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
                    public static extern Bool DeleteDC(IntPtr hdc);

                    [DllImport("gdi32.dll", ExactSpelling = true)]
                    public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

                    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
                    public static extern Bool DeleteObject(IntPtr hObject);
                }
            }
        }
    }
}


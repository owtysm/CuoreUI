﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CuoreUI.Components.Forms;
using Color = System.Drawing.Color;
using Task = System.Threading.Tasks.Task;

namespace CuoreUI.Components
{
    [Description("Smooth rounded corners for your form with a bitmap approach")]
    public partial class cuiFormRounder : Component
    {
        public cuiFormRounder()
        {
        }

        public RoundedForm roundedFormObj;

        private Form privateTargetForm;

        [Category("CuoreUI")]
        public Form TargetForm
        {
            get => privateTargetForm;
            set
            {
                if (value == null && privateTargetForm != null && !privateTargetForm.Disposing)
                {
                    privateTargetForm.Region = null;
                }

                if (privateTargetForm != null)
                {
                    FormsRegisteredByRounder.RemoveByForm(privateTargetForm);
                }

                privateTargetForm = value;

                if (value == null)
                {
                    roundedFormObj?.UpdBitmap();
                    roundedFormObj?.Hide();
                    return;
                }

                if (FormsRegisteredByRounder.AddByForm(value, this) == false)
                {
                    throw new Exception("This form already has a cuiformRounder");
                }

                TargetForm.Load += TargetForm_Load;
                TargetForm.Resize += TargetForm_Resize;
                TargetForm.LocationChanged += TargetForm_LocationChanged;
                TargetForm.FormClosing += TargetForm_FormClosing;
                TargetForm.VisibleChanged += TargetForm_VisibleChanged;
                TargetForm.BackColorChanged += TargetForm_BackColorChanged;
                TargetForm.Activated += TargetForm_Activated;
                TargetForm.HandleCreated += TargetForm_HandleCreated;
                TargetForm.ResizeEnd += (e, s) =>
                {
                    UpdateExperimentalBitmap();
                };

                if (roundedFormObj != null && roundedFormObj.IsDisposed == false && !DesignMode) // if for some reason you want to toggle between a form and 'null'
                {
                    UpdateRoundedFormRegion();

                    TargetForm_LocationChanged(this, EventArgs.Empty);
                    TargetForm_Resize(this, EventArgs.Empty);

                    roundedFormObj?.Show();
                }
            }
        }

        private void TargetForm_HandleCreated(object sender, EventArgs e)
        {
            FormsRegisteredByRounder.AddByForm(TargetForm, this);

            if (!DesignMode && TargetForm != null)
            {
                int disable = 1;
                DwmSetWindowAttribute(TargetForm.Handle, -3, ref disable, sizeof(int));
            }
        }

        bool targetFormActivating = false;

        private void TargetForm_Activated(object sender, EventArgs e)
        {
            if (shouldCloseDown)
            {
                return;
            }

            if (!targetFormActivating)
            {
                targetFormActivating = true;
                FakeForm_Activated(sender, e);
            }
        }

        private void TargetForm_BackColorChanged(object sender, EventArgs e)
        {
            if (shouldCloseDown)
            {
                return;
            }
        }

        private void TargetForm_VisibleChanged(object sender, EventArgs e)
        {
            if (shouldCloseDown)
            {
                return;
            }

            if (!DesignMode && roundedFormObj != null && !wasFormClosingCalled && roundedFormObj.IsDisposed == false)
            {
                roundedFormObj.Visible = TargetForm.Visible;
                roundedFormObj.Tag = TargetForm.Opacity;
                roundedFormObj.InvalidateNextDrawCall = true;
            }
        }

        private bool shouldCloseDown = false;
        private bool wasFormClosingCalled = false;

        private void TargetForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // causes the other parts of this code to not run
            // that stops any exceptions in the code trying to access disposed or null stuff 
            shouldCloseDown = true;
            if (!wasFormClosingCalled && TargetForm != null)
            {
                wasFormClosingCalled = true;

                // unattach events
                TargetForm.Load -= TargetForm_Load;
                TargetForm.Resize -= TargetForm_Resize;
                TargetForm.LocationChanged -= TargetForm_LocationChanged;
                TargetForm.FormClosing -= TargetForm_FormClosing;
                TargetForm.VisibleChanged -= TargetForm_VisibleChanged;
                TargetForm.BackColorChanged -= TargetForm_BackColorChanged;

                // clean controls from targetform
                TargetForm.Controls.Clear();

                // send close message to all forms

                TryCloseForm(TargetForm);
                TryCloseForm(roundedFormObj);
            }
        }

        void TryCloseForm(Form f)
        {
            if (f != null && !f.IsDisposed && f.IsHandleCreated)
            {
                Helper.Win32.SendMessage(f.Handle, 0x0010, IntPtr.Zero, IntPtr.Zero);
            }
        }

        public void FakeForm_Activated(object sender, EventArgs e)
        {
            if (DesignMode || shouldCloseDown || wasFormClosingCalled || DesignMode || TargetForm == null || TargetForm.IsDisposed)
            {
                return;
            }

            if (roundedFormObj != null && !roundedFormObj.IsDisposed)
            {
                roundedFormObj.Tag = TargetForm.Opacity;
                roundedFormObj.InvalidateNextDrawCall = true;

                // https://github.com/1Kxhu/CuoreUI/issues/11 fix #3
                if (roundedFormObj.WindowState != FormWindowState.Minimized)
                {
                    if (!wasFormClosingCalled && !shouldCloseDown)
                    {
                        if (TargetForm.WindowState != FormWindowState.Minimized)
                        {
                            SetWindowPos(roundedFormObj.Handle, HWND_TOP, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
                            SetWindowPos(TargetForm.Handle, HWND_TOP, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
                        }

                        TargetForm.BringToFront();
                    }
                }
            }

            targetFormActivating = false;
        }

        private static Point PointSubtract(Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }

        private int privateRounding = 8;

        [Category("CuoreUI")]
        public int Rounding
        {
            get => privateRounding;
            set
            {
                privateRounding = value;
                if (shouldCloseDown)
                {
                    return;
                }
                if (roundedFormObj != null && roundedFormObj.IsDisposed == false)
                {
                    roundedFormObj.Rounding = value;
                    if (TargetForm != null)
                    {
                        UpdateTargetFormRegion();
                        UpdateRoundedFormRegion();
                        roundedFormObj?.Invalidate();
                    }
                }
            }
        }

        private async void TargetForm_LocationChanged(object sender, EventArgs e)
        {
            // inlined not to repeat ourselves later
            bool IsSafeToEditRoundedForm() =>
                !shouldCloseDown &&
                !DesignMode &&
                roundedFormObj != null &&
                TargetForm != null &&
                !roundedFormObj.IsDisposed && roundedFormObj.initialized;

            if (IsSafeToEditRoundedForm())
            {
                roundedFormObj.Location = PointSubtract(TargetForm.Location, new Point(1, 1));
                UpdateRoundedFormRegion();

                // https://github.com/1Kxhu/CuoreUI/issues/11 fix #2
                if (TargetForm.WindowState == FormWindowState.Minimized)
                {
                    roundedFormObj?.Hide();
                }
                else
                {
                    await Task.Delay(1000 / Drawing.GetHighestRefreshRate());

                    // check same flags again because some time had passed
                    if (IsSafeToEditRoundedForm())
                    {
                        roundedFormObj?.Show();
                    }
                }
            }
        }

        private Color privateOutlineColor = Color.FromArgb(32, 128, 128, 128);

        [Category("CuoreUI")]
        public Color OutlineColor
        {
            get => privateOutlineColor;
            set
            {
                privateOutlineColor = value;
                if (shouldCloseDown)
                {
                    return;
                }

                if (roundedFormObj != null && roundedFormObj.IsDisposed == false)
                {
                    roundedFormObj?.Invalidate();
                }
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private static readonly IntPtr HWND_TOP = new IntPtr(0);

        void UpdateTargetFormRegion()
        {
            TargetForm.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, TargetForm.Width, TargetForm.Height, (int)(Rounding * 2f), (int)(Rounding * 2f)));
        }

        public void UpdateRoundedFormRegion()
        {
            if (!DesignMode || TargetForm?.Opacity != 1)
            {
                if (roundedFormObj != null && roundedFormObj.IsDisposed == false)
                {
                    // for opacity support
                    Region region = new Region(roundedFormObj.ClientRectangle);
                    Region offsetRegion = TargetForm.Region?.Clone();

                    if (offsetRegion != null)
                    {
                        offsetRegion.Translate(1, 1);
                        region.Exclude(offsetRegion);
                    }

                    roundedFormObj.Region = region;
                }
            }
            else
            {
                roundedFormObj.Region = new Region(roundedFormObj.ClientRectangle);
            }
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        private void TargetForm_Load(object sender, EventArgs e)
        {
            // initialize rounding
            FakeForm_Activated(sender, e);

            TargetForm.FormBorderStyle = FormBorderStyle.None;

            roundedFormObj = new RoundedForm(TargetForm.BackColor, OutlineColor, ref privateRounding);
            roundedFormObj.TargetForm = TargetForm;
            roundedFormObj.Tag = TargetForm.Opacity;

            TargetForm.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, TargetForm.Width, TargetForm.Height, (int)(Rounding * 2f), (int)(Rounding * 2f))); ;

            roundedFormObj?.Show();

            //FakeForm.Show();

            roundedFormObj.Activated += FakeForm_Activated;
            TargetForm_Resize(this, EventArgs.Empty);
            TargetForm_LocationChanged(this, EventArgs.Empty);

            TargetForm.FormBorderStyle = FormBorderStyle.None;

            // fix for the program not appearing in the taskbar until you'd interacted with the window
            if (TargetForm.ShowInTaskbar)
            {
                TargetForm.BringToFront();
                TargetForm.Activate();
                TargetForm.Focus();
                TargetForm.Location = TargetForm.Location;

                if (!DesignMode && TargetForm != null)
                {
                    int enable = 0;
                    DwmSetWindowAttribute(TargetForm.Handle, -3, ref enable, sizeof(int));
                }
            }

            // Drawing.TenFramesDrawn is called every 10000/hz milliseconds
            // where hz stands for the maximum refresh rate recorded from all display devices
            Drawing.TenFramesDrawn += (_, __) =>
            {
                if (roundedFormObj != null && shouldCloseDown == false && roundedFormObj.IsDisposed == false)
                {
                    try // https://github.com/1Kxhu/CuoreUI/issues/11 fix #1
                    {
                        // this is the part that MAY raise an exception from ComboBoxDropDown
                        roundedFormObj.Tag = TargetForm.Opacity;
                        roundedFormObj.InvalidateNextDrawCall = true;
                    }
                    catch
                    {
                        // ComboBoxDropDown raises an exception here
                        // but we can just not care about this, since it's opacity is ALWAYS 100%
                    }

                    // TopMost smooth corners n border disappear fix
                    if (TargetForm != null && TargetForm.IsDisposed == false && TargetForm.TopMost != roundedFormObj.TopMost)
                    {
                        roundedFormObj.TopMost = TargetForm.TopMost;
                    }
                }
                else if (roundedFormObj.initialized)
                {
                    // either roundedFormObj is null or "stop" is true
                    // stop is true when the form had announced it wants to close (see TargetForm_FormClosing)
                    Dispose();
                }
            };

        }

        Bitmap experimentalBitmap
        {
            get; set;
        }

        // truly the smooth corner experience (tears of joy as of writing this)
        private void UpdateExperimentalBitmap()
        {
            if (DesignMode || TargetForm == null || TargetForm.IsDisposed || shouldCloseDown || roundedFormObj != null || roundedFormObj.IsDisposed || !roundedFormObj.initialized)
            {
                return;
            }

            if (Rounding == 0)
            {
                experimentalBitmap?.Dispose();
                roundedFormObj?.UpdBitmap();
                return;
            }

            int targetWidth = Math.Max(TargetForm.Width + 1, 1);
            int targetHeight = Math.Max(TargetForm.Height + 1, 1);

            var newBitmap = new Bitmap(targetWidth, targetHeight);

            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                g.Clear(Color.Transparent);
                TargetForm?.DrawToBitmap(newBitmap, new Rectangle(0, 0, TargetForm.Width, TargetForm.Height));
            }

            experimentalBitmap?.Dispose();
            experimentalBitmap = newBitmap;
            //experimentalBitmap.Save(@"C:\Desktop\cuoreh.png");
            roundedFormObj?.UpdBitmap(newBitmap);

        }

        FormWindowState lastState;

        private void TargetForm_Resize(object sender, EventArgs e)
        {
            if (DesignMode || TargetForm == null || TargetForm.IsDisposed || shouldCloseDown || roundedFormObj == null || roundedFormObj.IsDisposed || !roundedFormObj.initialized)
            {
                return;
            }

            // If windowstate has changed, set said windowstate value to all the other forms
            if (TargetForm.WindowState != lastState)
            {
                lastState = TargetForm.WindowState;
                roundedFormObj.WindowState = TargetForm.WindowState;
            }

            if (TargetForm.WindowState != FormWindowState.Minimized)
            {
                // Related to how RoundedForm is drawn
                // Updates rounding if needed, too
                roundedFormObj.Size = System.Drawing.Size.Add(TargetForm.Size, new Size(2, 2));
                UpdateRoundedFormRegion();
                UpdateExperimentalBitmap();

                roundedFormObj.InvalidateNextDrawCall = true;

                if (TargetForm.WindowState == FormWindowState.Normal)
                {
                    TargetForm.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, TargetForm.Width, TargetForm.Height, (int)(Rounding * 2f), (int)(Rounding * 2f)));
                }
                else
                {
                    TargetForm.Region = null;
                }
            }

            targetFormActivating = false;
        }
    }
}

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// https://stackoverflow.com/a/56768588
public static class HandCursorFix
{
    static HandCursorFix()
    {
        EnableModernCursor();
    }

    public static void EnableModernCursor()
    {
        try
        {
            if (!IsInDesignMode())
            {
                Cursor SystemHandCursor = new Cursor(LoadCursor(IntPtr.Zero, 32649 /*IDC_HAND*/));
                typeof(Cursors).GetField("hand", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, SystemHandCursor);
            }
        }
        catch { }
    }

    private static bool IsInDesignMode()
    {
        // otherwise we'd get a serialization error in the designer at random times
        string processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName.ToLower().Trim();
        return processName.Contains("devenv") || processName.Contains("designtoolsserver");
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
}

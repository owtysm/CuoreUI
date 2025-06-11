using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public static class GlobalMouseHook
{
    private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;

    private static HookProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    public static bool isHooked = false;

    public static void Start()
    {
        _hookID = SetHook(_proc);
        isHooked = true;
    }

    public static void Stop()
    {
        UnhookWindowsHookEx(_hookID);
        isHooked = false;
        GC.Collect();
    }

    private static IntPtr SetHook(HookProc proc)
    {
        using (var curProcess = Process.GetCurrentProcess())
        using (var curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_LBUTTONDOWN)
        {
            OnGlobalMouseClick();
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    public static Action OnGlobalMouseClick;
}
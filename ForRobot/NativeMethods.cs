using System;
using System.Runtime.InteropServices;

namespace ForRobot
{
    internal enum ShowWindowCommand : int
    {
        Hide = 0x0,
        ShowNormal = 0x1,
        ShowMinimized = 0x2,
        ShowMaximized = 0x3,
        ShowNormalNotActive = 0x4,
        Minimize = 0x6,
        ShowMinimizedNotActive = 0x7,
        ShowCurrentNotActive = 0x8,
        Restore = 0x9,
        ShowDefault = 0xA,
        ForceMinimize = 0xB
    }

    internal class NativeMethods
    {
        public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);

        [DllImport("User32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern Boolean SetForegroundWindow([In] IntPtr windowHandle);

        [DllImport("User32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern Boolean ShowWindow([In] IntPtr windowHandle, [In] ShowWindowCommand command);

    }
}

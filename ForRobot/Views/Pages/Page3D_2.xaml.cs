using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ForRobot.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для Page3D_2.xaml
    /// </summary>
    public partial class Page3D_2 : Page, IDisposable
    {
        //[DllImport("user32.dll")]
        //private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        //[DllImport("user32.dll", SetLastError = true)]
        //private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        //[DllImport("user32")]
        //private static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);

        //[DllImport("user32")]
        //private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        //private const int SWP_NOZORDER = 0x0004;
        //private const int SWP_NOACTIVATE = 0x0010;
        //private const int GWL_STYLE = -16;
        //private const int WS_CAPTION = 0x00C00000;
        //private const int WS_VISIBLE = 0x10000000;
        //private const int WS_THICKFRAME = 0x00040000;

        private Process _process;
        private volatile bool _disposed = false;
        //private const string DefaultExe = "mspaint.exe";
        private const string DefaultExe = @"D:\ForRobot\py_gen\test_qt.py";

        public Page3D_2()
        {
            InitializeComponent();

            this.Loaded += (s, o) => LaunchChildProcess();
            this.Unloaded += (s, o) =>
            {
                if (_process != null)
                {
                    ////this._process.Refresh();
                    ////this._process.Close();
                    //this._process.Kill();

                    //this._process.CloseMainWindow();
                    //this._process.WaitForExit(5000);

                    //if (_process?.HasExited == false)
                    //{
                    //    _process.Kill();
                    //}

                    _process.Close();
                    _process.Dispose();
                    _process = null;

                    var prop = Process.GetProcesses().Where(item => item.ProcessName.Contains("python")).First();
                    if (prop.HasExited == false)
                    {
                        prop.Kill();
                    }
                }
            };
        }

        private void LaunchChildProcess(string sExeName = DefaultExe)
        {
            //this._process = new Process()
            //{
            //    StartInfo = new ProcessStartInfo()
            //    {
            //        UseShellExecute = false,
            //        RedirectStandardInput = false,
            //        RedirectStandardOutput = false,
            //        RedirectStandardError = true,
            //        CreateNoWindow = false,
            //        FileName = "python.exe",
            //        WorkingDirectory = System.IO.Path.GetDirectoryName(sExeName),
            //        WindowStyle = ProcessWindowStyle.Minimized,
            //        Arguments = sExeName
            //    }
            //};
            //this._process.Start();
            //this._process.WaitForInputIdle();
            ////this._process.WaitForExit();
            //System.Threading.Thread.Sleep(1000);


            //var hostedChild = new HwndHostEx(this._process.MainWindowHandle);
            //var win = Window.GetWindow(this);

            //var helper = new WindowInteropHelper(Window.GetWindow(this));
            //SetParent(this._process.MainWindowHandle, helper.Handle);

            //int style = GetWindowLong(this._process.MainWindowHandle, GWL_STYLE);
            //style = style & ~WS_CAPTION & ~WS_THICKFRAME;
            //SetWindowLong(this._process.MainWindowHandle, GWL_STYLE, style);
            //ResizeEmbeddedApp();

            //var helper = new WindowInteropHelper(Window.GetWindow(this.windowsFormsHost1));
            //SetParent(this._process.MainWindowHandle, helper.Handle);
            //int style = GetWindowLong(this._process.MainWindowHandle, GWL_STYLE);
            //style = style & ~WS_CAPTION & ~WS_THICKFRAME;
            //SetWindowLong(this._process.MainWindowHandle, GWL_STYLE, style);
            //ResizeEmbeddedApp();

            using (this._process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false,
                    FileName = "python.exe",
                    WorkingDirectory = System.IO.Path.GetDirectoryName(sExeName),
                    WindowStyle = ProcessWindowStyle.Minimized,
                    Arguments = $"-u {sExeName}"
                }
            })
            {
                this._process.OutputDataReceived += (s, e) =>
                {
                    //if (e.Data?.Length > 0)
                    //{
                    //    MessageBox.Show("Из дочернего интерфейса", e.Data);
                    //}

                    if (e.Data?.Length > 0 && e.Data == "<done>") // Из оболочки для модели
                    {
                        MessageBox.Show("Из дочернего интерфейса", e.Data);
                    }
                };
                this._process.ErrorDataReceived += (s, e) =>
                {
                    System.Threading.Thread.Sleep(1000);
                };
                this._process.Start();
                //this._process.WaitForInputIdle(int.MaxValue);
                //this._process.WaitForExit();
                //this._process.

                System.Threading.Thread.Sleep(1000);
                this._process.BeginOutputReadLine();
                var handle = this._process?.MainWindowHandle;
                if (handle != null)
                {
                    var host = new HwndHostEx(handle.Value);
                    host.Height = this.windowsFormsHost1.ActualHeight;
                    this.windowsFormsHost1.Children.Add(host);
                }

                //using (var sw = this._process.StandardOutput)
                //{
                //    if (sw.BaseStream.CanRead)
                //    {

                //    }
                //}
            }

            //var handle = this._process?.MainWindowHandle;
            //if (handle != null)
            //{
            //    var host = new HwndHostEx(handle.Value);
            //    host.Height = this.windowsFormsHost1.ActualHeight;
            //    this.windowsFormsHost1.Children.Add(host);
            //}
        }

        //private void ResizeEmbeddedApp()
        //{
        //    if (this._process == null)
        //        return;

        //    //UIElement container = VisualTreeHelper.GetParent(this.windowsFormsHost1) as UIElement;
        //    //Point relativeLocation = this.windowsFormsHost1.TranslatePoint(new Point(0, 0), container);

        //    //SetWindowPos(_process.MainWindowHandle, IntPtr.Zero, 0, 0, (int)windowsFormsHost1.Width, (int)windowsFormsHost1.Height, SWP_NOZORDER | SWP_NOACTIVATE);

        //    //UIElement container = VisualTreeHelper.GetParent(this.windowsFormsHost1) as UIElement;
        //    //Point relativeLocation = this.windowsFormsHost1.TranslatePoint(new Point(0, 0), container);

        //    //SetWindowPos(_process.MainWindowHandle, IntPtr.Zero, 0, 0, (int)this.windowsFormsHost1.Width, (int)this.windowsFormsHost1.Height, SWP_NOZORDER | SWP_NOACTIVATE);
        //}

        protected override Size MeasureOverride(Size availableSize)
        {
            Size size = base.MeasureOverride(availableSize);
            //ResizeEmbeddedApp();
            return size;
        }

        #region Implementations of IDisposable

        ~Page3D_2() => Dispose(false);

        public void Dispose() => this.Dispose(true);

        public void Dispose(bool disposing)
        {
            if (this._disposed)
                return;

            if (disposing)
            {
                if (_process != null)
                {
                    _process.Refresh();
                    _process.Close();
                }
            }
            this._disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    public class HwndHostEx : HwndHost
    {
        private readonly IntPtr _childHandle;

        public HwndHostEx(IntPtr handle)
        {
            _childHandle = handle;
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            var childRef = new HandleRef();
            if (_childHandle != IntPtr.Zero)
            {
                var childStyle = (IntPtr)(Win32API.WindowStyles.WS_CHILD |
                                          // Child window should be have a thin-line border
                                          Win32API.WindowStyles.WS_BORDER |
                                          // the parent cannot draw over the child's area. this is needed to avoid refresh issues
                                          Win32API.WindowStyles.WS_CLIPCHILDREN |
                                          Win32API.WindowStyles.WS_VISIBLE |
                                          Win32API.WindowStyles.WS_MAXIMIZE |
                                          Win32API.WindowStyles.WS_OVERLAPPED);

                childRef = new HandleRef(this, _childHandle);
                Win32API.SetWindowLongPtr(childRef, Win32API.GWL_STYLE, childStyle);
                Win32API.SetParent(_childHandle, hwndParent.Handle);
            }
            return childRef;
        }

        protected override void DestroyWindowCore(HandleRef hwnd) { }
    }

    public class Win32API
    {
        [DllImport("User32.dll")]
        internal static extern IntPtr SetParent(IntPtr hwc, IntPtr hwp);

        /// <summary>
        /// This static method is required because legacy OSes do not support SetWindowLongPtr
        /// </summary>
        internal static IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        /// <summary>
        /// This static method is required because Win32 does not support GetWindowLongPtr directly
        /// </summary>
        internal static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return GetWindowLongPtr32(hWnd, nIndex);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool BringWindowToTop(HandleRef hWnd);

        internal const int GWL_STYLE = -16;
        internal const int GWL_EXSTYLE = -20;
        internal const int WS_CAPTION = 0x00C00000;
        internal const int WS_THICKFRAME = 0x00040000;

        [Flags]
        internal enum WindowStyles : uint
        {
            WS_OVERLAPPED = 0x00000000,
            WS_POPUP = 0x80000000,
            WS_CHILD = 0x40000000,
            WS_MINIMIZE = 0x20000000,
            WS_VISIBLE = 0x10000000,
            WS_DISABLED = 0x08000000,
            WS_CLIPSIBLINGS = 0x04000000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_MAXIMIZE = 0x01000000,
            WS_BORDER = 0x00800000,
            WS_DLGFRAME = 0x00400000,
            WS_VSCROLL = 0x00200000,
            WS_HSCROLL = 0x00100000,
            WS_SYSMENU = 0x00080000,
            WS_THICKFRAME = 0x00040000,
            WS_GROUP = 0x00020000,
            WS_TABSTOP = 0x00010000,

            WS_MINIMIZEBOX = 0x00020000,
            WS_MAXIMIZEBOX = 0x00010000,

            WS_CAPTION = WS_BORDER | WS_DLGFRAME,
            WS_TILED = WS_OVERLAPPED,
            WS_ICONIC = WS_MINIMIZE,
            WS_SIZEBOX = WS_THICKFRAME,
            WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,

            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_CHILDWINDOW = WS_CHILD,

            //Extended Window Styles

            WS_EX_DLGMODALFRAME = 0x00000001,
            WS_EX_NOPARENTNOTIFY = 0x00000004,
            WS_EX_TOPMOST = 0x00000008,
            WS_EX_ACCEPTFILES = 0x00000010,
            WS_EX_TRANSPARENT = 0x00000020,

            //#if(WINVER >= 0x0400)

            WS_EX_MDICHILD = 0x00000040,
            WS_EX_TOOLWINDOW = 0x00000080,
            WS_EX_WINDOWEDGE = 0x00000100,
            WS_EX_CLIENTEDGE = 0x00000200,
            WS_EX_CONTEXTHELP = 0x00000400,

            WS_EX_RIGHT = 0x00001000,
            WS_EX_LEFT = 0x00000000,
            WS_EX_RTLREADING = 0x00002000,
            WS_EX_LTRREADING = 0x00000000,
            WS_EX_LEFTSCROLLBAR = 0x00004000,
            WS_EX_RIGHTSCROLLBAR = 0x00000000,

            WS_EX_CONTROLPARENT = 0x00010000,
            WS_EX_STATICEDGE = 0x00020000,
            WS_EX_APPWINDOW = 0x00040000,

            WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE),
            WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST),
            //#endif /* WINVER >= 0x0400 */

            //#if(WIN32WINNT >= 0x0500)

            WS_EX_LAYERED = 0x00080000,
            //#endif /* WIN32WINNT >= 0x0500 */

            //#if(WINVER >= 0x0500)

            WS_EX_NOINHERITLAYOUT = 0x00100000, // Disable inheritence of mirroring by children
            WS_EX_LAYOUTRTL = 0x00400000, // Right to left mirroring
                                          //#endif /* WINVER >= 0x0500 */

            //#if(WIN32WINNT >= 0x0500)

            WS_EX_COMPOSITED = 0x02000000,
            WS_EX_NOACTIVATE = 0x08000000
            //#endif /* WIN32WINNT >= 0x0500 */
        }
    }
}

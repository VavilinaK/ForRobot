using System;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Runtime.InteropServices;

namespace ForRobot.Libr
{
    public class SingleGlobalInstance : IDisposable
    {
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr handle);

        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        [DllImport("User32.dll")]
        private static extern bool IsIconic(IntPtr handle);

        private const int SW_RESTORE = 9;

        private const int TIME_OUT = 1000;

        public static bool _hasHandle = false;

        private static string _appGuid;

        private static Mutex _mutex;

        public SingleGlobalInstance(int timeOut = TIME_OUT)
        {
            string appGuid = GetAssemblyGuid(Assembly.GetExecutingAssembly());
            if (IsAlreadyRunning(timeOut))
            {

            }
            //InitMutex();
            //try
            //{
            //    if (timeOut < 0)
            //        _hasHandle = _mutex.WaitOne(Timeout.Infinite, false);
            //    else
            //        _hasHandle = _mutex.WaitOne(timeOut, false);

            //    if (_hasHandle == false)
            //        throw new TimeoutException("Timeout waiting for exclusive access on SingleInstance");
            //}
            //catch (AbandonedMutexException)
            //{
            //    _hasHandle = true;
            //}
        }

        /// <summary>
        /// Инициализация _mutex.
        /// </summary>
        private void InitMutex()
        {
            //string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value;
            //string mutexId = string.Format("Global\\{{{0}}}", appGuid);
            //_mutex = new Mutex(false, mutexId);

            //var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            //var securitySettings = new MutexSecurity();
            //securitySettings.AddAccessRule(allowEveryoneRule);
            //_mutex.SetAccessControl(securitySettings);
        }

        private static string GetAssemblyGuid(Assembly assembly)
        {
            object[] customAttribs = assembly.GetCustomAttributes(typeof(GuidAttribute), false);
            if (customAttribs.Length < 1)
                return null;                
            return ((GuidAttribute)(customAttribs.GetValue(0))).Value.ToString();
        }

        private static void BringProcessToFront(Process process)
        {
            IntPtr handle = process.MainWindowHandle;
            if (IsIconic(handle))
                ShowWindow(handle, SW_RESTORE);
            SetForegroundWindow(handle);
        }

        public static bool IsAlreadyRunning(int timeOut, bool useGlobal = false)
        {
            string mutexId;
            if (useGlobal)
            {
                mutexId = String.Format("Global\\{{{0}}}", _appGuid);
            }
            else
            {
                mutexId = String.Format("{{{0}}}", _appGuid);
            }

            MutexAccessRule allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            MutexSecurity securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            _mutex = new Mutex(false, mutexId, out bool createdNew, securitySettings);
            try
            {
                if (timeOut < 0)
                    _hasHandle = _mutex.WaitOne(Timeout.Infinite, false);
                else
                    _hasHandle = _mutex.WaitOne(0, false);

                //if (_hasHandle == false)
                //    throw new TimeoutException("Timeout waiting for exclusive access on SingleInstance");

                if (!_hasHandle)
                    return true;
            }
            catch (AbandonedMutexException)
            {
                _hasHandle = true;
            }
            return false;
        }

        public static void ShowRunningApp()
        {
            Process current = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcesses())
            {
                if (process.Id == current.Id)
                {
                    continue;
                }

                try
                {
                    Assembly assembly = Assembly.LoadFrom(process.MainModule.FileName);

                    string processGuid = GetAssemblyGuid(assembly);
                    if (_appGuid.Equals(processGuid))
                    {
                        BringProcessToFront(process);
                        return;
                    }
                }
                catch { }
            }
        }

        public void Dispose()
        {
            if (_mutex != null)
            {
                if (_hasHandle)
                    _mutex.ReleaseMutex();
                _mutex.Close();
            }
        }
    }
}

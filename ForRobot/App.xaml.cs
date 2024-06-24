using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows;
using System.Data.SqlClient;
using System.Configuration;

using ClassLibraryTaskManager;

using ForRobot.Libr;

using NLog;

namespace ForRobot
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Private variables

        private string _log;

        private static Mutex mutex = null;

        private static SingleGlobalInstance singleMutex;

        #region Widows

        private Views.Windows.MainWindow _mainWindow;

        #endregion

        #endregion

        #region Event

        public event EventHandler<LogEventArgs> Log;

        #endregion

        #region Public variables

        public string LoggerString
        {
            get => this._log;
            set
            {
                this._log = (value + "\n");
                this.Log(this, null);
            }
        }

        public static new App Current => Application.Current as App;

        public NLog.Logger Logger { get; set; } = NLog.LogManager.GetCurrentClassLogger();

        #region Windows

        public Views.Windows.MainWindow MainWindowView { get => _mainWindow ?? (_mainWindow = new Views.Windows.MainWindow()); }

        #endregion

        #endregion

        #region Private functions

        /// <summary>
        /// Запуск программы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [STAThread]
        private void OnStartUp(object sender, StartupEventArgs e)
        {
            try
            {
                using (mutex = new System.Threading.Mutex(true, ResourceAssembly.GetName().Name, out bool newMutex))
                {
                    if (!newMutex)
                    {
                        NativeMethods.PostMessage((IntPtr)NativeMethods.HWND_BROADCAST,
                            NativeMethods.WM_SHOWME,
                            IntPtr.Zero,
                            IntPtr.Zero);
                        Environment.Exit((int)Error.ExitCode.ERROR_SUCCESS);
                    }

                    if (!Manager.ProcessInf(ResourceAssembly.GetName().Name))
                    {
                        using (singleMutex = new SingleGlobalInstance(1000))
                        {
                            this.Logger.Info($"{DateTime.Now.ToString("HH:mm:ss")} Запуск приложения");
                            Application.Current.MainWindow = MainWindowView;
                            MainWindowView.Show();
                            GC.KeepAlive(mutex);
                        }
                    }
                    else
                    {
                        NativeMethods.PostMessage((IntPtr)NativeMethods.HWND_BROADCAST,
                            NativeMethods.WM_SHOWME,
                            IntPtr.Zero,
                            IntPtr.Zero);
                        Environment.Exit((int)Error.ExitCode.ERROR_SUCCESS);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex.Message);
            }
        }

        /// <summary>
        /// Детектит исключения в течении работы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message + "\t||\t" + e.Exception.Source, "", MessageBoxButton.OK);
            Logger.Fatal(e.Exception.Message + "\t||\t" + e.Exception.Source);
        }

        private void onExit(object sender, ExitEventArgs e)
        {
            if (((Application.Current.Windows.Count == 0) && (Application.Current.ShutdownMode == ShutdownMode.OnLastWindowClose))
                || (Application.Current.ShutdownMode == ShutdownMode.OnMainWindowClose))
            {
                this.Logger.Trace($"{DateTime.Now.ToString("HH:mm:ss")} Закрытие приложения");
                Application.Current.Shutdown(0);
            }
        }

        #endregion

        #region Public functions

        #endregion
    }
}

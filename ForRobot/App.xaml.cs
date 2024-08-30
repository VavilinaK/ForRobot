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

using Newtonsoft.Json;

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

        //private static SingleGlobalInstance singleMutex;

        /// <summary>
        /// Путь к программе на сервере
        /// </summary>
        private string UpdatePath { get => ForRobot.Properties.Settings.Default.UpdatePath; }

        /// <summary>
        /// Путь к программе на коммпьютере
        /// </summary>
        private string FilePathOnPC { get => Directory.GetCurrentDirectory(); }

        /// <summary>
        /// Настройки приложения
        /// </summary>
        private ForRobot.Libr.Settings.Settings Settings { get; } = ForRobot.Libr.Settings.Settings.GetSettings();

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
                //this._log = (value + "\n");
                this._log = value;
                this.Log(this, null);
            }
        }

        public static new App Current => Application.Current as App;

        public NLog.Logger Logger { get; set; } = NLog.LogManager.GetCurrentClassLogger();

        #region Windows

        public Views.Windows.MainWindow MainWindowView { get => _mainWindow ?? (_mainWindow = new Views.Windows.MainWindow()); }

        public Views.Windows.PropertiesWindow PropertiesWindow { get; set; }

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
                        Environment.Exit(0);
                    }

                    if (!(Process.GetProcessesByName(ResourceAssembly.GetName().Name).Length > 1)) // Проверка на существование более 1 процесса.
                    {
                        // Выгрузка настроек из Temp


                        //if (ForRobot.Properties.Settings.Default.AutoUpdate && File.Exists(Path.Combine(App.Current.UpdatePath, $"{ResourceAssembly.GetName().Name}.exe")) &&
                        //    new Version(FileVersionInfo.GetVersionInfo(Path.Combine(App.Current.UpdatePath, $"{ResourceAssembly.GetName().Name}.exe")).ProductVersion) > Assembly.GetExecutingAssembly().GetName().Version &&
                        //    (!ForRobot.Properties.Settings.Default.InformUser || MessageBox.Show($"Обнаружено обновление до версии {FileVersionInfo.GetVersionInfo(Path.Combine(App.Current.UpdatePath, $"{ResourceAssembly.GetName().Name}.exe")).ProductVersion}\nОбновить приложение?", "Обновление интерфейса", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK))
                        //{
                        //    App.Current.UpDateApp(e);
                        //}

                        using (ClassLibraryTaskManager.SingleGlobalInstance singleMutex = new ClassLibraryTaskManager.SingleGlobalInstance(1000))
                        {
                            this.Logger.Info("Запуск приложения\n");
                            Application.Current.MainWindow = MainWindowView;
                            MainWindowView.Show();
                            GC.KeepAlive(mutex);
                        }
                    }
                    else
                    {
                        this.Logger.Info("Приложение уже открыто!\n");
                        NativeMethods.PostMessage((IntPtr)NativeMethods.HWND_BROADCAST,
                            NativeMethods.WM_SHOWME,
                            IntPtr.Zero,
                            IntPtr.Zero);
                        Environment.Exit(0);
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex.Message);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Детектит исключения в течении работы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message + "\t||\t" + e.Exception.Source, "", MessageBoxButton.OK);
            Logger.Fatal(e.Exception.Message + "\t||\t" + e.Exception.Source);
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            if (((Application.Current.Windows.Count == 0) && (Application.Current.ShutdownMode == ShutdownMode.OnLastWindowClose))
                || (Application.Current.ShutdownMode == ShutdownMode.OnMainWindowClose))
            {
                this.Logger.Info("Закрытие приложения\n");
                Application.Current.Shutdown(0);
            }
        }

        /// <summary>
        /// Обновление программы.
        /// Копирует файлы из каталога на сервере в нынешнюю директорию программы и перезапускает её
        /// </summary>
        /// <param name="e"></param>
        private void UpDateApp(StartupEventArgs e)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = this.FilePathOnPC,
                    CreateNoWindow = true,
                    FileName = "cmd.exe",
                    Arguments = $"/K taskkill /im {ResourceAssembly.GetName().Name}.exe /f& xcopy \"{this.UpdatePath + "\\*.*"}\" \"{this.FilePathOnPC}\" /E /Y& START \"\" \"{this.FilePathOnPC + "\\" + ResourceAssembly.GetName().Name + ".exe"}\" \"{string.Join("\" \"", e.Args)}\"",
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                }
            };
            new Thread(() => process.Start()).Start();
        }

        #endregion

        #region Public functions

        #endregion
    }
}

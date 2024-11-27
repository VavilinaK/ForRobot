using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows;
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
        /// Экземпляр app из app.config
        /// </summary>
        private ForRobot.Libr.ConfigurationProperties.AppConfigurationSection AppConfig { get; set; } = (ConfigurationManager.GetSection("app") as ForRobot.Libr.ConfigurationProperties.AppConfigurationSection);

        #region Widows

        private Views.Windows.MainWindow _mainWindow;

        private Views.Windows.MainWindow2 _mainWindow2;

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
                this._log = value;
                this.Log(this, null);
            }
        }

        public static new App Current => Application.Current as App;

        public NLog.Logger Logger { get; set; } = NLog.LogManager.GetCurrentClassLogger();

        private static ForRobot.Libr.Settings.Settings _settings;
        /// <summary>
        /// Настройки приложения
        /// (Выгружаются из временных файлов, иначе инициализируются как класс)
        /// </summary>
        public static ForRobot.Libr.Settings.Settings Settings { get => _settings ?? (_settings = ForRobot.Libr.Settings.Settings.GetSettings()); }

        #region Windows

        /// <summary>
        /// Главное окно
        /// </summary>
        public Views.Windows.MainWindow MainWindowView { get => _mainWindow ?? (_mainWindow = new Views.Windows.MainWindow()); }

        /// <summary>
        /// Главное окно
        /// </summary>
        public Views.Windows.MainWindow2 MainWindowView2 { get => _mainWindow2 ?? (_mainWindow2 = new Views.Windows.MainWindow2()); }

        /// <summary>
        /// Окно настроек
        /// </summary>
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
        private void onStartUp(object sender, StartupEventArgs e)
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
                        // Проверка версии файла в папке с обновлением и запрос к пользователю.
                        if (Settings.AutoUpdate && File.Exists(Path.Combine(App.Current.UpdatePath, $"{ResourceAssembly.GetName().Name}.exe")) &&
                            new Version(FileVersionInfo.GetVersionInfo(Path.Combine(App.Current.UpdatePath, $"{ResourceAssembly.GetName().Name}.exe")).ProductVersion) > Assembly.GetExecutingAssembly().GetName().Version &&
                            (!Settings.InformUser || MessageBox.Show($"Обнаружено обновление до версии {FileVersionInfo.GetVersionInfo(Path.Combine(App.Current.UpdatePath, $"{ResourceAssembly.GetName().Name}.exe")).ProductVersion}\nОбновить приложение?", "Обновление интерфейса", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK))
                        {
                            this.Logger.Info($"Обновление приложения до версии {FileVersionInfo.GetVersionInfo(Path.Combine(App.Current.UpdatePath, $"{ResourceAssembly.GetName().Name}.exe")).ProductVersion}");
                            App.Current.UpDateApp(e);
                        }

                        // Обновление настроек приложения к пользовательским.
                        if (ForRobot.Properties.Settings.Default.IsSettingsUpgradeRequired)
                        {
                            ForRobot.Properties.Settings.Default.Upgrade();
                            ForRobot.Properties.Settings.Default.Reload();
                            ForRobot.Properties.Settings.Default.IsSettingsUpgradeRequired = false;
                            ForRobot.Properties.Settings.Default.Save();
                        }
                        else
                        {
                            foreach(var prop in typeof(ForRobot.Libr.ConfigurationProperties.AppConfigurationSection).GetProperties())
                            {
                                var v = typeof(ForRobot.Libr.ConfigurationProperties.AppConfigurationSection).GetProperty(prop.Name);
                                string scriptName = v.GetValue(AppConfig) as string;

                                if (Settings.AutoUpdate && (File.Exists(Path.Combine(this.FilePathOnPC, $"Scripts\\{scriptName}")) && File.Exists(Path.Combine(App.Current.UpdatePath, $"Scripts\\{scriptName}"))))
                                {
                                    Version oldVersion = Version.Parse(File.ReadLines(Path.Combine(this.FilePathOnPC, $"Scripts\\{scriptName}")).Where(str => str.Contains("__version__")).First().Split(new char[] { '=' }).Last().TrimStart().Trim(new char[] { '\'' }));
                                    Version newVersion = Version.Parse(File.ReadLines(Path.Combine(App.Current.UpdatePath, $"Scripts\\{scriptName}")).Where(str => str.Contains("__version__")).First().Split(new char[] { '=' }).Last().TrimStart().Trim(new char[] { '\'' }));

                                    if(newVersion > oldVersion && (!Settings.InformUser || 
                                        MessageBox.Show($"Обнаружено обновление скрипта {scriptName} до версии {newVersion}\nОбновить скрипт?", "Обновление скрипта-генерации", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK))
                                    {
                                        this.Logger.Info($"Обновление скрипта {scriptName} до версии {newVersion}");
                                        this.UpDateScript();
                                    }
                                }
                            }
                        }

                        using (ClassLibraryTaskManager.SingleGlobalInstance singleMutex = new ClassLibraryTaskManager.SingleGlobalInstance(1000))
                        {
                            this.Logger.Info("Запуск приложения\n");
                            Application.Current.MainWindow = MainWindowView;
                            MainWindowView.Show();

                            //Application.Current.MainWindow = MainWindowView2;
                            //MainWindowView2.Show();

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
                this.Logger.Info("Закрытие приложения\n");
                Application.Current.Shutdown(0);
            }
        }

        /// <summary>
        /// Обновление программы.
        /// Копирует файлы из каталога на сервере в нынешнюю директорию программы и перезапускает её
        /// </summary>
        /// <param name="e">Аргументы командной стоки</param>
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

        /// <summary>
        /// Обновление скрипта-генератора
        /// </summary>
        private void UpDateScript()
        {
            foreach (var file in Directory.GetFiles(Path.Combine(App.Current.UpdatePath, "Scripts")))
            {
                File.Copy(file, Path.Combine(this.FilePathOnPC, $"Scripts\\{new FileInfo(file).Name}"), true);
            }
        }

        #endregion
    }
}

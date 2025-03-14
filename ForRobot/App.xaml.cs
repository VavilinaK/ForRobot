using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows;
using System.Configuration;
//using System.Collections.ObjectModel;

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

        private static ForRobot.Libr.Settings.Settings _settings;       

        #region Widows

        private Views.Windows.MainWindow _mainWindow;


        #endregion

        #endregion

        #region Public variables

        //public string LoggerString
        //{
        //    get => this._log;
        //    set
        //    {
        //        this._log = value;
        //        this.Log(this, null);
        //    }
        //}

        public static new App Current => Application.Current as App;

        public Libr.Logger Logger { set; get; } = new Libr.Logger();

        /// <summary>
        /// Настройки приложения
        /// (Выгружаются из временных файлов, иначе инициализируются как класс)
        /// </summary>
        public ForRobot.Libr.Settings.Settings Settings
        {
            get => _settings ?? (_settings = ForRobot.Libr.Settings.Settings.GetSettings());
            set
            {
                _settings = value;
                _settings.Save();
            }
        }

        /// <summary>
        /// Открытые файлы
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<Model.File3D.File3D> OpenedFiles { get; set; } = new System.Collections.ObjectModel.ObservableCollection<Model.File3D.File3D>();
        //public FullyObservableCollection<Model.File3D.File3D> OpenedFiles { get; set; } = new FullyObservableCollection<Model.File3D.File3D>();

        #region Windows

        /// <summary>
        /// Главное окно
        /// </summary>
        public Views.Windows.MainWindow MainWindowView { get => _mainWindow ?? (_mainWindow = new Views.Windows.MainWindow()); }

        //public Views.Windows.NewWindow MainWindowView { get; } = new Views.Windows.NewWindow();

        //private Views.Windows.MainWindow2 _mainWindow2;

        //public Views.Windows.MainWindow2 MainWindowView { get => _mainWindow2 ?? (_mainWindow2 = new Views.Windows.MainWindow2()); }

        /// <summary>
        /// Окно создания нового файла
        /// </summary>
        public Views.Windows.CreateWindow CreateWindow { get; set; }

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
                foreach (var i in e.Args) // Исп. для открытия файла модели "с помощью"
                    this.OpenedFiles.Add(new Model.File3D.File3D(i));

                //"D:\Git\HelixToolkit\Models\stl\cube.stl"
                //this.OpenedFiles.Add(new Model.File3D.File3D(new Model.Detals.Plita(Model.Detals.DetalType.Plita), ""));

                // Проверка версии файла в папке с обновлением и запрос к пользователю.
                if (Settings.AutoUpdate && 
                    File.Exists(Path.Combine(App.Current.UpdatePath, $"{ResourceAssembly.GetName().Name}.exe")) &&
                    new Version(FileVersionInfo.GetVersionInfo(Path.Combine(App.Current.UpdatePath, $"{ResourceAssembly.GetName().Name}.exe")).ProductVersion) > Assembly.GetExecutingAssembly().GetName().Version &&
                    (!Settings.InformUser || MessageBox.Show($"Обнаружено обновление до версии {FileVersionInfo.GetVersionInfo(Path.Combine(App.Current.UpdatePath, $"{ResourceAssembly.GetName().Name}.exe")).ProductVersion}\nОбновить приложение?", "Обновление интерфейса", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK))
                {
                    this.Logger.Trace($"Обновление приложения до версии {FileVersionInfo.GetVersionInfo(Path.Combine(App.Current.UpdatePath, $"{ResourceAssembly.GetName().Name}.exe")).ProductVersion}");
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
                    // Проверка скрипта для обновления, если приложение не обновлялось.
                    foreach (var prop in typeof(ForRobot.Libr.ConfigurationProperties.AppConfigurationSection).GetProperties())
                    {
                        var v = typeof(ForRobot.Libr.ConfigurationProperties.AppConfigurationSection).GetProperty(prop.Name);
                        string scriptName = v.GetValue(AppConfig) as string;

                        if (Settings.AutoUpdate && 
                            (File.Exists(Path.Combine(this.FilePathOnPC, $"Scripts\\{scriptName}")) && File.Exists(Path.Combine(App.Current.UpdatePath, $"Scripts\\{scriptName}"))))
                        {
                            Version oldVersion = Version.Parse(File.ReadLines(Path.Combine(this.FilePathOnPC, $"Scripts\\{scriptName}")).Where(str => str.Contains("__version__")).First().Split(new char[] { '=' }).Last().TrimStart().Trim(new char[] { '\'' }));
                            Version newVersion = Version.Parse(File.ReadLines(Path.Combine(App.Current.UpdatePath, $"Scripts\\{scriptName}")).Where(str => str.Contains("__version__")).First().Split(new char[] { '=' }).Last().TrimStart().Trim(new char[] { '\'' }));

                            if (newVersion > oldVersion && (!Settings.InformUser ||
                                MessageBox.Show($"Обнаружено обновление скрипта {scriptName} до версии {newVersion}\nОбновить скрипт?", "Обновление скрипта-генерации", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK))
                            {
                                this.Logger.Trace($"Обновление скрипта {scriptName} до версии {newVersion}");
                                this.UpDateScript();
                            }
                        }
                    }
                }

                this.Logger.Trace("Запуск приложения");
                Application.Current.MainWindow = MainWindowView;
                MainWindowView.Show();

                GC.KeepAlive(mutex);
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, ex.Message);
                MessageBox.Show(ex.Message + "\t||\t" + ex.Source, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            Logger.Fatal(e.Exception, e.Exception.Message);
        }

        private void onExit(object sender, ExitEventArgs e)
        {
            if (((Application.Current.Windows.Count == 0) && (Application.Current.ShutdownMode == ShutdownMode.OnLastWindowClose))
                || (Application.Current.ShutdownMode == ShutdownMode.OnMainWindowClose))
            {
                this.Logger.Trace("Закрытие приложения\n\n");
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

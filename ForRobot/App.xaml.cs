using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows;
using System.Configuration;
using System.Collections.Generic;
//using System.Collections.ObjectModel;

using System.IO.Pipes;
using System.Threading.Tasks;

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

        private Mutex _mutex;
        private bool _isNewInstance;
        private const string _mutexName = "UniqueAppMutex";
        private const string _pipeName = "UniqueAppPipe";

        //private static Mutex mutex = null;

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

        private Views.Windows.MainWindow _mainWindow;

        #endregion

        #region Public variables

        public static new App Current => Application.Current as App;

        public readonly Stack<IUndoableCommand> UndoStack = new Stack<IUndoableCommand>();
        public readonly Stack<IUndoableCommand> RedoStack = new Stack<IUndoableCommand>();

        public Libr.Logger Logger { get; } = new Libr.Logger();

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

        //"D:\Git\HelixToolkit\Models\stl\cube.stl"
        /// <summary>
        /// Открытые файлы
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<Model.File3D.File3D> OpenedFiles { get; set; } = new System.Collections.ObjectModel.ObservableCollection<Model.File3D.File3D>();

        #region Windows

        /// <summary>
        /// Главное окно
        /// </summary>
        public Views.Windows.MainWindow MainWindowView { get => _mainWindow ?? (_mainWindow = new Views.Windows.MainWindow()); }

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
        private async void onStartUp(object sender, StartupEventArgs e)
        {
            try
            {
                _mutex = new Mutex(true, _mutexName, out _isNewInstance);

                if (!_isNewInstance)
                {
                    SendArgumentsToExistingInstance(e.Args);
                    Application.Current.Shutdown(0);
                    return;
                }

                foreach (var i in e.Args) // Исп. для открытия файла модели "с помощью"
                    this.OpenedFiles.Add(new Model.File3D.File3D(i));
                               
                RunApp(e.Args);
                await Task.Run(() => StartPipeServer());

                GC.KeepAlive(_mutex);
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
                if(this._isNewInstance)
                    this.Logger.Trace("Закрытие приложения\n\n");

                this._mutex?.Dispose();
                Application.Current.Shutdown(0);
            }
        }

        /// <summary>
        /// Запуск приложения
        /// </summary>
        /// <param name="args"></param>
        private void RunApp(string[] args)
        {
            // Проверка версии файла в папке с обновлением и запрос к пользователю.
            if (Settings.AutoUpdate &&
                File.Exists(Path.Combine(App.Current.UpdatePath, $"{ResourceAssembly.GetName().Name}.exe")) &&
                new Version(FileVersionInfo.GetVersionInfo(Path.Combine(App.Current.UpdatePath, $"{ResourceAssembly.GetName().Name}.exe")).ProductVersion) > Assembly.GetExecutingAssembly().GetName().Version &&
                (!Settings.InformUser || MessageBox.Show($"Обнаружено обновление до версии {FileVersionInfo.GetVersionInfo(Path.Combine(App.Current.UpdatePath, $"{ResourceAssembly.GetName().Name}.exe")).ProductVersion}\nОбновить приложение?", "Обновление интерфейса", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK))
            {
                this.Logger.Trace($"Обновление приложения до версии {FileVersionInfo.GetVersionInfo(Path.Combine(App.Current.UpdatePath, $"{ResourceAssembly.GetName().Name}.exe")).ProductVersion}");
                App.Current.UpDateApp(args);
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
        }

        /// <summary>
        /// Обновление программы.
        /// Копирует файлы из каталога на сервере в нынешнюю директорию программы и перезапускает её
        /// </summary>
        /// <param name="e">Аргументы командной стоки</param>
        private void UpDateApp(string[] args)
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
                    Arguments = $"/K taskkill /im {ResourceAssembly.GetName().Name}.exe /f& xcopy \"{this.UpdatePath + "\\*.*"}\" \"{this.FilePathOnPC}\" /E /Y& START \"\" \"{this.FilePathOnPC + "\\" + ResourceAssembly.GetName().Name + ".exe"}\" \"{string.Join("\" \"", args)}\"",
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

        /// <summary>
        /// Передача аргументов уже существующему экземпляру приложения
        /// </summary>
        /// <param name="args"></param>
        private void SendArgumentsToExistingInstance(string[] args)
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out))
                {
                    client.Connect(2000); // Таймаут 2 секунды
                    using (var writer = new StreamWriter(client))
                    {
                        foreach (var arg in args)
                        {
                            writer.WriteLine(arg);
                        }
                    }
                }
            }
            catch (TimeoutException ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Запуск сервера для перехвата аргументов
        /// </summary>
        private void StartPipeServer()
        {
            while (true)
            {
                using (var server = new NamedPipeServerStream(_pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                {
                    server.WaitForConnection();
                    using (var reader = new StreamReader(server))
                    {
                        var args = new List<string>();
                        while (!reader.EndOfStream)
                        {
                            args.Add(reader.ReadLine());
                        }
                        // Обновляем UI через Dispatcher
                        Application.Current.Dispatcher.Invoke(() => HandleArguments(args.ToArray()));
                    }
                }
            }
        }

        private void HandleArguments(string[] args)
        {
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    this.OpenedFiles.Add(new Model.File3D.File3D(args[i]));
                }
            }
            Dispatcher.Invoke(() =>
            {
                this.SelectAppMainWindow();
            });
        }

        /// <summary>
        /// Вывод и вокусировка главного окна приложения
        /// </summary>
        private void SelectAppMainWindow()
        {
            App.Current.MainWindow.Activate();
            App.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
            App.Current.MainWindow.Topmost = true;
            App.Current.MainWindow.Focus();
        }

        #endregion
    }
}

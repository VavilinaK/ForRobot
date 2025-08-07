using System;
using System.IO;
using System.Text;
using System.Data;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows;
using System.Security.Cryptography;
using System.Configuration;
using System.Collections.Generic;
//using System.Collections.ObjectModel;

using System.IO.Pipes;
using System.Threading.Tasks;

//using Newtonsoft.Json;

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
        private const string _mutexName = "InterfaceOfRobots_UniqueAppMutex";
        private const string _pipeName = "InterfaceOfRobots_UniqueAppPipe";
        private CancellationTokenSource _pipeServerCts;

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

        private static ForRobot.Model.Settings.Settings _settings;

        #endregion

        #region Public variables

        public static new App Current => Application.Current as App;

        /// <summary>
        /// Сервис открытия окон приложения
        /// </summary>
        public readonly ForRobot.Services.IWindowsAppService WindowsAppService = new ForRobot.Services.WindowsAppService();

        /// <summary>
        /// Стек возвращаемых действий (назад)
        /// </summary>
        public readonly Stack<IUndoableCommand> UndoStack = new Stack<IUndoableCommand>();
        /// <summary>
        /// Стек повторяемых действий (вперёд)
        /// </summary>
        public readonly Stack<IUndoableCommand> RedoStack = new Stack<IUndoableCommand>();

        /// <summary>
        /// Директория AvalonDock.config файла, в котором сохраняется макет интерфейса.
        /// </summary>
        public string AvalonConfigPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "AvalonDock.config");

        /// <summary>
        /// Общий логер
        /// </summary>
        public Libr.Logger Logger { get; } = new Libr.Logger();

        /// <summary>
        /// Настройки приложения
        /// (выгружаются из временных файлов, иначе инициализируются как класс)
        /// </summary>
        public ForRobot.Model.Settings.Settings Settings
        {
            get => _settings ?? (_settings = ForRobot.Model.Settings.Settings.GetSettings());
            set
            {
                _settings = value;
            }
        }

        /// <summary>
        /// Открытые файлы 3D моделей
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<Model.File3D.File3D> OpenedFiles { get; set; } = new System.Collections.ObjectModel.ObservableCollection<Model.File3D.File3D>();

        /// <summary>
        /// Обработчик сохранения настроек
        /// </summary>
        public EventHandler SaveAppSettings;

        #endregion Public variables

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

                this.Logger.Trace("Запуск приложения");

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
                if (Settings.SaveDetalProperties)
                {
                    Services.FileService.SaveFiles(OpenedFiles.Where(item => new List<string>() { ForRobot.Properties.Settings.Default.PlitaProgramm,
                                                                                                  ForRobot.Properties.Settings.Default.PlitaStringerProgramm,
                                                                                                  ForRobot.Properties.Settings.Default.PlitaTreugolnikProgramm
                                                                                                }.Contains(item.NameWithoutExtension)));
                }

                if (this._isNewInstance)
                    this.Logger.Trace("Закрытие приложения\n\n");

                _pipeServerCts?.Cancel(); // Отмена сервера каналов.
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

            Application.Current.MainWindow = WindowsAppService.AppMainWindow;
            SaveAppSettings += (s, o) => Settings.Save();
            Settings.ChangePropertyEvent += SaveAppSettings;

            // Вход в приложение по пин-коду
            if (this.Settings.LoginByPINCode)
            {
                bool pinResult = false;

                // Выполняем проверку пин-кода в UI потоке
                Application.Current.Dispatcher.Invoke(() =>
                {
                    pinResult = ForRobot.App.EqualsPinCode();
                });

                if (!pinResult)
                {
                    this.Logger.Error("Ошибка при входе: неверный пин-код!");
                    Application.Current.Shutdown(1);
                    return;
                }
            }

            WindowsAppService.AppMainWindow.Show();
            SelectAppMainWindow();
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
                    Arguments = $"/K taskkill /im {ResourceAssembly.GetName().Name}.exe /f& " +
                                $"xcopy \"{this.UpdatePath + "\\*.*"}\" \"{this.FilePathOnPC}\" /E /Y& " +
                                $"START \"\" \"{this.FilePathOnPC + "\\" + ResourceAssembly.GetName().Name + ".exe"}\" \"{string.Join("\" \"", args)}\"",
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
            _pipeServerCts = new CancellationTokenSource();
            while (!_pipeServerCts.IsCancellationRequested)
            {
                try
                {
                    using (var server = new NamedPipeServerStream(_pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                    {
                        // Асинхронное ожидание соединения с возможностью отмены
                        var asyncResult = server.BeginWaitForConnection(null, null);
                        WaitHandle.WaitAny(new[] { asyncResult.AsyncWaitHandle, _pipeServerCts.Token.WaitHandle });

                        if (_pipeServerCts.IsCancellationRequested)
                        {
                            server.Close();
                            return;
                        }

                        server.EndWaitForConnection(asyncResult);

                        if (Application.Current == null || Application.Current.Dispatcher == null || Application.Current.Dispatcher.HasShutdownStarted) return;

                        using (var reader = new StreamReader(server))
                        {
                            var args = reader.ReadToEnd().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                            if (args.Length == 0) continue;

                            bool isMainWindowReady = false; // Готовность главного окна.
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                isMainWindowReady = MainWindow != null && MainWindow.IsInitialized;
                            });

                            if (!isMainWindowReady)
                            {
                                var readyWait = new System.Threading.ManualResetEventSlim();
                                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    HandleArguments(args);
                                    readyWait.Set();
                                }));

                                if (!readyWait.Wait(TimeSpan.FromSeconds(5))) Logger.Warn("Таймаут ожидания главного окна");
                            }
                            else
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    try
                                    {
                                        HandleArguments(args);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Error(ex, "Ошибка обработки аргументов");
                                    }
                                }));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Ошибка в сервере каналов");
                    Thread.Sleep(1000); // Пауза перед повторной попыткой
                }                
            }
        }

        private void HandleArguments(string[] args)
        {
            if (args.Length > 0)
            {
                foreach (var arg in args)
                {
                    if (File.Exists(arg))
                    {
                        this.OpenedFiles.Add(new Model.File3D.File3D(arg));
                        //((App.Current.MainWindow.DataContext as ViewModels.MainWindowViewModel).NowPage.DataContext as ViewModels.MainPageViewModel3).SelectedFile = this.OpenedFiles.Last();
                        //(App.Current.MainWindow.DataContext as ViewModels.MainPageViewModel3).SelectedFile = this.OpenedFiles.Last();
                    }
                }
            }

            if (MainWindow?.IsVisible == true)
            {
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        this.SelectAppMainWindow();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Ошибка активации окна");
                    }
                });
            }
        }
        
        #endregion Private functions

        #region Public Static functions

        /// <summary>
        /// Хэширование строки
        /// </summary>
        /// <param name="str">Строка для хэширования</param>
        /// <returns></returns>
        public static string Sha256(string str)
        {
            StringBuilder Sb = new StringBuilder();
            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(str));
                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }

        /// <summary>
        /// Ввод и сравнение пин-кодов
        /// </summary>
        /// <returns>Верный ли введенный пользователем пин-код</returns>
        public static bool EqualsPinCode()
        {
            string pin = new ForRobot.Services.WindowsAppService().InputWindowShow();
            return !string.IsNullOrEmpty(pin) && Sha256(pin) == ForRobot.Properties.Settings.Default.PinCode;
        }

        /// <summary>
        /// Вывод и вокусировка главного окна приложения
        /// </summary>
        public void SelectAppMainWindow()
        {
            App.Current.MainWindow.Activate();
            //App.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
            App.Current.MainWindow.Topmost = true; // Вывод окна поверх уже открытых окон.
            App.Current.MainWindow.Topmost = false;
            App.Current.MainWindow.Focus();
        }

        #endregion
    }
}
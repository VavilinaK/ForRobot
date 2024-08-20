using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using ForRobot.Libr;
using ForRobot.Libr.Client;

namespace ForRobot.Model
{
    public class Robot : BaseClass, IDisposable
    {
        #region Private variables

        private volatile bool _disposed = false;
        private JsonRpcConnection _connection;
            
        private string _programName;
        private string _pathProgram;
        private string _pathControllerFolder;
        private int _timeout_milliseconds;
        private string _pro_state;

        private decimal _voltage;
        private decimal _current;
        private decimal _wire_feed;
        private decimal _m1;
        private decimal _tracking;

        private CancellationTokenSource _cancelTokenSource { get; set; }

        private RobotConfigurationSection Config { get; set; } = ConfigurationManager.GetSection("robot") as RobotConfigurationSection;

        private List<ForRobot.Model.Controls.File> FilesCollection = new List<Controls.File>();

        #region Readonly

        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            WriteIndented = true
        };

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Событие изменения свойств робота
        /// </summary>
        public event EventHandler ChangeRobot;
        /// <summary>
        /// Событие логирования действия
        /// </summary>
        public event EventHandler<LogEventArgs> Log;
        /// <summary>
        /// Событие логирования ошибки
        /// </summary>
        public event EventHandler<LogErrorEventArgs> LogError;

        #endregion

        #region Public variables

        [JsonIgnore]
        public string Json { get => JsonSerializer.Serialize<Robot>(this, options); }

        //[JsonIgnore]
        //public int ConnectionTimeOut { get; set; }

        [JsonPropertyName("pathProgram")]
        /// <summary>
        /// Путь к папке с программой
        /// </summary>
        public string PathProgramm
        {
            get => this._pathProgram;
            set
            {
                Set(ref this._pathProgram, value);
                this.ChangeRobot?.Invoke(this, null);
            }
        }

        //private string _pathControllerFolder;
        [JsonIgnore]
        /// <summary>
        /// Путь к папке на контроллере
        /// </summary>
        public string PathControllerFolder
        {
            //get; set;

            //get => this._pathControllerFolder ?? (this._pathControllerFolder = (string.IsNullOrEmpty(this.SearchPath(this.RobotProgramName?.ToLower() + ".src")) ? this.Config.PathControllerFolder : this.SearchPath(this.RobotProgramName?.ToLower() + ".src")));

            get
            {
                if (string.IsNullOrEmpty(this._pathControllerFolder))
                {
                    if (string.IsNullOrEmpty(this.SearchPath(this.RobotProgramName?.ToLower() + ".src")))
                        this._pathControllerFolder = this.Config.PathControllerFolder;
                    else
                        this._pathControllerFolder = this.SearchPath(this.RobotProgramName?.ToLower() + ".src");
                }
                return this._pathControllerFolder;
            }

            set => Set(ref this._pathControllerFolder, value);

            //get => this._pathControllerFolder ?? (this._pathControllerFolder = this.Config.PathControllerFolder);
            //set
            //{
            //    Set(ref this._pathControllerFolder, value);
            //    this.ChangeRobot?.Invoke(this, null);
            //}
        }

        [JsonPropertyName("host")]
        public string Host
        {
            get => this.Connection.Host;
            set
            {
                this.Connection.Host = value;
                if (this._timeout_milliseconds == 0 || this.Port == 0 || string.IsNullOrWhiteSpace(this.Connection.Host) || this.Connection.Host == "0.0.0.0")
                    return;
                this.OpenConnection(this._timeout_milliseconds);
                this.ChangeRobot?.Invoke(this, null);
            }
        }

        [JsonPropertyName("port")]
        public int Port
        {
            get => this.Connection.Port;
            set
            {
                this.Connection.Port = value;
                if (this._timeout_milliseconds == 0 || this.Connection.Port == 0 || string.IsNullOrWhiteSpace(this.Host) || this.Host == "0.0.0.0")
                    return;
                this.OpenConnection(this._timeout_milliseconds);
                this.ChangeRobot?.Invoke(this, null);
            }
        }

        [JsonIgnore]
        /// <summary>
        /// Соединение робота
        /// </summary>
        public JsonRpcConnection Connection
        {
            get => this._connection ?? (this._connection = new JsonRpcConnection());
            set => Set(ref this._connection, value);
        } 

        [JsonIgnore]
        public bool IsConnection { get => (this.Connection is null) || (this.Connection.Client is null) ? false : this.Connection.Client.Connected; }

        [JsonIgnore]
        /// <summary>
        /// Смещение по оси x, мм.
        /// </summary>
        public decimal X { get; set; } = decimal.Zero; // Поменять на вывод из соединения.

        [JsonIgnore]
        /// <summary>
        /// Смещение по оси y, мм.
        /// </summary>
        public decimal Y { get; set; } = decimal.Zero;

        [JsonIgnore]
        /// <summary>
        /// Смещение по оси z, мм.
        /// </summary>
        public decimal Z { get; set; } = decimal.Zero;

        /// <summary>
        /// Статус программы на роботе
        /// </summary>
        [JsonIgnore]
        public string Pro_State
        {
            get => this._pro_state;
            set
            {
                Set(ref this._pro_state, value);
                RaisePropertyChanged(nameof(this.ProcessState), nameof(this.RobotProgramName));
            }
        }

        [JsonIgnore]
        /// <summary>
        /// Статус процесса
        /// </summary>
        public string ProcessState
        {
            get
            {
                if (this.IsConnection)
                {
                    switch (this.Pro_State)
                    {
                        case "#P_FREE":
                            return "Программа не выбрана";

                        case "#P_RESET":
                            return $"Выбрана программа {this.RobotProgramName}";

                        case "#P_ACTIVE":
                            return $"Запущена программа {this.RobotProgramName}";

                        case "#P_STOP":
                            return $"Программа {this.RobotProgramName} остановлена";

                        case "#P_END":
                            return $"Программа {this.RobotProgramName} завершена";

                        default:
                            return "Нет соединения";
                    }
                }
                else
                    RaisePropertyChanged(nameof(this.IsConnection));
                    return "Нет соединения";
            }
        }

        [JsonIgnore]
        /// <summary>
        /// Название программы, выбранной на роботе
        /// </summary>
        public string RobotProgramName
        {
            get => this._programName;
            set
            {
                this._programName = value;
                //Set(ref this._programName, value);
                RaisePropertyChanged(nameof(this.RobotProgramName), nameof(this.ProcessState));
            }
            //get
            //{
            //    string name = "";
            //    try
            //    {
            //        name = Task.Run(async () => await this.Connection.Pro_Name()).Result.Replace("\"", "");
            //    }
            //    catch(Exception ex)
            //    {
            //        this.LogErrorMessage(ex.Message, ex);
            //    }
            //    return name;
            //}
        }

        [JsonIgnore]
        public decimal Voltage { get => this._voltage; set => Set(ref this._voltage, value); }

        [JsonIgnore]
        /// <summary>
        /// Ток
        /// </summary>
        public decimal Current { get => this._current; set => Set(ref this._current, value); }

        [JsonIgnore]
        /// <summary>
        /// Сила подачи
        /// </summary>
        public decimal WireFeed { get => this._wire_feed; set => Set(ref this._wire_feed, value); }

        [JsonIgnore]
        public decimal M1
        {
            get => this._m1;
            set
            {
                this._m1 = value;
                //RaisePropertyChanged("M1");
            }
        }

        [JsonIgnore]
        public decimal Tracking
        {
            get => this._tracking;
            set
            {
                this._tracking = value;
                //RaisePropertyChanged("Tracking");
            }
        }

        [JsonIgnore]
        public ObservableCollection<ForRobot.Model.Controls.File> Files { get => new ObservableCollection<Controls.File>(this.FilesCollection); }

        //[JsonIgnore]
        //public ForRobot.Model.Controls.File SelectedFile
        //{
        //    //get => (this.Files.Where(item => item.Name.ToLower().Contains(this.RobotProgramName.ToLower())).Count() > 0)
        //    //    ? this.Files.Where(item => item.Name.ToLower().Contains(this.RobotProgramName.ToLower())).First()
        //    //    : null;

        //    get=> this.SearchPath(this.RobotProgramName)
        //}

        //[JsonIgnore]
        //public string SelectDirectory { get => this.SearchPath(this.RobotProgramName.ToLower() + ".scr"); }

        #endregion

        #region Constructs

        public Robot(string hostname = "0.0.0.0", int port = 0000)
        {
            this.Host = hostname;
            this.Port = port;
        }

        #endregion

        #region Internal functions

        internal void LogMessage(string message)
        {
            if (string.IsNullOrEmpty(message) || this.Log == null)
            {
                return;
            }

            this.Log(this, new LogEventArgs($"{DateTime.Now.ToString("HH:mm:ss")} {this.Host}:{this.Port} " + message + "\n"));
        }

        internal void LogErrorMessage(string message) => this.LogErrorMessage(message, null);

        internal void LogErrorMessage(string message, Exception exception)
        {
            if (string.IsNullOrEmpty(message) || this.LogError == null)
            {
                return;
            }

            this.LogError(this, new LogErrorEventArgs($"{DateTime.Now.ToString("HH:mm:ss")} {this.Host}:{this.Port} " + message + "\n", exception));
        }

        #endregion

        #region Private function

        #region Asunc

        /// <summary>
        /// Переодический запрос состояния процесса на роботе
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task ProStateTimeChack(CancellationToken token)
        {
            try
            {
                while (this.IsConnection)
                {
                    var task = this.Connection.Process_State();
                    await Task.WhenAll(new Task[] { Task.Delay(1000, token), task });
                    this.Pro_State = task.Result;
                }
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        /// <summary>
        /// Переодический запрос тока на роботе
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task WeldTimeChack(CancellationToken token)
        {
            try
            {
                while (this.IsConnection)
                {
                    var task = this.Connection.In();
                    await Task.WhenAll(new Task[] { Task.Delay(3000, token), task });
                    this.ConvertToTelegraf(task.Result.ToArray());
                }
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        /// <summary>
        /// Переодический запрос имени выбранной на роботе программы
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task ProgramNameTimeChack(CancellationToken token)
        {
            try
            {
                while (this.IsConnection)
                {
                    var task = this.Connection.Pro_Name();
                    await Task.WhenAll(new Task[] { Task.Delay(1000, token), task });
                    this.RobotProgramName = task.Result.Replace("\"", "");
                }
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        #endregion

        /// <summary>
        /// Поиск пути файла по его имени
        /// </summary>
        /// <param name="sNameForSearch">Имя файла</param>
        /// <returns></returns>
        private string SearchPath(string sNameForSearch)
        {
            string path = "";
            foreach (var file in this.Files)
            {
                path = ForRobot.Libr.FileCollection.Search(file, sNameForSearch)?.Path;
                if (!string.IsNullOrEmpty(path))
                    return path;
            }
            return path;
        }

        private void OpenConnection()
        {
            try
            {
                this.Connection = new JsonRpcConnection(this.Host, this.Port);
                this.Connection.Log += this.Log;
                this.Connection.LogError += this.LogError;
                this.Connection.Connected += (sender, e) => RaisePropertyChanged(nameof(this.IsConnection));
                this.Connection.Aborted += (sender, e) => RaisePropertyChanged(nameof(this.IsConnection));
                this.Connection.Disconnected += (sender, e) => RaisePropertyChanged(nameof(this.IsConnection));

                this.Connection.Open();
                if (this.IsConnection)
                {
                    this._cancelTokenSource = new CancellationTokenSource();
                    Task.Run(async () => await this.ProgramNameTimeChack(this._cancelTokenSource.Token));
                    Task.Run(async () => await this.ProStateTimeChack(this._cancelTokenSource.Token));
                    Task.Run(async () => await this.WeldTimeChack(this._cancelTokenSource.Token));
                }
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        private void ConvertToTelegraf(char[] data)
        {
            this.Voltage = Convert.ToInt32(String.Join<byte>("", data.Skip(2064).Take(16).Reverse().Select(c => Convert.ToByte(c.ToString()))), 2) / 100;
            this.Current = Convert.ToInt32(String.Join<byte>("", data.Skip(2080).Take(16).Reverse().Select(c => Convert.ToByte(c.ToString()))), 2) / 10;
            this.WireFeed = Convert.ToInt32(String.Join<byte>("", data.Skip(2096).Take(16).Select(c => Convert.ToByte(c.ToString()))), 2) / 100;
            this.M1 = Convert.ToInt32(String.Join<byte>("", data.Skip(2160).Take(16).Reverse().Select(c => Convert.ToByte(c.ToString()))), 2) / 100;
            this.Tracking = Convert.ToInt32(String.Join<byte>("", data.Skip(2112).Take(16).Reverse().Select(c => Convert.ToByte(c.ToString()))), 2) / 10000;
        }

        #endregion

        #region Public functions

        /// <summary>
        /// Открытие соединения
        /// </summary>
        /// <param name="timeout_milliseconds"></param>
        public void OpenConnection(int timeout_milliseconds)
        {
            this._timeout_milliseconds = timeout_milliseconds;

            if (this._timeout_milliseconds == 0 || this.Port == 0 || string.IsNullOrWhiteSpace(this.Host) || this.Host == "0.0.0.0")
                return;

            Thread thread = new Thread(new ThreadStart(BeginConnect))
            {
                IsBackground = true
            };
            thread.Start();
            thread.Join(this._timeout_milliseconds);  // Закроется даже при неудачном подключении.
        }

        /// <summary>
        /// Выборка файлов робота и сборка дерева
        /// </summary>
        /// <param name="data"></param>
        /// <param name="node"></param>
        /// <param name="index"></param>
        public void GetFiles(List<ForRobot.Model.Controls.File> data = null, ForRobot.Model.Controls.File node = null, int index = 0)
        {
            if (this.IsConnection)
            {
                if (data == null)
                {
                    this.FilesCollection = new List<ForRobot.Model.Controls.File>();;

                    List<ForRobot.Model.Controls.File> fileDatas = new List<ForRobot.Model.Controls.File>();
                    var files = Task.Run<Dictionary<string, string>>(async () => await this.Connection.File_NameList()).Result;

                    foreach (var file in files.Where(item => item.Key.Split(new char[] { '\\' }).Last() != ""))
                    {
                        ForRobot.Model.Controls.File fileData = new ForRobot.Model.Controls.File(file.Key, file.Value.TrimStart(';').TrimEnd(';'));
                        fileDatas.Add(fileData);
                    }

                    GetFiles(fileDatas, null, 0);
                }
                else
                {
                    var groupData = data.Where(x => x.Path.Split(new char[] { '\\' }).ToArray().Length > index).GroupBy(x => x.Path.Split(new char[] { '\\' }).ToArray()[index]).ToList();
                    foreach (var group in groupData)
                    {
                        ForRobot.Model.Controls.File newNode = (data.Where(x => x.Path.Split(new char[] { '\\' }).Last() == group.Key).ToList().Count > 0) ?
                                            data.Where(x => x.Path.Split(new char[] { '\\' }).Last() == group.Key).ToList().First()
                                            : new ForRobot.Model.Controls.File() { Name = group.Key };
                        if (node == null)
                        {
                            this.FilesCollection.Add(newNode);
                        }
                        else
                        {
                            node.Children.Add(newNode);
                        }
                        GetFiles(group.ToList(), newNode, index + 1);
                    }
                }
            }

            //if (this.IsConnection)
            //{
            //    if (data == null)
            //    {
            //        this.FilesCollection = new List<FileData>();
            //        //this._filesCollection = new FileDataCollection();

            //        List<FileData> fileDatas = new List<FileData>();
            //        var files = Task.Run<Dictionary<string, string>>(async () => await this.Connection.File_NameList()).Result;

            //        foreach (var file in files.Where(item => item.Key.Split(new char[] { '\\' }).Last() != ""))
            //        {
            //            FileData fileData = new FileData(file.Key, file.Value.TrimStart(';').TrimEnd(';'));
            //            fileDatas.Add(fileData);
            //        }

            //        GetFiles(fileDatas, null, 0);
            //    }
            //    else
            //    {
            //        var groupData = data.Where(x => x.Path.Split(new char[] { '\\' }).ToArray().Length > index).GroupBy(x => x.Path.Split(new char[] { '\\' }).ToArray()[index]).ToList();
            //        foreach (var group in groupData)
            //        {
            //            FileData newNode = (data.Where(x => x.Path.Split(new char[] { '\\' }).Last() == group.Key).ToList().Count > 0) ?
            //                                data.Where(x => x.Path.Split(new char[] { '\\' }).Last() == group.Key).ToList().First() : new FileData() { Name = group.Key };
            //            if (node == null)
            //            {
            //                this.FilesCollection.Add(newNode);
            //            }
            //            else
            //            {
            //                node.Children.Add(newNode);
            //            }
            //            GetFiles(group.ToList(), newNode, index + 1);
            //        }
            //    }
            //}
        }

        protected void BeginConnect()
        {
            this.OpenConnection();
            this.GetFiles();
            RaisePropertyChanged(nameof(this.Files));
        }

        /// <summary>
        /// Запуск программы
        /// </summary>
        /// <returns></returns>
        public void Run()
        {
            try
            {
                if ((this.Pro_State == "#P_RESET" && MessageBox.Show($"Запустить программу {this.RobotProgramName}?", $"Запуск программы", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK) 
                    || this.Pro_State == "#P_STOP")
                {
                    if (!Task.Run<bool>(async () => await this.Connection.Start()).Result)
                        new Exception($"Ошибка запуска программы {RobotProgramName}");
                    else
                        this.LogMessage($"Программа {this.RobotProgramName} запущена");

                    do
                    {
                        if (string.Equals(this.Pro_State, "#P_STOP") || string.Equals(this.Pro_State, "#P_END"))
                        {
                            this.LogMessage($"Программа {this.RobotProgramName} остановлена/завершена");
                            return;
                        }
                    }
                    while (!string.Equals(this.Pro_State, "#P_END"));
                }

                if(this.Pro_State == "#P_END" && MessageBox.Show($"Перезапустить программу {this.RobotProgramName}?", $"Перезапуск программы", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                {
                    this.SelectProgramm(this.RobotProgramName + ".src");

                    if (!Task.Run<bool>(async () => await this.Connection.Start()).Result)
                        new Exception($"Ошибка перезапуска программы {RobotProgramName}");
                    else
                        this.LogMessage($"Программа {this.RobotProgramName} перезапущена");

                    do
                    {
                        if (string.Equals(this.Pro_State, "#P_STOP") || string.Equals(this.Pro_State, "#P_END"))
                        {
                            this.LogMessage($"Программа {this.RobotProgramName} остановлена/завершена");
                            return;
                        }
                    }
                    while (!string.Equals(this.Pro_State, "#P_END"));
                }
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        /// <summary>
        /// Остановка программы
        /// </summary>
        /// <returns></returns>
        public void Pause()
        {
            try
            {
                if (this.Pro_State == "#P_ACTIVE")
                {
                    if (!Task.Run<bool>(async () => await this.Connection.Pause()).Result)
                        new Exception($"Ошибка остановки программы {RobotProgramName}");
                    else
                        this.LogMessage($"Программа {RobotProgramName} остановлена");
                }
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        /// <summary>
        /// Аннулирование программы
        /// </summary>
        /// <returns></returns>
        public void Cancel()
        {
            try
            {
                if (this.Pro_State == "#P_RESET" || this.Pro_State == "#P_ACTIVE" || this.Pro_State == "#P_STOP" || this.Pro_State == "#P_END")
                {
                    if (string.Equals(this.Pro_State, "#P_ACTIVE"))
                    {
                        Task.Run(() => this.Pause());

                        System.Threading.Thread.Sleep(1000);
                    }

                    if (!Task.Run<bool>(async () => await this.Connection.SelectCancel()).Result)
                        new Exception("Ошибка аннулирования программы");
                    else
                        this.LogMessage("Программа аннулирована");
                }
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        /// <summary>
        /// Копирование программы в директорию робота
        /// </summary>
        /// <param name="sNameProgram">Имя главной программы (без расширения)</param>
        /// <returns></returns>
        public bool Copy(string sNameProgram)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(this.PathControllerFolder))
                {
                    this.LogErrorMessage("Нет пути на коталог на контроллере");
                    MessageBox.Show("Укажите путь к каталогу на контроллере", "Остановка", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                    return false;
                }

                switch (this.Pro_State)
                {
                    //case "#P_FREE":
                    //    if (!Equals(MessageBox.Show($"{this.Host}:{this.Port} Копировать файлы программы в {this.PathControllerFolder}?","Копирование файлов", 
                    //                                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly), MessageBoxResult.Yes))
                    //        return false;

                    //    var fileCollection1 = Task.Run<Dictionary<String, String>>(async () => await this.Connection.File_NameList(this.PathProgramm)).Result;
                    //    foreach (var file in fileCollection1.Keys.Where(i => i.EndsWith(".dat")).ToList<string>())
                    //    {
                    //        if (!Task.Run<bool>(async () => await this.Connection.Copy(Path.Combine(this.PathProgramm, file), Path.Combine(this.PathControllerFolder, new FileInfo(file).Name))).Result)
                    //            new Exception($"Ошибка копирования файла {file} в {this.PathControllerFolder}");
                    //        else
                    //            this.LogMessage($"Файл {file} скопирован ");
                    //    }

                    //    foreach (var file in fileCollection1.Keys.Where(i => i.EndsWith(".src")).ToList<string>())
                    //    {
                    //        if (!Equals(string.Join("", sNameProgram, ".src"), new FileInfo(file).Name))
                    //        {
                    //            if (!Task.Run<bool>(async () => await this.Connection.Copy(Path.Combine(this.PathProgramm, file), Path.Combine(this.PathControllerFolder, new FileInfo(file).Name))).Result)
                    //                new Exception($"Ошибка копирования файла {file} в {this.PathControllerFolder}");
                    //            else
                    //                this.LogMessage($"Файл {file} скопирован ");
                    //        }
                    //    }

                    //    if (!Task.Run<bool>(async () => await this.Connection.Copy(Path.Combine(this.PathProgramm, $"{sNameProgram}.src"), Path.Combine(this.PathControllerFolder, $"{sNameProgram}.src"))).Result)
                    //        new Exception($"Ошибка копирования файла {Path.Combine(this.PathProgramm, $"{sNameProgram}.src")} в {this.PathControllerFolder}");
                    //    else
                    //        this.LogMessage($"Файл {Path.Combine(this.PathProgramm, $"{sNameProgram}.src")} скопирован ");
                    //    break;

                    case "#P_RESET":
                    case "#P_END":
                        if (!Task.Run<bool>(async () => await this.Connection.SelectCancel()).Result)
                        {
                            this.LogErrorMessage("Не удаётся отменить выбор программы");
                            return false;
                        }
                        else
                            this.LogMessage("Текущий выбор отменён");

                        System.Threading.Thread.Sleep(1000); // Костыль).

                        //if (!Equals(MessageBox.Show($"Копировать файлы программы в {this.PathControllerFolder}?", "Копирование файлов", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly), MessageBoxResult.Yes))
                        //    return false;

                        //var fileCollection2 = Task.Run<Dictionary<String, String>>(async () => await this.Connection.File_NameList(this.PathProgramm)).Result;
                        //foreach (var file in fileCollection2.Keys.Where(i => i.EndsWith(".dat")).ToList<string>())
                        //{
                        //    if (!Task.Run<bool>(async () => await this.Connection.Copy(Path.Combine(this.PathProgramm, file), Path.Combine(this.PathControllerFolder, new FileInfo(file).Name))).Result)
                        //        new Exception($"Ошибка копирования файла {file} в {this.PathControllerFolder}");
                        //    else
                        //        this.LogMessage($"Файл {file} скопирован ");
                        //}

                        //foreach (var file in fileCollection2.Keys.Where(i => i.EndsWith(".src")).ToList<string>())
                        //{
                        //    if (!Equals(string.Join("", sNameProgram, ".src"), new FileInfo(file).Name))
                        //    {
                        //        if (!Task.Run<bool>(async () => await this.Connection.Copy(Path.Combine(this.PathProgramm, file), Path.Combine(this.PathControllerFolder, new FileInfo(file).Name))).Result)
                        //            new Exception($"Ошибка копирования файла {file} в {this.PathControllerFolder}");
                        //        else
                        //            this.LogMessage($"Файл {file} скопирован ");
                        //    }
                        //}

                        //if (!Task.Run<bool>(async () => await this.Connection.Copy(Path.Combine(this.PathProgramm, $"{sNameProgram}.src"), Path.Combine(this.PathControllerFolder, $"{programName}.src"))).Result)
                        //    new Exception($"Ошибка копирования файла {Path.Combine(this.PathProgramm, $"{sNameProgram}.src")} в {this.PathControllerFolder}");
                        //else
                        //    this.LogMessage($"Файл {Path.Combine(this.PathProgramm, $"{sNameProgram}.src")} скопирован ");
                        break;

                    case "#P_ACTIVE":
                    case "#P_STOP":
                        this.LogMessage("Отмена копирования: уже запущен процесс!");
                        return false;
                }

                if (!Equals(MessageBox.Show($"Копировать файлы программы в {this.PathControllerFolder}?", "Копирование файлов", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly), MessageBoxResult.Yes))
                    return false;

                var fileCollection2 = Task.Run<Dictionary<String, String>>(async () => await this.Connection.File_NameList(this.PathProgramm)).Result;
                foreach (var file in fileCollection2.Keys.Where(i => i.EndsWith(".dat")).ToList<string>())
                {
                    if (!Task.Run<bool>(async () => await this.Connection.Copy(Path.Combine(this.PathProgramm, file), Path.Combine(this.PathControllerFolder, new FileInfo(file).Name))).Result)
                        new Exception($"Ошибка копирования файла {file} в {this.PathControllerFolder}");
                    else
                        this.LogMessage($"Файл {file} скопирован ");
                }

                foreach (var file in fileCollection2.Keys.Where(i => i.EndsWith(".src")).ToList<string>())
                {
                    if (!Equals(string.Join("", sNameProgram, ".src"), new FileInfo(file).Name))
                    {
                        if (!Task.Run<bool>(async () => await this.Connection.Copy(Path.Combine(this.PathProgramm, file), Path.Combine(this.PathControllerFolder, new FileInfo(file).Name))).Result)
                            new Exception($"Ошибка копирования файла {file} в {this.PathControllerFolder}");
                        else
                            this.LogMessage($"Файл {file} скопирован ");
                    }
                }

                if (!Task.Run<bool>(async () => await this.Connection.Copy(Path.Combine(this.PathProgramm, $"{sNameProgram}.src"), Path.Combine(this.PathControllerFolder, $"{sNameProgram}.src"))).Result)
                    new Exception($"Ошибка копирования файла {Path.Combine(this.PathProgramm, $"{sNameProgram}.src")} в {this.PathControllerFolder}");
                else
                    this.LogMessage($"Файл {Path.Combine(this.PathProgramm, $"{sNameProgram}.src")} скопирован ");
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Копирование на компьютер
        /// </summary>
        /// <param name="sNameProgram">Имя главной программы (без расширения)</param>
        public void CopyToPC(string sNameProgram)
        {
            try
            {
                foreach (var file in Directory.GetFiles(this.PathProgramm, "*.dat"))
                {
                    if (!Task.Run<bool>(async () => await this.Connection.CopyMem2File(Path.Combine(this.PathProgramm, new FileInfo(file).Name), Path.Combine(this.PathProgramm, new FileInfo(file).Name))).Result)
                        new Exception($"Ошибка копирования содержимого файла {file} в {Path.Combine(this.PathProgramm, new FileInfo(file).Name)}");
                    else
                        this.LogMessage($"Содержимое файла {file} скопировано");
                }

                foreach (var file in Directory.GetFiles(this.PathProgramm, "*.src"))
                {
                    if (!Equals(sNameProgram, new FileInfo(file).Name))
                    {
                        if (!Task.Run<bool>(async () => await this.Connection.CopyMem2File(Path.Combine(this.PathProgramm, new FileInfo(file).Name), Path.Combine(this.PathProgramm, new FileInfo(file).Name))).Result)
                            new Exception($"Ошибка копирования содержимого файла {file} в { Path.Combine(this.PathProgramm, new FileInfo(file).Name)}");
                        else
                            this.LogMessage($"Содержимое файла {file} скопировано");
                    }
                }

                if (!Task.Run<bool>(async () => await this.Connection.CopyMem2File(Path.Combine(this.PathProgramm, sNameProgram), Path.Combine(this.PathProgramm, sNameProgram))).Result)
                    new Exception($"Ошибка копирования содержимого файла {sNameProgram}.src в { Path.Combine(this.PathProgramm, sNameProgram)}");
                else
                    this.LogMessage($"Содержимое файла {Path.Combine(this.PathProgramm, sNameProgram)} скопировано");
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        /// <summary>
        /// Выбор программы
        /// </summary>
        /// <param name="sNameProgram">Имя выбираемой программы (с расширением)</param>
        public void SelectProgramm(string sNameProgram)
        {
            try
            {
                if (string.IsNullOrEmpty(this.SearchPath(sNameProgram)))
                {
                    this.LogErrorMessage("Не найдена директория файла!");
                    return;
                }

                switch (this.Pro_State)
                {
                    case "#P_RESET":
                    case "#P_END":
                        if (!Task.Run<bool>(async () => await this.Connection.SelectCancel()).Result)
                        {
                            this.LogErrorMessage("Не удаётся отменить выбор программы");
                            return;
                        }
                        else
                            this.LogMessage("Текущий выбор отменён");

                        System.Threading.Thread.Sleep(1000);
                        break;

                    case "#P_ACTIVE":
                    case "#P_STOP":
                        this.LogMessage("Отмена выбора: уже запущен процесс!");
                        return;
                }

                string sFilePath = Path.Combine(JsonRpcConnection.DefaulRoot, this.SearchPath(sNameProgram));

                if (Task.Run<bool>(async () => await this.Connection.Select(sFilePath)).Result)
                    this.LogMessage($"Файл программы {sFilePath} выбран");
                else
                    new Exception("Ошибка выбора файла " + sFilePath);
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        /// <summary>
        /// Удаление файлов программы на ПК
        /// </summary>
        public void DeleteProgramOnPC()
        {
            foreach (var file in (Task.Run<Dictionary<String, String>>(async () => await this.Connection.File_NameList(this.PathProgramm)).Result).Keys.ToList<string>())
            {
                if (!Task.Run<bool>(async () => await this.Connection.Delet(Path.Combine(this.PathProgramm, file))).Result)
                    new Exception($"Ошибка удаления файла {file}");
                else
                    this.LogMessage($"Файл программы {file} удалён");
            }
        }

        /// <summary>
        /// Удаление файлов из папки на контроллере
        /// </summary>
        public void DeleteProgramm()
        {
            try
            {
                switch (this.Pro_State)
                {
                    //case "#P_FREE":
                    //    foreach (var file in (Task.Run<Dictionary<String, String>>(async () => await this.Connection.File_NameList(this.PathControllerFolder)).Result).Keys.ToList<string>())
                    //    {
                    //        if (!Task.Run<bool>(async () => await this.Connection.Delet(Path.Combine(this.PathControllerFolder, new FileInfo(file).Name))).Result)
                    //            new Exception($"Ошибка удаления файла {Path.Combine(this.PathControllerFolder, new FileInfo(file).Name)}");
                    //        else
                    //            this.LogMessage($"Файл программы {Path.Combine(this.PathControllerFolder, new FileInfo(file).Name)} удалён");
                    //    }
                    //    break;

                    case "#P_RESET":
                    case "#P_END":
                        if (!Task.Run<bool>(async () => await this.Connection.SelectCancel()).Result)
                        {
                            this.LogErrorMessage("Не удаётся отменить выбор программы");
                            return;
                        }
                        else
                            this.LogMessage("Текущий выбор отменён");

                        System.Threading.Thread.Sleep(1000);

                        //foreach (var file in (Task.Run<Dictionary<String, String>>(async () => await this.Connection.File_NameList(this.PathControllerFolder)).Result).Keys.ToList<string>())
                        //{
                        //    if (!Task.Run<bool>(async () => await this.Connection.Delet(Path.Combine(this.PathControllerFolder, new FileInfo(file).Name))).Result)
                        //        new Exception($"Ошибка удаления файла {Path.Combine(this.PathControllerFolder, new FileInfo(file).Name)}");
                        //    else
                        //        this.LogMessage($"Файл программы {Path.Combine(this.PathControllerFolder, new FileInfo(file).Name)} удалён");
                        //}
                        break;

                    case "#P_ACTIVE":
                    case "#P_STOP":
                        this.LogMessage("Отмена удаления: уже запущен процесс!");
                        return;
                }

                foreach (var file in (Task.Run<Dictionary<String, String>>(async () => await this.Connection.File_NameList(this.PathControllerFolder)).Result).Keys.ToList<string>())
                {
                    if (!Task.Run<bool>(async () => await this.Connection.Delet(Path.Combine(this.PathControllerFolder, new FileInfo(file).Name))).Result)
                        new Exception($"Ошибка удаления файла {Path.Combine(this.PathControllerFolder, new FileInfo(file).Name)}");
                    else
                        this.LogMessage($"Файл программы {Path.Combine(this.PathControllerFolder, new FileInfo(file).Name)} удалён");
                }
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        #endregion

        #region Implementations of IDisposable

        ~Robot() => Dispose(false);

        public void Dispose() => this.Dispose(true);

        public void Dispose(bool disposing)
        {
            if (this._disposed) return;
            if (disposing)
            {
                this._cancelTokenSource?.Cancel();
                if (this.Connection == null)
                {
                    this._disposed = true;
                    return;
                }
                else
                    this.Connection.Dispose();
            }
            this._disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

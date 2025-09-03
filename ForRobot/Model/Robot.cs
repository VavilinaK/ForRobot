using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using ForRobot.Libr;
using ForRobot.Libr.Client;

namespace ForRobot.Model
{
    public class Robot : INotifyPropertyChanged, IDisposable
    {
        #region Private variables

        private volatile bool _disposed = false;
        private JsonRpcConnection _connection;

        private string _name;
        private string _programName;
        private string _pathProgram;
        private string _pathControllerFolder;
        private string _pro_state;

        private decimal _voltage;
        private decimal _current;
        private decimal _wire_feed;
        private decimal _m1;
        private decimal _tracking;

        /// <summary>
        /// Токен отмены переодического запроса: статуса процесса, тока и активной программы
        /// </summary>
        private CancellationTokenSource _periodicTaskCancelTokenSource;

        private Task LoadFilesTask;

        private List<ForRobot.Model.Controls.IFile> FilesCollection = new List<Controls.IFile>();

        //private decimal[] _currentArray = new decimal[] { }; // = new decimal[] { 0, 4, 6, 5, 3, -3, -1, 2 }; // Примерные данные
        //private decimal[] _wireFeedArray = new decimal[] { }; // new decimal[] { 0, 4, 6, 3, 6, -3, -1, 2 }; // Примерные данные

        #region Readonly

        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            WriteIndented = true
        };

        #endregion

        #endregion

        #region Events

        //public event EventHandler ChangeRobot;
        //public event PropertyChangedEventHandler PropertyChangedEvent;
        /// <summary>
        /// Событие выгрузки файлов
        /// </summary>
        public event EventHandler LoadedFilesEvent;
        /// <summary>
        /// Событие изменения пути на контроллере
        /// </summary>
        //public event EventHandler ChangedControllerPathEvent;
        /// <summary>
        /// Событие логирования действия
        /// </summary>
        public event EventHandler<LogEventArgs> Log;
        /// <summary>
        /// Событие логирования ошибки
        /// </summary>
        public event EventHandler<LogErrorEventArgs> LogError;
        /// <summary>
        /// Событие изменения свойств робота
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public variables

        #region Constants

        [JsonIgnore]
        public const string PathOfTempFolder = @"C:\Windows\Temp";
        [JsonIgnore]
        public const string DefaultHost = "0.0.0.0";
        [JsonIgnore]
        public const int DefaultPort = 3333;
        [JsonIgnore]
        /// <summary>
        /// Задержка запроса состояния процесса на роботе
        /// </summary>
        public const int DelayProcessStatus = 1;
        [JsonIgnore]
        /// <summary>
        /// Задержка запроса
        /// </summary>
        public const int DelayTelegraf = 3;
        [JsonIgnore]
        /// <summary>
        /// Задержка запроса имени запущенной на роботе программы
        /// </summary>
        public const int DelayProgramName = 3;

        #endregion Constants

        /// <summary>
        /// Наименование робота
        /// </summary>
        public string Name
        {
            get => this._name;
            set
            {
                this._name = value;
                this.OnChangeProperty();
            }
        }

        [JsonIgnore]
        /// <summary>
        /// JSON-строка для сохранения
        /// </summary>
        public string Json { get => JsonSerializer.Serialize<Robot>(this, options); }

        /// <summary>
        /// Путь к папке с программой
        /// </summary>
        public string PathProgramm
        {
            get => this._pathProgram;
            set
            {
                this._pathProgram = value;
                this.OnChangeProperty();
                //this.ChangeRobot?.Invoke(this, null);
            }
        }

        /// <summary>
        /// Путь к папке на контроллере
        /// </summary>
        public string PathControllerFolder
        {
            get => this._pathControllerFolder;
            set
            {
                this._pathControllerFolder = value;
                this.OnChangeProperty();
                //this.ChangeRobot?.Invoke(this, null);
                //this.ChangedControllerPathEvent?.Invoke(this, null);
            }
        }

        public string Host
        {
            get => this.Connection.Host;
            set
            {
                this.Connection.Host = value;
                if (this.Port == 0 || string.IsNullOrWhiteSpace(this.Connection.Host) || this.Connection.Host == "0.0.0.0")
                    return;
                this.OpenConnection();
                this.OnChangeProperty();
                //this.ChangeRobot?.Invoke(this, null);
            }
        }

        public int Port
        {
            get => this.Connection.Port;
            set
            {
                this.Connection.Port = value;
                if (this.Connection.Port == 0 || string.IsNullOrWhiteSpace(this.Host) || this.Host == "0.0.0.0")
                    return;
                this.OpenConnection();
                this.OnChangeProperty();
                //this.ChangeRobot?.Invoke(this, null);
            }
        }

        /// <summary>
        /// Время ожидания ответа от сервера, сек.
        /// </summary>
        public int ConnectionTimeOutMilliseconds { get; set; } = 0;

        [JsonIgnore]
        /// <summary>
        /// Соединение робота
        /// </summary>
        public JsonRpcConnection Connection
        {
            get => this._connection ?? (this._connection = new JsonRpcConnection());
            set
            {
                if (this._connection != null)
                {
                    this._connection.Connected -= (s, e) => this.OnChangeProperty();
                    this._connection.Aborted -= (s, e) => this.OnChangeProperty();
                    this._connection.Disconnected -= (s, e) => this.OnChangeProperty();
                }

                this._connection = value;
                this.OnChangeProperty();

                if (this._connection != null)
                {
                    this._connection.Connected += (s, e) => this.OnChangeProperty();
                    this._connection.Aborted += (s, e) => this.OnChangeProperty();
                    this._connection.Disconnected += (s, e) => this.OnChangeProperty();
                }
            }
        }

        /// <summary>
        /// Токен отмены задачи запуска программы на роботе (нужен при зажатии кнопки)
        /// </summary>
        public CancellationTokenSource RunCancelTokenSource;

        [JsonIgnore]
        /// <summary>
        /// Активно ли соединение
        /// </summary>
        public bool IsConnection { get => (this.Connection is null) || (this.Connection.Client is null) ? false : this.Connection.Client.Connected; }

        [JsonIgnore]
        /// <summary>
        /// Загружаются ли файлы в данный момент
        /// </summary>
        public bool IsLoadFiles { get => (this.LoadFilesTask?.Status == TaskStatus.Running) ? true : false; }

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
            get => this.IsConnection ? this._pro_state : null;
            set
            {
                this._pro_state = value;
                this.OnChangeProperty();
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
                this.OnChangeProperty();
            }
        }

        [JsonIgnore]
        public decimal Voltage
        {
            get => this._voltage;
            set
            {
                this._voltage = value;
                this.OnChangeProperty();
            }
        }

        [JsonIgnore]
        /// <summary>
        /// Ток
        /// </summary>
        public decimal Current
        {
            get => this._current;
            set
            {
                this._current = value;
                this.OnChangeProperty();
                //this.CurrentArray.Append(this._current);
            }
        }

        [JsonIgnore]
        /// <summary>
        /// Сила подачи
        /// </summary>
        public decimal WireFeed
        {
            get => this._wire_feed;
            set
            {
                this._wire_feed = value;
                this.OnChangeProperty();
                //this.WireFeedArray.Append(this._wire_feed);
            }
        }

        [JsonIgnore]
        public decimal M1
        {
            get => this._m1;
            set
            {
                this._m1 = value;
                this.OnChangeProperty();
            }
        }

        [JsonIgnore]
        public decimal Tracking
        {
            get => this._tracking;
            set
            {
                this._tracking = value;
                this.OnChangeProperty();
            }
        }

        [JsonIgnore]
        /// <summary>
        /// Файлы роботов
        /// </summary>
        public ForRobot.Model.Controls.File Files { get; } = new Controls.File(JsonRpcConnection.DefaulRoot);

        //[JsonIgnore]
        ///// <summary>
        ///// Массив значений тока
        ///// </summary>
        //public decimal[] CurrentArray { get => this._currentArray; set => Set(ref this._currentArray, value); }

        //[JsonIgnore]
        ///// <summary>
        ///// Массив значений силы подачи
        ///// </summary>
        //public decimal[] WireFeedArray { get => this._wireFeedArray; set => Set(ref this._wireFeedArray, value); }

        #endregion

        #region Constructor

        public Robot(string hostname = DefaultHost, int port = DefaultPort)
        {
            this.Host = hostname;
            this.Port = port;
        }

        #endregion

        #region Private function

        private void LogMessage(string message)
        {
            if (string.IsNullOrEmpty(message) || this.Log == null)
                return;

            this.Log(this, new LogEventArgs(String.Format("{0}:{1}\t{2}", this.Host, this.Port, message)));
        }

        private void LogErrorMessage(string message) => this.LogErrorMessage(message, null);

        private void LogErrorMessage(string message, Exception exception)
        {
            if (string.IsNullOrEmpty(message) || this.LogError == null)
                return;

            this.LogError(this, new LogErrorEventArgs(String.Format("{0}:{1}\t{2}", this.Host, this.Port, message), exception));
        }

        /// <summary>
        /// Начало подключения
        /// </summary>
        private void BeginConnect()
        {
            try
            {
                this.Connection = new JsonRpcConnection(this.Host, this.Port);
                this.Connection.Log += this.Log;
                this.Connection.LogError += this.LogError;
                this.Connection.Connected += (sender, e) => this.OnChangeProperty();
                this.Connection.Aborted += (sender, e) =>
                {
                    this.LogMessage("Соединение разорвано со стороны сервера");
                    this.OnChangeProperty();
                };
                this.Connection.Disconnected += (sender, e) =>
                {
                    this.CloseConnection();
                    this.OnChangeProperty();
                };

                this.LogMessage($"Открытие соединения с сервером . . .");
                if (this.Connection.Open(this.ConnectionTimeOutMilliseconds))
                    this.LogMessage($"Открыто соединение");

                if (this.IsConnection)
                {
                    this._periodicTaskCancelTokenSource = new CancellationTokenSource();

                    var task1 = PeriodicTask(() => { if (this.IsConnection) this.Pro_State = this.Connection.Process_StateAsync().Result; }, new TimeSpan(0, 0, 0, 0, DelayProcessStatus * 1000), this._periodicTaskCancelTokenSource.Token); // Переодический запрос состояния процесса на роботе.

                    var task2 = PeriodicTask(() => { if (this.IsConnection) this.ConvertToTelegraf(this.Connection.InAsync().Result.ToArray()); }, new TimeSpan(0, 0, 0, 0, DelayTelegraf * 1000), this._periodicTaskCancelTokenSource.Token); // Переодический запрос тока на роботе.

                    var task3 = PeriodicTask(() => { if (this.IsConnection) this.RobotProgramName = this.Connection.Pro_NameAsync().Result.Replace("\"", ""); }, new TimeSpan(0, 0, 0, 0, DelayProgramName * 1000), this._periodicTaskCancelTokenSource.Token); // Переодический запрос имени выбранной на роботе программы

                    Task.Run(async () => await Task.WhenAll(task1, task2, task3));
                    Task.Run(async () => await this.GetFilesAsync());
                }
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        /// <summary>
        /// Сборка дерева файлов
        /// </summary>
        /// <param name="data"></param>
        /// <param name="node"></param>
        /// <param name="index"></param>
        private void LoadFiles(List<ForRobot.Model.Controls.IFile> data = null, ForRobot.Model.Controls.IFile node = null, int index = 1)
        {
            if (!this.IsConnection)
                return;

            if (data == null)
            {
                this.FilesCollection = new List<ForRobot.Model.Controls.IFile>();
                List<ForRobot.Model.Controls.File> fileDatas = new List<ForRobot.Model.Controls.File>();
                try
                {
                    var files = Task.Run<Dictionary<string, string>>(async () => await this.Connection.File_NameListAsync()).Result;

                    foreach (var file in files)
                    {
                        ForRobot.Model.Controls.File fileData = new ForRobot.Model.Controls.File(file.Key.TrimEnd(new char[] { '\\' }), file.Value.TrimStart(';').TrimEnd(';'));
                        fileData.Path = Path.Combine(JsonRpcConnection.DefaulRoot, fileData.Path);
                        fileDatas.Add(fileData);
                    }
                }
                catch (Exception ex)
                {
                    this.LogErrorMessage(ex.Message, ex);
                }
                LoadFiles(fileDatas.OrderBy(item => item.Path).ToList<ForRobot.Model.Controls.IFile>(), node, index);
            }
            else
            {
                var groupData = data.Where(x => x.Path.Split(new char[] { '\\' }).ToArray().Length > index).GroupBy(x => x.Path.Split(new char[] { '\\' }).ToArray()[index]).ToList();
                foreach (var group in groupData)
                {
                    ForRobot.Model.Controls.IFile newNode = data.Where(x => x.Name == group.Key).ToList().First();
                    if (node == null)
                    {
                        this.FilesCollection.Add(newNode);
                    }
                    else
                    {
                        node.Children.Add(newNode);
                    }
                    LoadFiles(group.ToList(), newNode, index + 1);
                }
            }
        }
        //{
        //    if (!this.IsConnection)
        //        return;

        //    if (data == null)
        //    {
        //        this.FilesCollection = new List<ForRobot.Model.Controls.IFile>() { new Controls.File(JsonRpcConnection.DefaulRoot) };
        //        List<ForRobot.Model.Controls.File> fileDatas = new List<ForRobot.Model.Controls.File>();
        //        try
        //        {
        //            var files = Task.Run<Dictionary<string, string>>(async () => await this.Connection.File_NameListAsync()).Result;

        //            foreach (var file in files)
        //            {
        //                ForRobot.Model.Controls.File fileData = new ForRobot.Model.Controls.File(file.Key.TrimEnd(new char[] { '\\' }), file.Value.TrimStart(';').TrimEnd(';'));
        //                fileData.Path = Path.Combine(JsonRpcConnection.DefaulRoot, fileData.Path);
        //                fileDatas.Add(fileData);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            this.LogErrorMessage(ex.Message, ex);
        //        }
        //        LoadFiles(fileDatas.OrderBy(item => item.Path).ToList<ForRobot.Model.Controls.IFile>(), node, index);
        //    }
        //    else
        //    {
        //        var groupData = data.Where(x => x.Path.Split(new char[] { '\\' }).ToArray().Length > index).GroupBy(x => x.Path.Split(new char[] { '\\' }).ToArray()[index]).ToList();
        //        foreach (var group in groupData)
        //        {
        //            ForRobot.Model.Controls.File newNode = data.Where(x => x.Name == group.Key).ToList().First();
        //            if (node == null)
        //            {
        //                this.FilesCollection.Add(newNode);
        //            }
        //            else
        //            {
        //                node.Children.Add(newNode);
        //            }
        //            LoadFiles(group.ToList(), newNode, index + 1);
        //        }
        //    }
        //}

        /// <summary>
        /// Поиск пути файла по его имени
        /// </summary>
        /// <param name="sNameForSearch">Имя файла</param>
        /// <returns></returns>
        private string SearchPath(string sNameForSearch)
        {
            foreach (var file in this.Files.Children)
            {
                string path = ForRobot.Libr.FileCollection.Search(file, sNameForSearch)?.Path;
                if (!string.IsNullOrEmpty(path))
                    return path;
            }
            return string.Empty;
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
        /// Вызов события изменения свойства
        /// </summary>
        /// <param name="propertyName">Наименование свойства</param>
        public virtual void OnChangeProperty([CallerMemberName] string propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Открытие соединения
        /// </summary>
        /// <param name="timeout_milliseconds"></param>
        public bool OpenConnection()
        {
            if (this.Port == 0 || string.IsNullOrWhiteSpace(this.Host) || this.Host == "0.0.0.0")
                return false;

            Thread thread = new Thread(new ThreadStart(BeginConnect))
            {
                IsBackground = true
            };
            thread.Start();
            thread.Join(this.ConnectionTimeOutMilliseconds);  // Закроется даже при неудачном подключении.
            return this.IsConnection;
        }

        /// <summary>
        /// Закрытие соединения.
        /// </summary>
        public void CloseConnection()
        {
            this._periodicTaskCancelTokenSource?.Cancel();
            this.RunCancelTokenSource?.Cancel();

            if (this.IsConnection)
            {
                if (this.Connection.Close())
                {
                    this.Connection.Dispose();
                }
            }
        }

        /// <summary>
        /// Запуск выбранной программы
        /// </summary>
        /// <returns></returns>
        public void Run()
        {
            try
            {
                if(this.Pro_State == ProcessStatuses.End)
                {
                    this.SelectProgramByName(this.RobotProgramName + ".src");

                    if (!Task.Run<bool>(async () => await this.Connection.StartAsync()).Result)
                        throw new Exception($"Ошибка перезапуска программы {RobotProgramName}");
                    else
                        this.LogMessage($"Программа {this.RobotProgramName} перезапущена");
                }

                if (this.Pro_State == ProcessStatuses.Reset || this.Pro_State == ProcessStatuses.Stop)
                {
                    if (!Task.Run<bool>(async () => await this.Connection.StartAsync()).Result)
                        throw new Exception($"Ошибка запуска программы {RobotProgramName}");
                    else
                        this.LogMessage($"Программа {this.RobotProgramName} запущена");
                }

                do
                {
                    if (string.Equals(this.Pro_State, ProcessStatuses.Stop) || string.Equals(this.Pro_State, ProcessStatuses.End))
                    {
                        this.LogMessage($"Программа {this.RobotProgramName} остановлена/завершена");
                        return;
                    }
                }
                while (!string.Equals(this.Pro_State, ProcessStatuses.End));
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
                if (this.Pro_State == ProcessStatuses.Active)
                {
                    if (!Task.Run<bool>(async () => await this.Connection.PauseAsync()).Result)
                        throw new Exception($"Ошибка остановки программы {RobotProgramName}");
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
                if (this.Pro_State == ProcessStatuses.Reset || this.Pro_State == ProcessStatuses.Active || this.Pro_State == ProcessStatuses.Stop || this.Pro_State == ProcessStatuses.End)
                {
                    if (string.Equals(this.Pro_State, ProcessStatuses.Active))
                    {
                        Task.Run(() => this.Pause());

                        System.Threading.Thread.Sleep(1000);
                    }

                    if (!Task.Run<bool>(async () => await this.Connection.SelectCancelAsync()).Result)
                        throw new Exception("Не удаётся отменить выбор программы");
                    else
                        this.LogMessage("Текущий выбор программы отменён");
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
        /// <param name="sPathOnPC">Путь к файлу на ПК</param>
        /// <param name="sPathOnController">Новый путь на контроллере</param>
        /// <returns></returns>
        public bool Copy(string sPathOnPC, string sPathOnController)
        {
            try
            {
                switch (this.Pro_State)
                {
                    case ProcessStatuses.Reset:
                    case ProcessStatuses.End:
                        this.Cancel();
                        System.Threading.Thread.Sleep(1000); // Костыль).
                        break;

                    case ProcessStatuses.Active:
                    case ProcessStatuses.Stop:
                        this.LogMessage("Отмена копирования: уже запущен процесс!");
                        return false;
                }

                if (Task.Run<bool>(async () => await this.Connection.CopyAsync(sPathOnPC, sPathOnController)).Result)
                    this.LogMessage($"Файл {sPathOnPC} скопирован в {sPathOnController}");
                else
                    throw new Exception($"Ошибка копирования файла {sPathOnPC} в {sPathOnController}");
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Копирование программы в директорию робота 
        /// </summary>
        /// <param name="sNameProgram">Имя главной программы (с расширением)</param>
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
                    case ProcessStatuses.Reset:
                    case ProcessStatuses.End:
                        this.Cancel();
                        System.Threading.Thread.Sleep(1000); // Костыль).
                        break;

                    case ProcessStatuses.Active:
                    case ProcessStatuses.Stop:
                        this.LogMessage("Отмена копирования: уже запущен процесс!");
                        return false;
                }

                var fileCollection2 = Task.Run<Dictionary<String, String>>(async () => await this.Connection.File_NameListAsync(this.PathProgramm)).Result;

                foreach (var file in fileCollection2.Keys.Where(i => i.EndsWith(".dat")).ToList<string>())
                {
                    this.Copy(Path.Combine(this.PathProgramm, file), Path.Combine(this.PathControllerFolder, file));
                }

                foreach (var file in fileCollection2.Keys.Where(i => i.EndsWith(".src") && i != sNameProgram).ToList<string>())
                {
                    this.Copy(Path.Combine(this.PathProgramm, file), Path.Combine(this.PathControllerFolder, file));
                }

                this.Copy(Path.Combine(this.PathProgramm, sNameProgram), Path.Combine(this.PathControllerFolder, sNameProgram));

                Task.Run(async () => await this.GetFilesAsync()).Wait();
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Копирование файла на компьютер
        /// </summary>
        /// <param name="sFilePath">Путь файла</param>
        /// <param name="sPCPath">Путь на компьютере</param>
        /// <returns></returns>
        public bool CopyToPC(string sFilePath, string sPCPath)
        {
            try
            {
                if (Task.Run<bool>(async () => await Connection.CopyMem2FileAsync(sFilePath, sPCPath)).Result)
                    this.LogMessage($"Содержимое файла {sFilePath} скопировано");
                else
                    throw new Exception($"Ошибка копирования содержимого файла {sFilePath} в {sPCPath}");
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Копирование на компьютер из <see cref="PathProgramm"/>
        /// </summary>
        /// <param name="sNameProgram">Имя главной программы (с расширением)</param>
        public bool CopyToPC(string sNameProgram)
        {
            try
            {
                foreach (var file in Directory.GetFiles(this.PathProgramm, "*.dat"))
                {
                    this.CopyToPC(file, file);
                }

                foreach (var file in Directory.GetFiles(this.PathProgramm, "*.src").Where<string>(item => new FileInfo(item).Name != sNameProgram))
                {
                    this.CopyToPC(file, file);
                }

                string mainProgramPath = Path.Combine(this.PathProgramm, sNameProgram);
                this.CopyToPC(mainProgramPath, mainProgramPath);
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Выбор программы по имени
        /// </summary>
        /// <param name="sProgramName">Наименование программы (с расширением)</param>
        public void SelectProgramByName(string sProgramName)
        {
            try
            {
                string path = this.SearchPath(sProgramName);

                if (string.IsNullOrEmpty(path))
                    throw new Exception($"Не найдена директория файла {sProgramName}!");

                this.SelectProgramByPath(path);
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        /// <summary>
        /// Выбор программы по пути
        /// </summary>
        /// <param name="sProgramPath">Путь к выбираемой программе</param>
        public void SelectProgramByPath(string sProgramPath)
        {
            try
            {
                switch (this.Pro_State)
                {
                    case ProcessStatuses.Reset:
                    case ProcessStatuses.End:
                        this.Cancel();
                        System.Threading.Thread.Sleep(1000);
                        break;

                    case ProcessStatuses.Active:
                    case ProcessStatuses.Stop:
                        this.LogMessage("Отмена выбора: уже запущен процесс!");
                        return;
                }

                if (Task.Run<bool>(async () => await this.Connection.SelectAsync(sProgramPath)).Result)
                    this.LogMessage($"Выбран файл {sProgramPath}.");
                else
                    throw new Exception($"Ошибка выбора файла {sProgramPath}.");
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
        }
        
        /// <summary>
        /// Удаление файла с контроллера по пути
        /// </summary>
        /// <param name="sPathToFile">Путь к файлу</param>
        /// <returns></returns>
        public bool DeleteFile(string sPathToFile)
        {
            try
            {
                switch (this.Pro_State)
                {
                    case ProcessStatuses.Reset:
                    case ProcessStatuses.End:
                        this.Cancel();
                        System.Threading.Thread.Sleep(1000);
                        break;

                    case ProcessStatuses.Active:
                    case ProcessStatuses.Stop:
                        this.LogMessage("Отмена удаления: уже запущен процесс!");
                        return false;
                }

                if (!Task.Run<bool>(async () => await this.Connection.DeletAsync(sPathToFile)).Result)
                    throw new Exception($"Ошибка удаления файла {sPathToFile}");
                else
                    this.LogMessage($"Файл программы {sPathToFile} удалён");                
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Удаление файлов программы на ПК
        /// </summary>
        /// <param name="sPathOnFile">Путь к папке, из которой удаляются файлы</param>
        /// <returns></returns>
        public bool DeleteFileOnPC(string sPathOnFolder)
        {
            try
            {
                Dictionary<String, String> files = Task.Run<Dictionary<String, String>>(async () => await this.Connection.File_NameListAsync(sPathOnFolder)).Result;
                if (files.Count <= 0)
                    throw new Exception($"Не удалось найти папку {sPathOnFolder} или она пустая");
                foreach (var file in files.Keys)
                {
                    if (!Task.Run<bool>(async () => await this.Connection.DeletAsync(Path.Combine(this.PathProgramm, file))).Result)
                        throw new Exception($"Ошибка удаления файла {file}");
                    else
                        this.LogMessage($"Файл {file} удалён");
                }
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Удаление файлов на ПК из папки выбранной для генерации
        /// </summary>
        public bool DeleteFileOnPC()
        {
            try
            {
                this.DeleteFileOnPC(this.PathProgramm);
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Запуск события по окончанию выгрузки файлов
        /// </summary>
        private void OnLoadedFile() => this.LoadedFilesEvent?.Invoke(this, null);

        #region Asunc

        /// <summary>
        /// Переодическое выполнение задачи
        /// </summary>
        /// <param name="action">Выполняемое действие</param>
        /// <param name="period">Промежуток между выполнением, мс</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task PeriodicTask(Action action, TimeSpan period, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(period, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                    action();
            }
        }

        /// <summary>
        /// Переодическое выполнение задачи
        /// </summary>
        /// <param name="action">Выполняемое действие</param>
        /// <param name="period">Промежуток между выполнением, мс</param>
        /// <returns></returns>
        public async Task PeriodicTask(Action action, TimeSpan period) => await this.PeriodicTask(action, period, CancellationToken.None);

        /// <summary>
        /// Выборка файлов робота и сборка дерева
        /// </summary>
        /// <param name="data"></param>
        /// <param name="node"></param>
        /// <param name="index"></param>
        public async Task GetFilesAsync()
        {
            await Task.Run(() =>
            {
                this.LoadFilesTask = new Task(() => { this.LoadFiles(); });
                this.LoadFilesTask.Start();
                this.OnChangeProperty(nameof(this.IsLoadFiles));
                this.LoadFilesTask.Wait();
                this.Files.Children = new ObservableCollection<Controls.IFile>(this.FilesCollection);
                this.OnChangeProperty();
                this.OnLoadedFile();
            });
        }

        #endregion

        #endregion

        #region Implementations of IDisposable

        ~Robot() => Dispose(false);

        public void Dispose() => this.Dispose(true);

        public void Dispose(bool disposing)
        {
            try
            {
                if (this._disposed)
                    return;

                if (disposing)
                {
                    this.CloseConnection();
                }
                this._disposed = true;
                GC.SuppressFinalize(this);
            }
            catch(Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        #endregion
    }
}

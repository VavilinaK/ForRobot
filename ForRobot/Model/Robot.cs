using System;
using System.IO;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;

//using System.Collections.Generic;
using System.Linq;
//using System.Text;

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
        //private bool Disposed => _disposed != 0;

        private JsonRpcConnection _connection;

        private string _json;
        private string _pathProgram;
        private string _pathControllerFolder = "KRC:\\R1\\Program\\";
        private string _host;
        private string _pro_state;
        private string _processState = "Нет соединения";
        
        private int _port;
        private int _timeout_milliseconds;

        private decimal _voltage;
        private decimal _current;
        private decimal _wire_feed;
        private decimal _m1;
        private decimal _tracking;

        private CancellationTokenSource _cancelTokenSource { get; set; } = new CancellationTokenSource();

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
        public string Json
        {
            get => string.IsNullOrWhiteSpace(_json) == true ? _json = JsonSerializer.Serialize<Robot>(this, options) : _json;
            set => _json = value;
        }

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
                Json = JsonSerializer.Serialize<Robot>(this, options);
                if (!string.IsNullOrWhiteSpace(this._pathProgram))
                    this.ChangeRobot?.Invoke(this, null);
            }
        }

        [JsonPropertyName("pathControllerFolder")]
        /// <summary>
        /// Путь к папке на контроллере
        /// </summary>
        public string PathControllerFolder
        {
            get => this._pathControllerFolder;
            set
            {
                Set(ref this._pathControllerFolder, value);
                Json = JsonSerializer.Serialize<Robot>(this, options);
                if (!string.IsNullOrWhiteSpace(this._pathControllerFolder))
                    this.ChangeRobot?.Invoke(this, null);
            }
        }

        [JsonPropertyName("host")]
        public string Host
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._host))
                    return "0.0.0.0";
                else
                    return this._host;
            }
            set
            {
                this._host = value;
                Json = JsonSerializer.Serialize<Robot>(this, options);
                if (!int.Equals(this._timeout_milliseconds, 0) && !int.Equals(this.Port, 0) && (!string.IsNullOrWhiteSpace(this._host) || !Equals(this._host, "0.0.0.0")))
                {
                    this.ChangeRobot?.Invoke(this, null);
                    this.OpenConnection(this._timeout_milliseconds);
                }
            }
        }

        [JsonPropertyName("port")]
        public int Port
        {
            get => this._port;
            set
            {
                this._port = value;
                Json = JsonSerializer.Serialize<Robot>(this, options);
                if (!int.Equals(this._timeout_milliseconds, 0) && !int.Equals(this._port, 0) && (!string.IsNullOrWhiteSpace(this.Host) || !Equals(this.Host, "0.0.0.0")))
                {
                    this.ChangeRobot?.Invoke(this, null);
                    this.OpenConnection(this._timeout_milliseconds);
                }
            }
        }

        [JsonIgnore]
        /// <summary>
        /// Соединение робота
        /// </summary>
        public JsonRpcConnection Connection
        {
            get
            {
                if(object.Equals(this._connection,null) && (!string.IsNullOrWhiteSpace(this.Host) || !Equals("0.0.0.0", this.Host)) && !int.Equals(this.Port, 0))
                    this._connection = new JsonRpcConnection(this.Host, this.Port);

                return this._connection;
            }
            set => this._connection = value;
        }

        [JsonIgnore]
        public bool IsConnection { get => Equals(this.Connection, null) ? false : this.Connection.IsConnected; }

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

        [JsonIgnore]
        public string Pro_State { get => this._pro_state; set => Set(ref this._pro_state, value); }

        [JsonIgnore]
        /// <summary>
        /// Статус процесса
        /// </summary>
        public string ProcessState { get => this._processState; set => Set(ref this._processState, value); }

        [JsonIgnore]
        /// <summary>
        /// Имя процесса
        /// </summary>
        public string ProcessName { get => this.Connection.Pro_Name(); }

        [JsonIgnore]
        public decimal Voltage
        {
            get => this._voltage;
            set
            {
                this._voltage = value;
                //RaisePropertyChanged("Voltage");
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
                //RaisePropertyChanged("Current");
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
                //RaisePropertyChanged("WireFeed");
            }
        }

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

        #endregion

        #region Constructs

        public Robot() { }

        public Robot(string hostname, int port)
        {
            this.Host = hostname;
            this.Port = port;
            //if (string.IsNullOrWhiteSpace(hostname))
            //{
            //    throw new ArgumentNullException("hostname");
            //}

            //if (int.Equals(port, 0))
            //{
            //    throw new ArgumentNullException("port");
            //}
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

        private async Task ProStateTimeChack(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var delay = Task.Delay(1000);
                var state = this.Connection.Process_State();
                //var @in = this.Connection.In();

                await Task.WhenAll(new Task[] { delay, state });
                this.Pro_State = state.Result;
                switch (this.Pro_State)
                {
                    case "#P_FREE":
                        //LogMessage($"{this.Connection.Host}:{this.Connection.Port} не выбрана программа");
                        ProcessState = "Программа не выбрана";
                        break;

                    case "#P_RESET":
                        //LogMessage($"{this.Connection.Host}:{this.Connection.Port} программа выбрана, но не запущена/сброшена");
                        ProcessState = $"Выбрана программа {this.Connection.Pro_Name()}";
                        break;

                    case "#P_ACTIVE":
                        //LogMessage($"{this.Connection.Host}:{this.Connection.Port} программа запущена");
                        ProcessState = $"Запущена программа {this.Connection.Pro_Name()}";
                        break;

                    case "#P_STOP":
                        //LogMessage($"{this.Connection.Host}:{this.Connection.Port} программа запущена, но остановлена");
                        ProcessState = $"Программа {this.Connection.Pro_Name()} остановлена";
                        break;

                    case "#P_END":
                        //LogMessage($"{this.Connection.Host}:{this.Connection.Port} программа завершила работу");
                        ProcessState = $"Программа {this.Connection.Pro_Name()} завершена";
                        break;
                }

                //this.ConvertToTelegraf(@in.Result.ToArray());
            }
        }

        private async Task WeldTimeChack(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var delay = Task.Delay(2000);
                var @in = this.Connection.In();
                await Task.WhenAll(new Task[] { delay, @in });
                this.ConvertToTelegraf(@in.Result.ToArray());
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

        public void OpenConnection(int timeout_milliseconds)
        {
            this._timeout_milliseconds = timeout_milliseconds;
            try
            {
                if (!int.Equals(this.Port, 0) && (!string.IsNullOrWhiteSpace(this.Host) || !Equals(this.Host, "0.0.0.0")))
                {
                    this.Connection = new JsonRpcConnection(this.Host, this.Port);
                    this.Connection.Log += this.Log;
                    this.Connection.LogError += this.LogError;

                    Thread thread = new Thread(new ThreadStart(BeginConnect))
                    {
                        IsBackground = true // Закроется даже при неудачном подключении.
                    };
                    thread.Start();
                    thread.Join(this._timeout_milliseconds);

                    if (this.Connection.IsConnected)
                    {
                        //Task.Run(async () => await this.ProStateTimeChack(_cancelTokenSource.Token));
                        //Task.Run(async () => await this.WeldTimeChack(_cancelTokenSource.Token));
                    }
                    else
                        throw new TimeoutException("TcpClient соединение прервано");
                }
            }
            catch(Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        protected void BeginConnect() => this.Connection.Open();

        /// <summary>
        /// Запуск программы
        /// </summary>
        /// <returns></returns>
        public void Run()
        {
            try
            {
                this._cancelTokenSource.Cancel();
                if ((this.Pro_State == "#P_RESET" && MessageBox.Show($"Запустить программу {ProcessName}?", $"Запуск программы", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK) || this.Pro_State == "#P_STOP")
                {
                    if (!Task.Run<bool>(async () => await this.Connection.Run()).Result)
                        new Exception($"Ошибка запуска программы {ProcessName}");
                    else
                        this.LogMessage($"Программа {ProcessName} запущена");

                    this._cancelTokenSource = new CancellationTokenSource();
                    Task.Run(async () => await this.ProStateTimeChack(this._cancelTokenSource.Token));
                    Task.Run(async () => await this.WeldTimeChack(this._cancelTokenSource.Token));

                    while ((!string.Equals(this.Pro_State, "#P_END")))
                    {
                        if (string.Equals(this.Pro_State, "#P_STOP"))
                        {
                            this.LogMessage($"Программа {ProcessState} остановлена");
                            return;
                        }
                        System.Threading.Thread.Sleep(1_000);
                    }

                    if (string.Equals(this.Pro_State, "#P_END"))
                    {
                        this.LogMessage($"Программа {ProcessName} завершена");
                    }
                }

                //if(this.Pro_State == "#P_END" && MessageBox.Show($"Перезапустить программу {ProcessName}?", $"Перезапуск программы", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                //{

                //}
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
                if (string.Equals(this.Pro_State, "#P_ACTIVE"))
                {
                    if (!Task.Run<bool>(async () => await this.Connection.Pause()).Result)
                        new Exception($"Ошибка остановки программы {ProcessName}");
                    else
                        this.LogMessage($"Программа {ProcessName} остановлена");
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
        public void Stop()
        {
            try
            {
                if ((string.Equals(this.Pro_State, "#P_RESET") || (string.Equals(this.Pro_State, "#P_ACTIVE") || string.Equals(this.Pro_State, "#P_STOP") || string.Equals(this.Pro_State, "#P_END"))))
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
        /// <param name="endPath">Конечный путь</param>
        /// <param name="fileName">Директива файла</param>
        public void Copy(string endPath, string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(this.PathControllerFolder))
                {
                    this.LogErrorMessage("Нет пути на коталог на контроллере");
                    MessageBox.Show("Укажите путь к каталогу на контроллере", "Остановка", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                switch (this.Connection.Pro_State())
                {
                    //case "#P_FREE":
                        ////foreach(var file in Directory.GetFiles(this.PathProgramm, "*.dat"))
                        ////{
                        ////    if (!Task.Run<bool>(async () =>
                        ////        await this.Connection.Copy(file, this.PathControllerFolder)).Result)
                        ////        new Exception($"Ошибка копирования файла {file} в {endPath}");
                        ////    else
                        ////        this.LogMessage($"Файл {file} скопирован ");
                        ////}
                        ////break;

                        //if (!Task.Run<bool>(async () =>
                        //    await this.Connection.Copy(Path.Combine(this.PathControllerFolder, string.Join("", fileName, ".src")), Path.Combine(endPath, fileName))).Result)
                        //    new Exception($"Ошибка копирования файла {string.Join("", fileName, ".src")} из {this.PathControllerFolder} в {endPath}");
                        //else
                        //    this.LogMessage($"Файл {string.Join("", fileName, ".src")} скопирован");

                        //if (!Task.Run<bool>(async () =>
                        //    await this.Connection.Copy(Path.Combine(this.PathControllerFolder, string.Join("", fileName, ".dat")), Path.Combine(endPath, fileName))).Result)
                        //    new Exception($"Ошибка копирования файла {string.Join("", fileName, ".dat")} из {this.PathControllerFolder} в {endPath}");
                        //else
                        //    this.LogMessage($"Файл {string.Join("", fileName, ".dat")} скопирован");
                        //break;

                        //case "#P_RESET":
                        //case "#P_END":
                        //    if (!Task.Run<bool>(async () => await this.Connection.SelectCancel()).Result)
                        //    {
                        //        this.LogErrorMessage("Не удаётся отменить выбор программы");
                        //        return;
                        //    }
                        //    else
                        //        this.LogMessage("Текущий выбор отменён");

                        //    System.Threading.Thread.Sleep(1000); // Костыль).

                        //    if (!Task.Run<bool>(async () =>
                        //        await this.Connection.Copy(Path.Combine(this.PathControllerFolder, string.Join("", fileName, ".src")), Path.Combine(endPath, fileName))).Result)
                        //        new Exception($"Ошибка копирования файла {string.Join("", fileName, ".src")} из {this.PathControllerFolder} в {endPath}");
                        //    else
                        //        this.LogMessage($"Файл {string.Join("", fileName, ".src")} скопирован");

                        //    if (!Task.Run<bool>(async () => 
                        //        await this.Connection.Copy(Path.Combine(this.PathControllerFolder, string.Join("", fileName, ".dat")), Path.Combine(endPath, fileName))).Result)
                        //        new Exception($"Ошибка копирования файла {string.Join("", fileName, ".dat")} из {this.PathControllerFolder} в {endPath}");
                        //    else
                        //        this.LogMessage($"Файл {string.Join("", fileName, ".dat")} скопирован");
                        //    break;

                        //case "#P_ACTIVE":
                        //case "#P_STOP":
                        //    this.LogMessage("Отмена копирования: уже запущен процесс!");
                        //    return;
                }
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        /// <summary>
        /// Копирование файла на контроллер
        /// </summary>
        /// <param name="filesPath">Директива файлов</param>
        public void CopyToController()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(this.PathProgramm) || !Directory.Exists(this.PathProgramm))
                {
                    this.LogErrorMessage($"Путь {this.PathProgramm} не существует или указан неверно");
                    //MessageBox.Show($"Укажите путь к каталогу программы робота с соединением {this.Host}:{this.Port}", "Остановка", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                if (string.IsNullOrWhiteSpace(this.PathControllerFolder))
                {
                    this.LogErrorMessage("Не указан путь для контролера");
                    MessageBox.Show("Укажите путь к каталогу на контроллере", "Остановка", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                switch (this.Connection.Pro_State())
                {
                    case "#P_FREE":
                        foreach (var file in Directory.GetFiles(this.PathProgramm, "*.dat"))
                        {
                            Task.Run(async () => await this.Connection.CopyMem2File(file, Path.Combine(this.PathControllerFolder, new FileInfo(file).Name))).Wait();

                            if (!Task.Run<bool>(async () => await this.Connection.CopyMem2File(file, Path.Combine(this.PathControllerFolder, new FileInfo(file).Name))).Result)
                                new Exception($"Ошибка копирования содержимого файла {file} в {Path.Combine(this.PathControllerFolder, new FileInfo(file).Name)} конроллера");
                            else
                                this.LogMessage($"Содержимое файла {file} не скопировано на контроллер");
                        }
                        break;

                    case "#P_RESET":
                    case "#P_END":
                        break;

                    case "#P_ACTIVE":
                    case "#P_STOP":
                        break;
                }

                //if (string.IsNullOrWhiteSpace(this.PathControllerFolder))
                //{
                //    this.LogErrorMessage("Нет пути на коталог на контроллере");
                //    MessageBox.Show("Укажите путь к каталогу на контроллере", "Остановка", MessageBoxButton.OK, MessageBoxImage.Stop);
                //    return;
                //}

                //if (!Task.Run<bool>(async () => await this.Connection.CopyMem2File(Path.Combine(yourePath, string.Join("", fileName, ".src")),
                //         Path.Combine(this.PathControllerFolder, string.Join("", fileName, ".src")))).Result)
                //    new Exception($"Ошибка копирования содержимого файла {string.Join("", fileName, ".src")} " +
                //                  $"в {Path.Combine(this.PathControllerFolder, string.Join("", fileName, ".src"))} контроллера");
                //else
                //    this.LogMessage($"Содержимое файла {string.Join("", fileName, ".src")} скопировано на контроллер");

                //if (!Task.Run<bool>(async () => await this.Connection.CopyMem2File(Path.Combine(yourePath, string.Join("", fileName, ".dat")),
                //                 Path.Combine(this.PathControllerFolder, string.Join("", fileName, ".dat")))).Result)
                //    new Exception($"Ошибка копирования содержимого файла {string.Join("", fileName, ".dat")} " +
                //                  $"в {Path.Combine(this.PathControllerFolder, string.Join("", fileName, ".dat"))} конроллера");
                //else
                //    this.LogMessage($"Содержимое файла {string.Join("", fileName, ".dat")} скопировано на контроллер");
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        /// <summary>
        /// Выбор программы
        /// </summary>
        /// <returns></returns>
        public void SelectProgramm(string filePath)
        {
            try
            {
                switch (this.Pro_State)
                {
                    case "#P_FREE":
                        if (!Task.Run<bool>(async () => await this.Connection.Select(filePath)).Result)
                            new Exception("Ошибка выбора файла " + filePath);
                        else
                            this.LogMessage($"Файл программы {filePath} выбран");
                        break;

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

                        //if (!Task.Run<bool>(async () => await this.Connection.Select(Path.Combine("KRC:\\R1\\Program\\", string.Join("", fileName, ".src")))).Result)
                        //    new Exception("Ошибка выбора файла " + Path.Combine("KRC:\\R1\\Program\\", string.Join("", fileName, ".src")));
                        //else
                        //    this.LogMessage($"Файл программы {string.Join("", fileName, ".src")} выбран");

                        if (!Task.Run<bool>(async () => await this.Connection.Select(filePath)).Result)
                            new Exception("Ошибка выбора файла " + filePath);
                        else
                            this.LogMessage($"Файл программы {filePath} выбран");
                        break;

                    case "#P_ACTIVE":
                    case "#P_STOP":
                        this.LogMessage("Отмена выбора: уже запущен процесс!");
                        break;
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
                this._cancelTokenSource.Cancel();
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

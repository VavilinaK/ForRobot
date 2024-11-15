using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Sockets;

using Newtonsoft.Json.Linq;

using StreamJsonRpc;

namespace ForRobot.Libr.Client
{
    public class JsonRpcConnection : IDisposable
    {
        #region Constants

        //private const string AnnouncementEnd = "}";
        //private const string AnnouncementEndAlternative = "}\n";
        private const int DefaultPort = 0000;
        private const string DefaultHost = "0.0.0.0";
        private const string OkResponse = "Ok";
        internal const string DefaulRoot = "KRC:\\";

        #endregion

        #region Private variables

        private volatile int _disposed;
        private bool Disposed => _disposed != 0;

        /// <summary>
        /// Используется классом для указания, что соединение установлено
        /// </summary>
        protected bool _isActive
        {
            get
            {
                bool actived = false;
                try
                {
                    if (this.Client == null || !this.Client.Connected)
                        return actived;
                }
                catch (Exception ex)
                {
                    throw new Exception("Не удалось определить состояние TCP-сокета", ex);
                }

                if (this._key())
                    actived = true;

                return actived;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Событие открытия соединения
        /// </summary>
        public event EventHandler Connected;
        /// <summary>
        /// Событие прерывания соединения
        /// </summary>
        public event EventHandler Aborted;
        /// <summary>
        /// Событие закрытие соединения
        /// </summary>
        public event EventHandler Disconnected;
         
        /// <summary>
        /// Событие записи лога
        /// </summary>
        public event EventHandler<LogEventArgs> Log;
        /// <summary>
        /// Событие логирования ошибки
        /// </summary>
        public event EventHandler<LogErrorEventArgs> LogError;

        #endregion

        #region Public variables
        
        public string Host { get; set; }

        public int Port { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        public TcpClient Client { get; set; }

        /// <summary>
        /// Управляет подключением JSON-RPC к серверу через Stream
        /// </summary>
        private JsonRpc JsonRpc { get; set; }

        #endregion

        #region Constructors

        public JsonRpcConnection() : this(null, 0) { }

        public JsonRpcConnection(string hostname = DefaultHost, int port = DefaultPort)
        {
            this.Host = hostname;
            this.Port = port;
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Запрос ключа с сервера
        /// </summary>
        /// <returns></returns>
        private bool _key()
        {
            Task<string> task = Task.Run(() => this.JsonRpc.InvokeAsync<string>("auth", "My_example_KEY"));
            if (task.Result == OkResponse)
                return true;
            return false;
        }

        /// <summary>
        /// Срабатывание события подключения
        /// </summary>
        private void _onConnected()
        {
            if (this.Connected == null)
            {
                return;
            }

            this.Connected(this, null);
        }

        /// <summary>
        /// Срабатывание события прерывания подключения
        /// </summary>
        private void _onAborted()
        {
            if (this.Aborted == null)
            {
                return;
            }

            this.Aborted(this, null);
        }

        /// <summary>
        /// Срабатывание события отключения соединения
        /// </summary>
        private void _onDisconnected()
        {
            if (this.Disconnected == null)
                return;

            this.Disconnected(this, null);
        }

        /// <summary>
        /// Вызов события записи в журнал логгирования
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _onLog(object sender, LogEventArgs e) => this.Log?.Invoke(this, e);

        /// <summary>
        /// Вызов события записи ошибки в журнал логгирования
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _onLogError(object sender, LogErrorEventArgs e) => this.LogError?.Invoke(this, e);

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

        #region Public function

        /// <summary>
        /// Открытие соединения
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            bool opened = false;
            try
            {
                this.Client = new TcpClient(this.Host, this.Port)
                {
                    ReceiveTimeout = 3000,
                    SendTimeout = 3000
                };
                this.JsonRpc = new JsonRpc(new NewLineDelimitedMessageHandler(Client.GetStream(), Client.GetStream(), new JsonMessageFormatter()));
                this.JsonRpc.StartListening();
                this.JsonRpc.Disconnected += (sender, e) =>
                {
                    if (this._disposed == 0)
                        this._onAborted();
                    else
                        this._onDisconnected();
                };

                if (!this._isActive)
                {
                    this.Close();
                    return false;
                }

                this._onConnected();
                opened = true;
            }
            catch (SocketException ex)
            {
                throw new Exception($"Не удалось открыть соединение с сервером", ex);
            }
            return opened;
        }

        /// <summary>
        /// Закрытие соединения
        /// </summary>
        public bool Close()
        {
            bool closed = false;
            if (this.Client.Connected)
            {
                try
                {
                    this.Client.Client.Disconnect(false);
                    closed = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Не удалось отключиться от TCP-сокета", ex);
                }
            }
            return closed;
        }

        #region Asyn

        /// <summary>
        /// Запрос статуса процесса
        /// </summary>
        /// <returns></returns>
        public async Task<string> Process_State() => await this.JsonRpc.InvokeAsync<string>("Var_ShowVar", "$PRO_STATE");

        /// <summary>
        /// Название программы
        /// </summary>
        /// <returns></returns>
        public async Task<string> Pro_Name() => await this.JsonRpc.InvokeAsync<string>("Var_ShowVar", "$PRO_NAME[]");

        /// <summary>
        /// Запрос состояния входов
        /// </summary>
        /// <returns></returns>
        public async Task<string> In() => await this.JsonRpc.InvokeAsync<string>("Var_ShowVar", "$IN[]");

        /// <summary>
        /// Копирование файла в дерективу робота
        /// </summary>
        /// <param name="sFilePath">Директория файла</param>
        /// <param name="sNewPath">Новый путь для файла</param>
        /// <returns></returns>
        public async Task<bool> Copy(string sFilePath, string sNewPath)
        {
            this.LogMessage($"Копирование файла программы в директорию робота {sNewPath} . . .");
            object[] args = { sFilePath, sNewPath, 64 };
            string result = await this.JsonRpc.InvokeAsync<string>("File_Copy", args);
            if (result == OkResponse)
                return true;
            return false;
        }

        /// <summary>
        /// Копирование содержание файла в дерективу робота
        /// </summary>
        /// <param name="sFilePath">Директория файла</param>
        /// <param name="sControllerPath">Путь для файла на роботе</param>
        /// <returns></returns>
        public async Task<bool> CopyMem2File(string sFilePath, string sControllerPath)
        {
            this.LogMessage($"Копирование содержание файла {sFilePath} в {sControllerPath} . . .");

            if (!File.Exists(Path.Combine(sFilePath)))
                this.LogErrorMessage($"Не существует файла {sFilePath}");

            string content;
            using (StreamReader reader = new StreamReader(sFilePath))
            {
                content = await reader.ReadToEndAsync();
            }

            object[] args = { content, sControllerPath, 64 };
            string result = await this.JsonRpc.InvokeAsync<string>("File_CopyMem2File", args);

            if (result == OkResponse)
                return true;
            return false;
        }

        /// <summary>
        /// Выбор файла программы
        /// </summary>
        /// <param name="sFilePath">Директория файла программы</param>
        /// <returns></returns>
        public async Task<bool> Select(string sFilePath)
        {
            this.LogMessage($"Выбор программы {sFilePath} . . .");

            string result = await this.JsonRpc.InvokeAsync<string>("Select_Select", sFilePath);

            return result == OkResponse;
        }

        /// <summary>
        /// Отмена выбора программы
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SelectCancel()
        {
            this.LogMessage($"Отмена выбора текущего файла . . .");

            string result = await this.JsonRpc.InvokeAsync<string>("Select_Cancel");

            return result == OkResponse;
        }

        /// <summary>
        /// Удаление файла
        /// </summary>
        /// <param name="sFilePath">Директория файла</param>
        /// <returns></returns>
        public async Task<bool> Delet(string sFilePath)
        {
            this.LogMessage($"Удаление файла {sFilePath} . . .");

            string result = await this.JsonRpc.InvokeAsync<string>("File_Delete", sFilePath);

            return result == OkResponse;
        }

        /// <summary>
        /// Запуск уже выбранной программы
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Start()
        {
            this.LogMessage($"Запуск текущей программы . . .");

            string result = await this.JsonRpc.InvokeAsync<string>("Select_Start");

            return result == OkResponse;
        }

        /// <summary>
        /// Выбор и запуск программы
        /// </summary>
        /// <param name="sFilePath">Директория файла</param>
        /// <returns></returns>
        public async Task<bool> Run(string sFilePath)
        {
            this.LogMessage($"Запуск программы {sFilePath} . . .");

            string result = await this.JsonRpc.InvokeAsync<string>("Select_Run", sFilePath);

            return result == OkResponse;
        }

        /// <summary>
        /// Остановка программы
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Pause()
        {
            this.LogMessage($"Остановка текущей программы . . .");

            object[] args = { 1 };

            string result = await this.JsonRpc.InvokeAsync<string>("Select_Stop", args);

            return result == OkResponse;
        }

        /// <summary>
        /// Вывод списка файлов в папке
        /// </summary>
        /// <param name="sFolderPath">Директория папки для запроса</param>
        /// <returns></returns>
        public async Task<Dictionary<String, String>> File_NameList(string sFolderPath = DefaulRoot)
        {
            Dictionary<String, String> result = new Dictionary<string, string>();
            try
            {
                object[] args = { sFolderPath, 511, 127 };
                result = await this.JsonRpc.InvokeAsync<Dictionary<String, String>>("File_NameList", args);
            }
            catch (Exception e)
            {
                App.Current.Logger.Error(e.Message, e);
            }
            return result;
        }

        #endregion

        #endregion

        #region Implementations of IDisposable

        ~JsonRpcConnection() => Dispose(false);

        public void Dispose() => this.Dispose(true);

        public void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            {
                if (disposing)
                {
                    try
                    {
                        lock (this.Client.Client)
                        {
                            this.Client.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Не удалось закрыть TCP-клиент", ex);
                    }
                    finally
                    {
                        this._disposed = 1;
                    }
                    GC.SuppressFinalize(this);
                }
            }
        }

        #endregion
    }
}

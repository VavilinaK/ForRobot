using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

using Newtonsoft.Json.Linq;
using StreamJsonRpc;

//using Microsoft.VisualStudio.Threading;
using System.Collections.Generic;

namespace ForRobot.Libr.Client
{
    public class JsonRpcConnection : IDisposable
    {
        #region Constants

        //private const string AnnouncementEnd = "}";
        //private const string AnnouncementEndAlternative = "}\n";
        private const string OkResponse = "Ok";

        #endregion

        #region Private variables

        private volatile int _disposed;
        private bool Disposed => _disposed != 0;

        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private NewLineDelimitedMessageHandler _message_handler;
        private JsonRpc _jsonRpc;

        /// <summary>
        /// Используется классом для указания, что соединение установлено
        /// </summary>
        protected bool _isActive
        {
            get
            {
                try
                {
                    if (this.SocketConnection == null || !this.SocketConnection.Connected)
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    this.LogErrorMessage($"{this.Host}:{this.Port} Не удалось определить состояние TCP-сокета", ex);
                    return false;
                }

                if (!Key())
                {
                    return false;
                }
                return true;
            }
        }

        #endregion

        #region Events

        public event EventHandler Connected;
        public event EventHandler Aborted;
        public event EventHandler Disconnected;
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

        //public bool IsConnected => Client?.Connected ?? false;

        public string Host { get; set; }

        public int Port { get; set; }

        public bool IsConnected { get => !Equals(this.Client, null) ? SocketConnection.Connected : false; }

        public TcpClient Client
        {
            get
            {
                //if (TcpClient.Equals(this._tcpClient, null) || !this._tcpClient.Connected)
                //{
                //    this._tcpClient = new TcpClient(Host, Port)
                //    {
                //        ReceiveTimeout = 3000,
                //        SendTimeout = 3000
                //    };
                //    this.onConnected();
                //    return this._tcpClient;
                //}
                //else
                    return this._tcpClient;
            }
            set => this._tcpClient = value;
        }

        public NetworkStream Stream
        {
            get
            {
                //if (NetworkStream.Equals(this._stream, null) || !this._stream.CanWrite || !this._stream.CanWrite)
                //{
                //    this._stream = _tcpClient.GetStream();
                //    this.onConnected();
                //    return this._stream;
                //}
                //else
                    return this._stream;
            }
            set => this._stream = value;
        }

        public NewLineDelimitedMessageHandler MessageHandler { get => this._message_handler; set => this._message_handler = value; }

        public JsonRpc JsonRpc
        {
            get => this._jsonRpc;
            set
            {
                this._jsonRpc = value;
                //this._jsonRpc.AddLocalRpcMethod("auth", new Func<string>((a) =>))
            }
        }

        public Socket SocketConnection { get => this.Client.Client; set => this.Client.Client = value; }

        #endregion

        #region Constructor

        public JsonRpcConnection() : this(null, 0) { }

        public JsonRpcConnection(string hostname, int port)
        {
            if (string.IsNullOrWhiteSpace(hostname))
            {
                throw new ArgumentNullException("hostname");
            }

            if (int.Equals(port, 0))
            {
                throw new ArgumentNullException("port");
            }

            this.Host = hostname;
            this.Port = port;
        }

        #endregion

        #region Private functions

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
            {
                return;
            }

            this.Disconnected(this, null);
        }

        private void _onAnnouncement(string data)
        {
            JObject announcement = JObject.Parse(data);
            JValue param = announcement["result"] as JValue;

            if (param == null || string.CompareOrdinal((string)param, OkResponse) != 0)
            {
                return;
            }
            //string type = (string)param["message"];
        }

        //private void receiveAnnouncements(IAsyncResult result)
        //{
        //    if (this.Disposed)
        //    {
        //        return;
        //    }

        //    lock (this.SocketConnection)
        //    {
        //        SocketStateObject state = result.AsyncState as SocketStateObject;

        //        if (state == null || this.SocketConnection == null || !this.SocketConnection.Connected)
        //        {
        //            return;
        //        }

        //        int read = 0;
        //        try
        //        {
        //            read = this.SocketConnection.EndReceive(result);
        //        }
        //        catch (Exception ex)
        //        {
        //            this.LogErrorMessage("Не удалось прочитать TCP-сокет", ex);
        //            this.Close();
        //            this.onAborted();
        //        }

        //        if (read > 0)
        //        {
        //            state.Builder.Append(Encoding.UTF8.GetString(state.Buffer, 0, read));
        //            this.receive(state);
        //        }

        //        string data = state.Builder.ToString();

        //        if (data.Length > 0 && data.Contains(AnnouncementEnd) || data.Contains(AnnouncementEndAlternative))
        //        {
        //            this.LogMessage("Получено объявление JSON RPC: " + data);

        //            int pos = data.IndexOf(AnnouncementEnd);
        //            if (pos < 0)
        //            {
        //                pos = data.IndexOf(AnnouncementEndAlternative) + AnnouncementEndAlternative.Length;
        //            }
        //            else
        //            {
        //                pos += AnnouncementEnd.Length;
        //            }
        //            state.Builder.Remove(0, pos);
        //            this.onAnnouncement(data.Substring(0, pos));
        //        }

        //        this.receive(state);
        //    }
        //}

        //private void receive(SocketStateObject state)
        //{
        //    if (state == null || this.SocketConnection == null || !this.SocketConnection.Connected)
        //    {
        //        return;
        //    }

        //    try
        //    {
        //        this.SocketConnection.BeginReceive(state.Buffer, 0, SocketStateObject.BufferSize,
        //            0, new AsyncCallback(this.receiveAnnouncements), state);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.LogErrorMessage("Не удалось начать прием из TCP-сокета", ex);
        //        this.Close();
        //        this.onAborted();
        //    }
        //}

        private void _onLog(object sender, LogEventArgs e)
        {
            this.Log?.Invoke(this, e);
        }

        private void _onLogError(object sender, LogErrorEventArgs e)
        {
            this.LogError?.Invoke(this, e);
        }

        #region Asyn functions

        /// <summary>
        /// Запрос ключа с сервера
        /// </summary>
        /// <returns></returns>
        private bool Key()
        {
            Task<string> task = Task.Run(() => this.JsonRpc.InvokeAsync<string>("auth", "My_example_KEY"));
            if (task.Result == OkResponse)
                return true;

            return false;

            //return Task.Run(() => this.JsonRpc.InvokeAsync<string>("auth", "My_example_KEY"));
        }

        #endregion

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
        /// Закрытие соединения
        /// </summary>
        public void Close()
        {
            this.LogMessage($"Закрытие соединения . . .");

            try
            {
                if (this.SocketConnection != null && this.SocketConnection.Connected)
                {
                    this.SocketConnection.Disconnect(false);
                    this._onDisconnected();
                    this.LogMessage($"Соединение закрыто");
                }
            }
            catch (Exception ex)
            {
                this.LogErrorMessage("Не удалось отключиться от TCP-сокета", ex);
            }
        }

        /// <summary>
        /// Открытие соединения
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            this.LogMessage($"Открытие соединения с сервером . . .");
            try
            {
                this._tcpClient = new TcpClient(this.Host, this.Port)
                {
                    ReceiveTimeout = 3000,
                    SendTimeout = 3000
                };
                //this._tcpClient.Connect(Host, Port);
                this.Stream = _tcpClient.GetStream();
                this.MessageHandler = new NewLineDelimitedMessageHandler(_stream, _stream, new JsonMessageFormatter());
                this.JsonRpc = new JsonRpc(this._message_handler);
                this.JsonRpc.StartListening();

                if (!this._isActive)
                {
                    this.Close();
                    return false;
                }

                this._onConnected();
                this.LogMessage($"Открыто соединение");
                //this.receive(new SocketStateObject());
            }
            catch (SocketException ex)
            {
                this.LogErrorMessage($"Не удалось открыть соединение с сервером", ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Запрос статуса процесса
        /// </summary>
        /// <returns></returns>
        public string Pro_State()
        {
            this.LogMessage($"Запрос статуса процесса . . .");
            Task<string> task = Task.Run(async () => await this.JsonRpc.InvokeAsync<string>("Var_ShowVar", "$PRO_STATE"));
            task.Wait();
            return task.Result;
        }

        #region Asyn

        /// <summary>
        /// Запрос статуса процесса
        /// </summary>
        /// <returns></returns>
        public async Task<string> Process_State()
        {
            return await this.JsonRpc.InvokeAsync<string>("Var_ShowVar", "$PRO_STATE");
        }

        /// <summary>
        /// Запрос состояния входов
        /// </summary>
        /// <returns></returns>
        public async Task<string> In()
        {
            return await this.JsonRpc.InvokeAsync<string>("Var_ShowVar", "$IN[]");
        }

        /// <summary>
        /// Копирование файла в дерективу робота
        /// </summary>
        public async Task<bool> Copy(string oldPath, string newPath)
        {
            this.LogMessage($"Копирование файла программы в директорию робота {newPath} . . .");

            object[] args = { oldPath, newPath, 64 };
            string result = await this.JsonRpc.InvokeAsync<string>("File_Copy", args);

            if (result == OkResponse)
                return true;
            return false;
        }

        /// <summary>
        /// Копирование содержание файла в дерективу робота
        /// </summary>
        public async Task<bool> CopyMem2File(string filePath, string controllerPath)
        {
            this.LogMessage($"Копирование содержание файла {filePath} в {controllerPath} . . .");

            if (!File.Exists(Path.Combine(filePath)))
                this.LogErrorMessage($"Не существует файла {filePath}");

            string content;
            using (StreamReader reader = new StreamReader(filePath))
            {
                content = await reader.ReadToEndAsync();
            }

            object[] args = { content, controllerPath, 64 };
            string result = await this.JsonRpc.InvokeAsync<string>("File_CopyMem2File", args);

            if (result == OkResponse)
                return true;
            return false;
        }

        /// <summary>
        /// Выбор файла программы
        /// </summary>
        /// <param name="path"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<bool> Select(string path)
        {
            this.LogMessage($"Выбор программы {path} . . .");

            string result = await this.JsonRpc.InvokeAsync<string>("Select_Select", path);

            if (result == OkResponse)
                return true;
            return false;
        }

        /// <summary>
        /// Отмена выбора программы
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SelectCancel()
        {
            this.LogMessage($"Отмена выбора текущего файла . . .");

            string result = await this.JsonRpc.InvokeAsync<string>("Select_Cancel");

            if (result == OkResponse)
                return true;
            return false;
        }

        /// <summary>
        /// Удаление файла программы из директивы робота
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Delet(string path)
        {
            this.LogMessage($"Удаление файла {path} . . .");

            string result = await this.JsonRpc.InvokeAsync<string>("File_Delete", path);

            if (result == OkResponse)
                return true;
            return false;
        }

        /// <summary>
        /// Запуск программы
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Run()
        {
            this.LogMessage($"Запуск текущей программы . . .");

            string result = await this.JsonRpc.InvokeAsync<string>("Select_Start");

            if (result == OkResponse)
                return true;

            return false;
        }

        /// <summary>
        /// Запуск программы
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Run(string path)
        {
            this.LogMessage($"Запуск программы {path} . . .");

            string result = await this.JsonRpc.InvokeAsync<string>("Select_Run", path);

            if (result == OkResponse)
                return true;

            return false;
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

            if (result == OkResponse)
                return true;

            return false;
        }

        /// <summary>
        /// Название программы
        /// </summary>
        /// <returns></returns>
        public string Pro_Name()
        {
            Task<string> task = Task.Run(async () => await this.JsonRpc.InvokeAsync<string>("Var_ShowVar", "$PRO_NAME[]"));
            task.Wait();
            return task.Result.Replace("\"", "");
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
                        lock (this.SocketConnection)
                        {
                            this.Close();
                            this.SocketConnection.Close();
                            this._stream.Close();
                            this._tcpClient.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        this.LogErrorMessage($"Не удалось закрыть TCP-сокет", ex);
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

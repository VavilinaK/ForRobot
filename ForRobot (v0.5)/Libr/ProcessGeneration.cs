using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using ForRobot.Model;

namespace ForRobot.Libr
{
    public class ProcessGeneration : IDisposable
    {
        #region Private variables

        private string _filePath;

        private bool disposed = false;

        #endregion

        #region Events

        /// <summary>
        /// Событие логирования действия
        /// </summary>
        public event EventHandler<LogEventArgs> Log;
        /// <summary>
        /// Событие логирования ошибки
        /// </summary>
        public event EventHandler<LogErrorEventArgs> LogError;
        /// <summary>
        /// Событие изменения статуса процесса на роботе
        /// </summary>
        public event EventHandler<Client.StatusEventArgs> Status;

        #endregion

        #region Internal variables

        /// <summary>
        /// Название сгенерированной программы
        /// </summary>
        internal string FileName { get; set; }

        /// <summary>
        /// Название программы-генератора
        /// </summary>
        internal string NameGenerator { get; set; }

        #endregion

        #region Public variables

        public bool ProccesEnd
        {
            get
            {
                bool res = false;

                if (File.Exists(Path.Combine(_filePath, string.Join("", FileName, ".src"))))
                {
                    this.LogMessage($"Файл {string.Join("", FileName, ".src")} сгенерирован");
                    res = true;
                }
                else
                    this.LogErrorMessage($"Файл {Path.Combine(_filePath, string.Join("", FileName, ".src"))} не найден");

                if (File.Exists(Path.Combine(_filePath, string.Join("", FileName, ".dat"))))
                {
                    this.LogMessage($"Файл {string.Join("", FileName, ".dat")} сгенерирован");
                    res = true;
                }
                else
                {
                    this.LogErrorMessage($"Файл {Path.Combine(_filePath, string.Join("", FileName, ".dat"))} не найден");
                    res = false;
                }

                return res;
            }
        }

        public string NowProgramm { get => this.Connection.Pro_Name(); }

        public Client.JsonRpcConnection Connection { get; private set; }

        #endregion

        #region Constructors

        public ProcessGeneration(string path) { _filePath = path; }

        //public ProcessGeneration(string path, Detal detal, Svarka svarka)
        //{
        //    _filePath = path;
        //    Start(detal, svarka);
        //}

        #endregion

        #region Internal functions

        internal void LogMessage(string message)
        {
            if (string.IsNullOrEmpty(message) || this.Log == null)
            {
                return;
            }

            this.Log(this, new LogEventArgs(message));
        }

        internal void LogErrorMessage(string message) => this.LogErrorMessage(message, null);

        internal void LogErrorMessage(string message, Exception exception)
        {
            if (string.IsNullOrEmpty(message) || this.LogError == null)
            {
                return;
            }

            this.LogError(this, new LogErrorEventArgs(message, exception));
        }

        internal void StatusMessage(string message)
        {
            if (string.IsNullOrEmpty(message) || this.Status == null)
            {
                return;
            }

            this.Status(this, new Client.StatusEventArgs(message));
        }

        #endregion

        #region Public functions

        public void Start() => this.Start(null, null);

        public void Start(Detal detal, Svarka svarka)
        {
            try
            {
                if (object.Equals(detal, null) || object.Equals(svarka, null))
                {
                    throw new ArgumentException();
                }

                if (DetalPropertiesAreNull(detal))
                {
                    //this.Dispose();
                    //return;
                    throw new Exception("Не заполнен ни один параметр детали");
                }

                if (SvarkaPropertiesAreNull(svarka))
                    throw new Exception("Не заполнены параметры сварки");

                string[] args = new string[]
                {
                //$"-dt {detal.DetalType}",
                $"-s {detal.SumReber}",
                $"-l {detal.Long}",
                $"-h {detal.Hight}",
                $"-w {detal.Wight}",
                $"-is {detal.IndentionStart}",
                $"-ie {detal.IndentionEnd}",
                $"-ds {detal.DissolutionStart}",
                $"-de {detal.DissolutionEnd}",
                $"-df {detal.DistanceToFirst}",
                $"-db {detal.DistanceBetween}",
                $"-tp {detal.ThicknessPlita}",
                $"-tr {detal.ThicknessRebro}",
                $"-ss {detal.SearchOffsetStart}",
                $"-se {detal.SearchOffsetEnd}",
                $"-ws {svarka.WildingSpead}",
                $"-pn {svarka.ProgramNom}"
                };

                this.Start(detal, args);
            }
            catch(Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
                this.Dispose();
                return;
            }
        }

        /// <summary>
        /// Начало процесса генерации программы
        /// </summary>
        /// <param name="detal">Деталь</param>
        /// <param name="args">Аргументы командной строки</param>
        public void Start(object detal, string[] args)
        {
            try
            {
                Process process = new Process();
                switch (detal)
                {
                    case Plita plita:
                        this.LogMessage("Начат процесс генерации программы для плиты с рёбрами . . .");
                        process.StartInfo.FileName = Path.Combine(_filePath, NameGenerator);
                        process.StartInfo.Arguments = String.Join(" ", args);
                        break;

                    case PlitaStringer stringer:
                        this.LogMessage("Начат процесс генерации программы для плиты со стрингером . . .");
                        break;

                    case PlitaTreygolnik treygolnik:
                        this.LogMessage("Начат процесс генерации программы для плиты треугольником . . .");
                        break;
                }
                if(!File.Exists(Path.Combine(_filePath, NameGenerator)))
                {
                    this.LogErrorMessage($"Не существует файла генератора {Path.Combine(_filePath, NameGenerator)}");
                    this.StatusMessage(Client.StatusProcess.ErrorGeneration);
                    this.Dispose();
                    return;
                }
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                this.StatusMessage(Client.StatusProcess.ErrorGeneration);
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        /// <summary>
        /// Открытие соеднение
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        public void OpenConnection(string hostname, int port)
        {
           this.Connection = new Client.JsonRpcConnection(hostname, port);
           this.Connection.Log += this.Log;
           this.Connection.LogError += this.LogError;
           this.Connection.Open();
        }

        /// <summary>
        /// Генерация программы и её выбор
        /// </summary>
        /// <returns></returns>
        public async Task Generation_Select(string pathOnControler)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(@pathOnControler))
                {
                    this.LogErrorMessage("Нет пути на коталог на контроллере");
                    MessageBox.Show("Укажите путь к каталогу на контроллере", "Остановка", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                if (!await this.Connection.CopyMem2File(Path.Combine(_filePath, string.Join("", FileName, ".src")),
                                         Path.Combine(pathOnControler, string.Join("", FileName, ".src"))))
                    new Exception($"Ошибка копирования содержимого файла {string.Join("", FileName, ".src")} " +
                                  $"в {Path.Combine(pathOnControler, string.Join("", FileName, ".src"))} контроллера");
                else
                    this.LogMessage($"Содержимое файла {string.Join("", FileName, ".src")} скопировано на контроллер");

                if (!await this.Connection.CopyMem2File(Path.Combine(_filePath, string.Join("", FileName, ".dat")),
                                 Path.Combine(pathOnControler, string.Join("", FileName, ".dat"))))
                    new Exception($"Ошибка копирования содержимого файла {string.Join("", FileName, ".dat")} " +
                                  $"в {Path.Combine(pathOnControler, string.Join("", FileName, ".dat"))} конроллера");
                else
                    this.LogMessage($"Содержимое файла {string.Join("", FileName, ".dat")} скопировано на контроллер");

                switch (this.Connection.Pro_State())
                {
                    case "#P_FREE":
                        if (!await this.Connection.Copy(Path.Combine(pathOnControler, string.Join("", FileName, ".src")),
                                                         Path.Combine("KRC:\\R1\\Program\\", FileName)))
                            new Exception($"Ошибка копирования файла {string.Join("", FileName, ".src")} из {pathOnControler} в KRC:\\R1\\Program\\");
                        else
                            this.LogMessage($"Файл {string.Join("", FileName, ".src")} скопирован");

                        if (!await this.Connection.Copy(Path.Combine(pathOnControler, string.Join("", FileName, ".dat")),
                                 Path.Combine("KRC:\\R1\\Program\\", FileName)))
                            new Exception($"Ошибка копирования файла {string.Join("", FileName, ".dat")} из {pathOnControler} в KRC:\\R1\\Program\\");
                        else
                            this.LogMessage($"Файл {string.Join("", FileName, ".dat")} скопирован");

                        if (!await this.Connection.Select(Path.Combine("KRC:\\R1\\Program\\", string.Join("", FileName, ".src"))))
                            new Exception("Ошибка выбора файла " + Path.Combine("KRC:\\R1\\Program\\", string.Join("", FileName, ".src")));
                        else
                            this.LogMessage("Файл программы выбран");

                        this.StatusMessage(Client.StatusProcess.ProgrammReady);
                        break;

                    case "#P_RESET":
                    case "#P_END":
                        if (!await this.Connection.SelectCancel())
                        {
                            this.LogErrorMessage("Не удаётся отменить выбор программы");
                            this.StatusMessage(Client.StatusProcess.Error);
                            return;
                        }
                        else
                            this.LogMessage("Текущий выбор отменён");

                        //int elapsed = 0;
                        //while ((!string.Equals(this.Connection.Pro_State(), "#P_FREE")) && (elapsed <= 1000))
                        //{
                        //    System.Threading.Thread.Sleep(100);
                        //    elapsed += 100;
                        //} // Не сработал, хоть статус free

                        System.Threading.Thread.Sleep(1000); // Костыль).

                        if (!await this.Connection.Copy(Path.Combine(pathOnControler, string.Join("", FileName, ".src")),
                                                         Path.Combine("KRC:\\R1\\Program\\", FileName)))
                            new Exception($"Ошибка копирования файла {string.Join("", FileName, ".src")} из {pathOnControler} в KRC:\\R1\\Program\\");
                        else
                            this.LogMessage($"Файл {string.Join("", FileName, ".src")} скопирован");

                        if (!await this.Connection.Copy(Path.Combine(pathOnControler, string.Join("", FileName, ".dat")),
                                 Path.Combine("KRC:\\R1\\Program\\", FileName)))
                            new Exception($"Ошибка копирования файла {string.Join("", FileName, ".dat")} из {pathOnControler} в KRC:\\R1\\Program\\");
                        else
                            this.LogMessage($"Файл {string.Join("", FileName, ".dat")} скопирован");

                        if (!await this.Connection.Select(Path.Combine("KRC:\\R1\\Program\\", string.Join("", FileName, ".src"))))
                            new Exception("Ошибка выбора файла " + Path.Combine("KRC:\\R1\\Program\\", string.Join("", FileName, ".src")));
                        else
                            this.LogMessage("Файл программы выбран");

                        this.StatusMessage(Client.StatusProcess.ProgrammReady);
                        break;

                    case "#P_ACTIVE":
                    case "#P_STOP":
                        this.LogMessage("Уже запущен процесс!");
                        this.StatusMessage(Client.StatusProcess.RobotBusy);
                        return;
                }
            }
            catch(Exception ex)
            {
                this.StatusMessage(Client.StatusProcess.Error);
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        /// <summary>
        /// Выбор ранее сгенерированной программы
        /// </summary>
        /// <returns></returns>
        public async Task Select(string fileName)
        {
            this.FileName = fileName;
            try
            {
                switch (this.Connection.Pro_State())
                {
                    case "#P_FREE":
                        if (!await this.Connection.Select(Path.Combine("KRC:\\R1\\Program\\", string.Join("", FileName, ".src"))))
                            new Exception("Ошибка выбора файла " + Path.Combine("KRC:\\R1\\Program\\", string.Join("", FileName, ".src")));
                        else
                            this.LogMessage($"Файл программы {string.Join("", FileName, ".src")} выбран");

                        this.StatusMessage(Client.StatusProcess.ProgrammReady);
                        break;

                    case "#P_RESET":
                    case "#P_END":
                        if (!await this.Connection.SelectCancel())
                        {
                            this.LogErrorMessage("Не удаётся отменить выбор программы");
                            this.StatusMessage(Client.StatusProcess.Error);
                            return;
                        }
                        else
                            this.LogMessage("Текущий выбор отменён");

                        System.Threading.Thread.Sleep(1000);

                        if (!await this.Connection.Select(Path.Combine("KRC:\\R1\\Program\\", string.Join("", FileName, ".src"))))
                            new Exception("Ошибка выбора файла " + Path.Combine("KRC:\\R1\\Program\\", string.Join("", FileName, ".src")));
                        else
                            this.LogMessage($"Файл программы {string.Join("", FileName, ".src")} выбран");

                        this.StatusMessage(Client.StatusProcess.ProgrammReady);
                        break;

                    case "#P_ACTIVE":
                    case "#P_STOP":
                        this.LogMessage("Уже запущен процесс!");
                        this.StatusMessage(Client.StatusProcess.RobotBusy);
                        return;
                }
            }
            catch (Exception ex)
            {
                this.StatusMessage(Client.StatusProcess.Error);
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        /// <summary>
        /// Запуск программы
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            try
            {
                if (!object.Equals(this.Connection, null))
                {
                    switch (this.Connection.Pro_State())
                    {
                        case "#P_FREE":
                            this.StatusMessage(Client.StatusProcess.ProgrammNotSelected);
                            this.Dispose();
                            return;

                        case "#P_END":
                            this.StatusMessage(Client.StatusProcess.ProgrammEnd);
                            this.Dispose();
                            return;

                        case "#P_RESET":
                            if (MessageBox.Show($"Запустить программу {NowProgramm}?\nСоединение {this.Connection.Host}:{this.Connection.Port}", $"Запуск программы", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                            {
                                this.Dispose();
                                return;
                            }
                            break;
                    }

                    if (!await this.Connection.Run())
                        new Exception($"Ошибка запуска программы {NowProgramm}");
                    else
                        this.LogMessage($"Программа {NowProgramm} запущена");

                    this.StatusMessage(Client.StatusProcess.ProgrammStart);

                    string result = this.Connection.Pro_State();

                    while ((!string.Equals(result, "#P_END")))
                    {
                        result = this.Connection.Pro_State();

                        if (string.Equals(result, "#P_STOP"))
                        {
                            this.StatusMessage(Client.StatusProcess.ProgrammPause);
                            this.LogMessage($"Программа {NowProgramm} остановлена");
                            return;
                        }

                        System.Threading.Thread.Sleep(1000);
                    }

                    if(string.Equals(result, "#P_END"))
                    {
                        this.LogMessage($"Программа {NowProgramm} завершена");
                        this.StatusMessage(Client.StatusProcess.ProgrammEnd);
                    }
                }
                this.Dispose();
            }
            catch(Exception ex)
            {
                this.StatusMessage(Client.StatusProcess.Error);
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        /// <summary>
        /// Остановка программы
        /// </summary>
        /// <returns></returns>
        public async Task Pause()
        {
            try
            {
                if (!object.Equals(this.Connection, null) && string.Equals(this.Connection.Pro_State(), "#P_ACTIVE"))
                {
                    if (!await this.Connection.Pause())
                        new Exception($"Ошибка остановки программы {NowProgramm}");
                    else
                        this.LogMessage($"Программа {NowProgramm} остановлена");

                    this.StatusMessage(Client.StatusProcess.ProgrammPause);
                }
            }
            catch (Exception ex)
            {
                this.StatusMessage(Client.StatusProcess.Error);
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        /// <summary>
        /// Аннулирование программы
        /// </summary>
        /// <returns></returns>
        public async Task Stop()
        {
            try
            {
                string state = this.Connection.Pro_State();
                if (!object.Equals(this.Connection, null) && (string.Equals(state, "#P_ACTIVE") || string.Equals(state, "#P_STOP") || string.Equals(state, "#P_END")))
                {
                    if(string.Equals(this.Connection.Pro_State(), "#P_ACTIVE"))
                    {
                        await this.Pause();

                        this.StatusMessage(Client.StatusProcess.ProgrammPause);
                        System.Threading.Thread.Sleep(1000);
                    }

                    if (!await this.Connection.SelectCancel())
                        new Exception("Ошибка аннулирования программы");
                    else
                        this.LogMessage("Программа аннулирована");

                    this.StatusMessage(Client.StatusProcess.ProgrammStop);
                }
            }
            catch (Exception ex)
            {
                this.StatusMessage(Client.StatusProcess.Error);
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Все свойства Detal равны нулю
        /// </summary>
        /// <param name="detal"></param>
        /// <returns></returns>
        private bool DetalPropertiesAreNull(Detal detal)
        {
            return detal.Long == decimal.Zero && detal.Hight == decimal.Zero && detal.Wight == decimal.Zero && detal.IndentionStart == decimal.Zero && detal.IndentionEnd == decimal.Zero && detal.DissolutionStart == decimal.Zero && detal.DissolutionEnd == decimal.Zero &&
                detal.DistanceToFirst == decimal.Zero && detal.DistanceBetween == decimal.Zero && detal.ThicknessPlita == decimal.Zero && detal.ThicknessRebro == decimal.Zero && detal.SearchOffsetStart == decimal.Zero && detal.SearchOffsetEnd == decimal.Zero;
        }

        /// <summary>
        /// Все свойства Svarka равны нулю
        /// </summary>
        /// <param name="svarka"></param>
        /// <returns></returns>
        private bool SvarkaPropertiesAreNull(Svarka svarka)
        {
            return svarka.WildingSpead == 0 && svarka.ProgramNom == 0;
        }

        #endregion

        #region Implementations of IDisposable

        ~ProcessGeneration() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                if (this.Connection == null)
                {
                    disposed = true;
                    return;
                }
                else
                   this.Connection.Dispose();
            }
            disposed = true;
        }

        #endregion
    }
}

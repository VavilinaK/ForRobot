using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

//using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using Python.Runtime;
using Python.Runtime.Native;

using ForRobot.Model;

namespace ForRobot.Libr
{
    public class Generation
    {
        #region Private variables

        private NLog.Logger Logger { get; } = NLog.LogManager.GetCurrentClassLogger();

        //private bool disposed = false;

        /// <summary>
        /// Все свойства Detal равны нулю
        /// </summary>
        /// <param name="detal"></param>
        /// <returns></returns>
        private static bool DetalPropertiesAreNull(Detal detal) => detal.Long == decimal.Zero && detal.Hight == decimal.Zero
            && detal.DissolutionStart == decimal.Zero && detal.DissolutionEnd == decimal.Zero && detal.DistanceToFirst == decimal.Zero && detal.DistanceBetween == decimal.Zero
            && detal.ThicknessPlita == decimal.Zero && detal.ThicknessRebro == decimal.Zero && detal.SearchOffsetStart == decimal.Zero && detal.SearchOffsetEnd == decimal.Zero;

        #endregion

        #region Public variables

        #region Events

        /// <summary>
        /// Событие логирования действия
        /// </summary>
        public event EventHandler<LogEventArgs> Log;
        /// <summary>
        /// Событие логирования ошибки
        /// </summary>
        public event EventHandler<LogErrorEventArgs> LogError;

        #endregion

        #endregion

        #region Internal variables

        /// <summary>
        /// Имя главной программы
        /// </summary>
        internal string FileName { get; private set; }

        /// <summary>
        /// Путь программы-генератора
        /// </summary>
        internal string PathGenerator { get; private set; }

        /// <summary>
        /// Путь для вывода
        /// </summary>
        internal string PathProgramm { get; private set; }

        #endregion

        #region Constructors

        public Generation() : this(null, null) { }

        public Generation(string pathGenerator, string fileName) : this(pathGenerator, fileName, null) { }

        public Generation(string pathGenerator, string fileName, string pathControl)
        {
            if (string.IsNullOrWhiteSpace(pathGenerator))
                throw new ArgumentNullException("pathGenerator");

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException("fileName");

            this.PathGenerator = pathGenerator;
            this.FileName = fileName;
            this.PathProgramm = pathControl;
        }

        #endregion

        #region Private functions

        private void StartGeneration(object detal, string[] args)
        {
            try
            {
                //Process process = new Process();
                switch (detal)
                {
                    case Plita plita:
                        this.LogMessage("Начат процесс генерации программы для плиты с рёбрами . . .");
                        break;

                    case PlitaStringer stringer:
                        this.LogMessage("Начат процесс генерации программы для плиты со стрингером . . .");
                        break;

                    case PlitaTreygolnik treygolnik:
                        this.LogMessage("Начат процесс генерации программы для плиты треугольником . . .");
                        break;
                }

                if (!File.Exists(this.PathGenerator))
                {
                    this.LogErrorMessage($"Не существует файла {this.PathGenerator}");
                    //this.Dispose();
                    return;
                }

                //process.StartInfo.FileName = Path.Combine(this.PathGenerator);
                //process.StartInfo.Arguments = String.Join(" ", args);
                //process.Start();
                //process.WaitForExit();
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
        }

        #endregion

        #region Internal functions

        internal void LogMessage(string message)
        {
            if (string.IsNullOrEmpty(message) || this.Log == null)
            {
                return;
            }

            this.Log(this, new LogEventArgs($"{DateTime.Now.ToString("HH:mm:ss")} " + message + "\n"));
        }

        internal void LogErrorMessage(string message) => this.LogErrorMessage(message, null);

        internal void LogErrorMessage(string message, Exception exception)
        {
            if (string.IsNullOrEmpty(message) || this.LogError == null)
            {
                return;
            }

            this.LogError(this, new LogErrorEventArgs($"{DateTime.Now.ToString("HH:mm:ss")} " + message + "\n", exception));
        }

        #endregion

        #region Public functions

        /// <summary>
        /// Проверка завершения процесса генерации
        /// </summary>
        /// <param name="pathProgram">Путь к папке с программой</param>
        /// <returns></returns>
        public bool ProccesEnd(string pathProgram)
        {
            bool res = false;
            try
            {
                if (string.IsNullOrWhiteSpace(pathProgram))
                    throw new ArgumentNullException("Путь к папке с программой");

                if (File.Exists(Path.Combine(pathProgram, string.Join("", this.FileName, ".src"))))
                {
                    this.LogMessage($"Файл {string.Join("", this.FileName, ".src")} сгенерирован");
                    res = true;
                }
                else
                    this.LogErrorMessage($"Файл {Path.Combine(pathProgram, string.Join("", this.FileName, ".src"))} не найден");
            }
            catch(Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
            return res;
        }

        public void Start() => this.Start(null);

        public void Start(Detal detal)
        {
            try
            {
                if (object.Equals(detal, null)) throw new ArgumentNullException("detal");

                //if (object.Equals(svarka, null)) throw new ArgumentNullException("svarka");

                if (DetalPropertiesAreNull(detal)) throw new Exception("Не заполнен ни один параметр детали");
                    
                //if (SvarkaPropertiesAreNull(svarka)) throw new Exception("Не заполнены параметры сварки");

                switch (detal)
                {
                    case Plita plita:
                        this.LogMessage("Начат процесс генерации программы для плиты с рёбрами . . .");
                        break;

                    case PlitaStringer stringer:
                        this.LogMessage("Начат процесс генерации программы для плиты со стрингером . . .");
                        break;

                    case PlitaTreygolnik treygolnik:
                        this.LogMessage("Начат процесс генерации программы для плиты треугольником . . .");
                        break;
                }

                if (!File.Exists(this.PathGenerator))
                {
                    this.LogErrorMessage($"Не существует файла {this.PathGenerator}");
                    //this.Dispose();
                    return;
                }

                string[,] args = { { "-p", $"{new FileInfo(this.PathGenerator).DirectoryName}\\{this.FileName}.json" }, { "-o", $"\"{this.PathProgramm}\"" }, { "-n", $"\"{this.FileName}.src\"" } };

                string arv = "";
                for(int i=0; i < args.GetLength(0); i++)
                {
                    arv += (string.IsNullOrWhiteSpace(args[i, 1]) ? "" : $" {args[i, 0]} {args[i, 1]}");
                }

                Process process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        //RedirectStandardInput = true,
                        WorkingDirectory = new FileInfo(this.PathGenerator).DirectoryName,
                        FileName = "python.exe",
                        Arguments = this.PathGenerator + " " + arv,
                        //WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                    }
                };
                process.Start();
                //process.StandardInput.Write($"/K {new DirectoryInfo(new FileInfo(this.PathGenerator).DirectoryName).Root}:& cd {new FileInfo(this.PathGenerator).DirectoryName}& py {this.PathGenerator}" + arv);
                //process.StandardInput.Flush();
                //process.StandardInput.Close();
                //process.BeginOutputReadLine();
                //string output = process.StandardOutput.ReadToEnd();
                //if (!string.IsNullOrEmpty(output) && !string.Equals(output.TrimEnd().TrimStart().Replace(">", ""), new FileInfo(this.PathGenerator).DirectoryName))
                //    new Exception(output);
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
                this.Logger.Error(ex.Message);
                //this.Dispose();
                return;
            }
        }

        #endregion

        #region Implementations of IDisposable

        //~Generation() => Dispose(false);

        //public void Dispose() => Dispose(true);

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (disposed) return;
        //    if (disposing)
        //    {
        //        if (this.Connection == null)
        //        {
        //            disposed = true;
        //            return;
        //        }
        //        else
        //            this.Connection.Dispose();
        //    }
        //    disposed = true;
        //    GC.SuppressFinalize(this);
        //}

        #endregion
    }
}

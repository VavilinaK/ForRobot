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
using ForRobot.Model.Detals;

namespace ForRobot.Libr
{
    public class Generation
    {
        #region Private variables

        private NLog.Logger Logger { get; } = NLog.LogManager.GetCurrentClassLogger();

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
        internal string PathOut { get; set; }

        #endregion

        #region Constructors

        public Generation() : this(null, null) { }

        public Generation(string pathGenerator, string fileName) : this(pathGenerator, fileName, null) { }

        public Generation(string pathGenerator, string fileName, string pathOut)
        {
            if (string.IsNullOrWhiteSpace(pathGenerator))
                throw new ArgumentNullException("pathGenerator");

            this.PathGenerator = pathGenerator;
            this.FileName = fileName;
            this.PathOut = pathOut;
        }

        #endregion

        #region Private functions



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
        public bool ProccesEnd()
        {
            bool res = false;
            try
            {
                foreach(var subdir in Directory.GetDirectories(this.PathOut))
                {
                    if (File.Exists(Path.Combine(subdir, string.Join("", this.FileName, ".src"))))
                    {
                        this.LogMessage($"Файл {string.Join("", this.FileName, ".src")} сгенерирован в {subdir}");
                        res = true;
                    }
                    else
                        this.LogErrorMessage($"Файл {Path.Combine(subdir, string.Join("", this.FileName, ".src"))} не найден");
                }
            }
            catch(Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
            }
            return res;
        }

        public void Start(Detal detal)
        {
            try
            {
                if (detal == null)
                    throw new ArgumentNullException("detal");

                if (DetalPropertiesAreNull(detal))
                    throw new Exception("Не заполнен ни один параметр детали");

                if (!File.Exists(this.PathGenerator))
                {
                    this.LogErrorMessage($"Не существует файла {this.PathGenerator}");
                    return;
                }

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

                string[] args = { $"-p {this.PathOut}\\{this.FileName}.json" , $"-o \"{this.PathOut}\"" };

                if (!string.IsNullOrWhiteSpace(this.FileName))
                    args = args.Append<string>($"-n \"{this.FileName}.src\"").ToArray<string>();

                Process process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = new FileInfo(this.PathGenerator).DirectoryName,
                        FileName = "python.exe",
                        Arguments = this.PathGenerator + " " + string.Join(" ", args),
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
                    }
                };
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
                this.Logger.Error(ex.Message);
                return;
            }
        }

        #endregion
    }
}

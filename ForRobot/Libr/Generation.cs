using System;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;

//using IronPython.Hosting;
//using Microsoft.Scripting.Hosting;

//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using Python.Runtime;
//using Python.Runtime.Native;

using ForRobot.Models;
using ForRobot.Models.Detals;

namespace ForRobot.Libr
{
    public class Generation
    {
        #region Private variables

        #endregion

        #region Public variables

        /// <summary>
        /// Имя главной программы
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Путь для вывода
        /// </summary>
        public string PathOut { get; set; }

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

        #region Constructors

        public Generation() { }

        public Generation(string fileName, string pathOut)
        {
            this.FileName = fileName;
            this.PathOut = pathOut;
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Проверка всех свойств на наличие значений
        /// </summary>
        /// <param name="detal">Проверяемый объект</param>
        /// <returns></returns>
        private bool CheckForNull(Object obj)
        {
            var value = new object(); // Значение проверяемого свойства.
            int nullKol = 0; // Кол-во свойст равных null.

            var @switch = new Dictionary<Type, Action> {
                    { typeof(System.String), () => { if(string.IsNullOrEmpty(value as string)) nullKol++; } },
                    { typeof(System.Decimal), () => { if((decimal)value == decimal.Zero) nullKol++; } },
                    { typeof(int), () => { if ((int)value == 0) nullKol++; } }
            };

            List<string>s = new List<string>(){ "BevelToStart", "BevelToEnd" };
            
            int checkKol = obj.GetType().GetProperties().Where(pr => !s.Contains(pr.Name) && @switch.ContainsKey(pr.PropertyType)).Count();

            foreach (var prop in obj.GetType().GetProperties().Where(wr => !s.Contains(wr.Name)))
            {
                value = prop.GetValue(obj);
                if (@switch.ContainsKey(prop.PropertyType)) @switch[prop.PropertyType]();
            }
            return checkKol == nullKol;
        }

        /// <summary>
        /// Метод поиска имени генератора, в зависимости от типа передаваемого объекта
        /// </summary>
        /// <param name="obj">Передаваемый объект</param>
        /// <returns></returns>
        private string GenaratorName(Object obj)
        {
            switch (obj)
            {
                //case ForRobot.Models.Detals.Plita plita:
                //    return (ConfigurationManager.GetSection("app") as ForRobot.Libr.ConfigurationProperties.AppConfigurationSection).PlitaGenerator;

                default:
                    return "none";
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

            //this.Log(this, new LogEventArgs($"{DateTime.Now.ToString("HH:mm:ss")} " + message + "\n"));
            this.Log(this, new LogEventArgs(String.Format("{0:HH:mm:ss}\t{1}", DateTime.Now, message)));
        }

        internal void LogErrorMessage(string message) => this.LogErrorMessage(message, null);

        internal void LogErrorMessage(string message, Exception exception)
        {
            if (string.IsNullOrEmpty(message) || this.LogError == null)
            {
                return;
            }

            //this.LogError(this, new LogErrorEventArgs($"{DateTime.Now.ToString("HH:mm:ss")} " + message + "\n", exception));
            this.LogError(this, new LogErrorEventArgs(String.Format("{0:HH:mm:ss}\t{1}", DateTime.Now, message), exception));
        }

        #endregion

        #region Public functions

        public bool ProccesEnd(string sPath)
        {
            bool result = false;
            if (File.Exists(Path.Combine(sPath, string.Join("", this.FileName, ".src"))))
            {
                this.LogMessage($"Файл {string.Join("", this.FileName, ".src")} сгенерирован в {sPath}");
                result = true;
            }
            else
                this.LogErrorMessage($"Файл {Path.Combine(sPath, string.Join("", this.FileName, ".src"))} не найден");
            return result;
        }        

        public void Start(Detal detal)
        {
            try
            {
                if (detal == null)
                    throw new ArgumentNullException("detal");

                if (this.CheckForNull(detal))
                    throw new Exception("Не заполнен ни один параметр детали");

                if(!File.Exists($"Scripts/{this.GenaratorName(detal)}"))
                    throw new Exception($"Не найден скрипт-генератор {this.GenaratorName(detal)}");

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
                        RedirectStandardInput = false,
                        RedirectStandardOutput = false,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = new FileInfo(Path.GetFullPath($"Scripts/{this.GenaratorName(detal)}")).DirectoryName,
                        FileName = "python.exe",
                        Arguments = Path.GetFullPath($"Scripts/{this.GenaratorName(detal)}") + " " + string.Join(" ", args)
                    }
                };
                process.ErrorDataReceived += (s, e) => 
                {
                    this.LogErrorMessage(e.Data);
                };
                process.Start();
                string outStr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new Exception(outStr);
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
                return;
            }
        }

        #endregion
    }
}

using System;
using System.IO;
using System.Diagnostics;

//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using ForRobot.Model;

namespace ForRobot.Libr
{
    public class Generation
    {
        #region Private variables

        //private bool disposed = false;
        
        /// <summary>
        /// Все свойства Detal равны нулю
        /// </summary>
        /// <param name="detal"></param>
        /// <returns></returns>
        private static bool DetalPropertiesAreNull(Detal detal) => detal.Long == decimal.Zero && detal.Hight == decimal.Zero && detal.Wight == decimal.Zero &&
            detal.IndentionStart == decimal.Zero && detal.IndentionEnd == decimal.Zero && detal.DissolutionStart == decimal.Zero && detal.DissolutionEnd == decimal.Zero &&
            detal.DistanceToFirst == decimal.Zero && detal.DistanceBetween == decimal.Zero && detal.ThicknessPlita == decimal.Zero && detal.ThicknessRebro == decimal.Zero &&
            detal.SearchOffsetStart == decimal.Zero && detal.SearchOffsetEnd == decimal.Zero;

        /// <summary>
        /// Все свойства Svarka равны нулю
        /// </summary>
        /// <param name="svarka"></param>
        /// <returns></returns>
        private static bool SvarkaPropertiesAreNull(Svarka svarka) => svarka.WildingSpead == 0 && svarka.ProgramNom == 0;

        #endregion

        #region Public variables

        public bool ProccesEnd
        {
            get
            {
                bool res = false;

                if (File.Exists(Path.Combine(new FileInfo(this.NameGenerator).DirectoryName, string.Join("", this.FileName, ".src"))))
                {
                    this.LogMessage($"Файл {string.Join("", this.FileName, ".src")} сгенерирован");
                    res = true;
                }
                else
                    this.LogErrorMessage($"Файл {Path.Combine(new FileInfo(this.NameGenerator).DirectoryName, string.Join("", this.FileName, ".src"))} не найден");

                if (File.Exists(Path.Combine(new FileInfo(this.NameGenerator).DirectoryName, string.Join("", this.FileName, ".dat"))))
                {
                    this.LogMessage($"Файл {string.Join("", this.FileName, ".dat")} сгенерирован");
                    res = true;
                }
                else
                {
                    this.LogErrorMessage($"Файл {Path.Combine(new FileInfo(this.NameGenerator).DirectoryName, string.Join("", this.FileName, ".dat"))} не найден");
                    res = false;
                }

                return res;
            }
        }

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
        /// Название сгенерированной программы
        /// </summary>
        internal string FileName { get; private set; }

        /// <summary>
        /// Название программы-генератора
        /// </summary>
        internal string NameGenerator { get; private set; }

        #endregion

        #region Constructors

        public Generation() : this(null, null) { }

        public Generation(string nameGenerator, string fileName)
        {
            if (string.IsNullOrWhiteSpace(nameGenerator))
                throw new ArgumentNullException("nameGenerator");

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException("fileName");

            this.NameGenerator = nameGenerator;
            this.FileName = fileName;
        }

        #endregion

        #region Private functions

        private void StartGeneration(object detal, string[] args)
        {
            try
            {
                Process process = new Process();
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

                if (!File.Exists(this.NameGenerator))
                {
                    this.LogErrorMessage($"Не существует файла {this.NameGenerator}");
                    //this.Dispose();
                    return;
                }

                process.StartInfo.FileName = Path.Combine(this.NameGenerator);
                process.StartInfo.Arguments = String.Join(" ", args);
                process.Start();
                process.WaitForExit();
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

        public void Start() => this.Start(null, null);

        public void Start(Detal detal, Svarka svarka)
        {
            try
            {
                if (object.Equals(detal, null))
                    throw new ArgumentNullException("detal");

                if (object.Equals(svarka, null))
                    throw new ArgumentNullException("svarka");

                if (DetalPropertiesAreNull(detal))
                    throw new Exception("Не заполнен ни один параметр детали");
                    
                if (SvarkaPropertiesAreNull(svarka))
                    throw new Exception("Не заполнены параметры сварки");

                string[] args = new string[]
                {
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

                this.StartGeneration(detal, args);
            }
            catch (Exception ex)
            {
                this.LogErrorMessage(ex.Message, ex);
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

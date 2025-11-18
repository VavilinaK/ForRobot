using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using ForRobot.Model.Detals;

namespace ForRobot.Libr.Services
{
    public class GenerationService
    {
        /// <summary>
        /// Имя главной программы
        /// </summary>
        public string ProgramName { get; private set; }

        /// <summary>
        /// Выходной путь для генерации
        /// </summary>
        public string PathOut { get; private set; }

        /// <summary>
        /// Название файла скрипта-генератора
        /// </summary>
        public string GenerationScript { get; private set; }

        public GenerationService(string pathOut, string programName, string generationScript)
        {
            this.PathOut = pathOut;
            this.ProgramName = programName;
            this.GenerationScript = generationScript;
        }

        //private static void build_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        //{

        //}

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

            List<string> propertiesNull = new List<string>();

            switch (obj)
            {
                case Plita plita:
                    propertiesNull = new List<string>() { "BevelToStart", "BevelToEnd" };
                    break;
            }
            
            int checkKol = obj.GetType().GetProperties().Where(pr => !propertiesNull.Contains(pr.Name) && @switch.ContainsKey(pr.PropertyType)).Count();  // Кол-во проверяемых свойств.
            foreach (var prop in obj.GetType().GetProperties().Where(pr => !propertiesNull.Contains(pr.Name)))
            {
                value = prop.GetValue(obj);
                if (@switch.ContainsKey(prop.PropertyType)) @switch[prop.PropertyType]();
            }
            return checkKol == nullKol;
        }
        
        /// <summary>
        /// Начало генерации
        /// </summary>
        /// <param name="detal">Деталь для генерации</param>
        public void Start(Detal detal)
        {
            try
            {
                if (detal == null)
                    throw new ArgumentNullException("detal");

                if (this.CheckForNull(detal))
                    throw new Exception("Не заполнен ни один параметр детали");

                if (!File.Exists($"Scripts/{this.GenerationScript}"))
                    throw new FileNotFoundException($"Не найден скрипт-генератор {this.GenerationScript}");

                string[] args = { $"-p {this.PathOut}\\{this.ProgramName}.json", $"-o \"{this.PathOut}\"" };
                if (!string.IsNullOrWhiteSpace(this.ProgramName))
                    args = args.Append<string>($"-n \"{this.ProgramName}.src\"").ToArray<string>();

                Process process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        UseShellExecute = false,
                        RedirectStandardInput = false,
                        RedirectStandardOutput = false,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = new FileInfo(Path.GetFullPath($"Scripts/{this.GenerationScript}")).DirectoryName,
                        FileName = "python.exe",
                        Arguments = Path.GetFullPath($"Scripts/{this.GenerationScript}") + " " + string.Join(" ", args)
                    }
                };
                process.ErrorDataReceived += (s, e) => { throw new Exception(e.Data); };
                process.Start();
                string outStr = process.StandardError.ReadToEnd();
                process.WaitForExit();
                if (process.ExitCode != 0)
                    throw new Exception(outStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

using System;
using System.IO;
//using System.Net;
using System.Windows;
//using System.Windows.Forms;
//using System.Text.Json;
//using System.Drawing;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using ForRobot.Libr;
using ForRobot.Model;

namespace ForRobot.ViewModels
{
    public class MainPageViewModel : BaseClass
    {
        #region Private variables

        //private string _filePath;

        private string _logger;
        private string _statusProcess1;
        private string _statusProcess2;

        private Detal _detal;

        private Svarka _svarka;

        private ObservableCollection<string> _typeCollection;

        private List<ProcessGeneration> _processGenerations;

        //private ProcessGeneration _processGeneration;

        private Thread _secondThread;

        //private OpenFileDialog _openFile;

        #region Readonly

        private static readonly List<string> _typeList = new List<string> { "Плита с ребром", "Плита со стрингером", "Плита треугольником" };
        private static readonly List<string> _privyazkaList = new List<string> { "Вправо", "Влево" };

        #endregion

        #region Commands

        private RelayCommand _refreshRobotCommand;

        private RelayCommand _runProgrammCommand;

        private RelayCommand _pauseProgrammCommand;

        private RelayCommand _stopProgrammCommand;

        private RelayCommand _selectProgrammCommand;

        #endregion

        #endregion

        #region Public variables

        public string Logger
        {
            get => this._logger;
            set
            {
                this._logger = value;
                RaisePropertyChanged("Logger");
            }
        }

        public string FilePath { get => Directory.GetCurrentDirectory() + @"\generation"; }

        public Detal DetalObject
        {
            get => _detal ?? (_detal = new Detal());
            set
            {
                _detal = value;
                //_detal.Change += new EventHandler(ChangeProperies); // Обределение события изменения свойств

                _detal.Change += ChangeProperies; // Обределение события изменения свойств
                //_detal.OnChange().Wait();
                RaisePropertyChanged("DetalObject");
            }
        }

        public Svarka Svarka
        {
            get => _svarka ?? (_svarka = new Svarka());
            set
            {
                _svarka = value;
                //_svarka.Change += new EventHandler(ChangeProperies); // Обределение события изменения свойств

                _svarka.Change += ChangeProperies; // Обределение события изменения свойств
                //_svarka.OnChange().Wait();
                RaisePropertyChanged("Svarka");
            }
        }

        public string StatusProcess1
        {
            get => _statusProcess1 ?? Libr.Client.StatusProcess.None;
            set
            {
                _statusProcess1 = value;
                RaisePropertyChanged("StatusProcess1");
            }
        }

        public string StatusProcess2
        {
            get => _statusProcess2;
            set
            {
                _statusProcess2 = value ?? Libr.Client.StatusProcess.None;
                RaisePropertyChanged("StatusProcess2");
            }
        }

        public string Host1
        {
            get => string.IsNullOrWhiteSpace(Properties.Settings.Default.Host1) ? "0.0.0.0" : Properties.Settings.Default.Host1;
            set
            {
                Properties.Settings.Default.Host1 = string.IsNullOrWhiteSpace(value) ? "0.0.0.0" : value;
                Properties.Settings.Default.Save();
            }
        }

        public string Port1
        {
            get => Properties.Settings.Default.Port1.ToString();
            set
            {
                Properties.Settings.Default.Port1 = string.IsNullOrWhiteSpace(value) == true ? 0000 : Convert.ToInt32(value);
                Properties.Settings.Default.Save();
            }
        }

        public string Host2
        {
            get => string.IsNullOrWhiteSpace(Properties.Settings.Default.Host2) ? "0.0.0.0" : Properties.Settings.Default.Host2;
            set
            {
                Properties.Settings.Default.Host2 = string.IsNullOrWhiteSpace(value) ? "0.0.0.0" : value;
                Properties.Settings.Default.Save();
            }
        }

        public string Port2
        {
            get => Properties.Settings.Default.Port2.ToString();
            set
            {
                Properties.Settings.Default.Port2 = string.IsNullOrWhiteSpace(value) == true ? 0000 : Convert.ToInt32(value);
                Properties.Settings.Default.Save();
            }
        }

        public string PathControllerField
        {
            get => Properties.Settings.Default.PathControllerField;
            set
            {
                Properties.Settings.Default.PathControllerField = value;
                Properties.Settings.Default.Save();
            }
        }

        public string GeneratorName
        {
            get
            {
                if (DetalObject is PlitaStringer) { return Properties.Settings.Default.PlitaStringerGenerator; }
                else if (DetalObject is PlitaTreygolnik) { return Properties.Settings.Default.PlitaTreygolnikGenerator; }
                else if (DetalObject is Plita) { return Properties.Settings.Default.PlitaGenerator; }
                else { return ""; }
            }
            set
            {
                if (DetalObject is PlitaStringer) { Properties.Settings.Default.PlitaStringerGenerator = value; }
                else if (DetalObject is PlitaTreygolnik) { Properties.Settings.Default.PlitaTreygolnikGenerator = value; }
                else if (DetalObject is Plita) { Properties.Settings.Default.PlitaGenerator = value; }
                Properties.Settings.Default.Save();
            }
        }

        public string ProgrammName
        {
            get
            {
                if (DetalObject is PlitaStringer) { return Properties.Settings.Default.PlitaStringerProgramm; }
                else if (DetalObject is PlitaTreygolnik) { return Properties.Settings.Default.PlitaTreygolnikProgramm; }
                else if (DetalObject is Plita) { return Properties.Settings.Default.PlitaProgramm; }
                else { return ""; }
            }
            set
            {
                if (DetalObject is PlitaStringer) { Properties.Settings.Default.PlitaStringerProgramm = value; }
                else if (DetalObject is PlitaTreygolnik) { Properties.Settings.Default.PlitaTreygolnikProgramm = value; }
                else if (DetalObject is Plita) { Properties.Settings.Default.PlitaProgramm = value; }
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// выбранный тип детали
        /// </summary>
        public string SelectedType
        {
            get
            {
                if (DetalObject is PlitaStringer) { return "Плита со стрингером"; }
                else if (DetalObject is PlitaTreygolnik) { return "Плита треугольником"; }
                else if (DetalObject is Plita) { return "Плита с ребром"; }
                else { return ""; }
            }
            set
            {
                if (value == "Плита с ребром") { DetalObject = new Plita(); }
                if (value == "Плита со стрингером") { DetalObject = new PlitaStringer(); }
                if (value == "Плита треугольником") { DetalObject = new PlitaTreygolnik(); }
                this.Svarka = new Svarka();
                RaisePropertyChanged("GeneratorName");
                RaisePropertyChanged("ProgrammName");
            }
        }

        /// <summary>
        /// Выбранная продольная привязка
        /// </summary>
        public string SelectedLongitudinalPrivyazka
        {
            set
            {
                if (value == "Вправо")
                    DetalObject.LongitudinalPrivyazka = Privyazka.FromLeftToRight;
                if (value == "Влево")
                    DetalObject.LongitudinalPrivyazka = Privyazka.FromRightToLeft;
            }
        }

        /// <summary>
        /// Выбранная поперечная привязка
        /// </summary>
        public string SelectedTransversePrivyazka
        {
            set
            {
                if (value == "Вправо")
                    DetalObject.TransversePrivyazka = Privyazka.FromLeftToRight;
                if (value == "Влево")
                    DetalObject.TransversePrivyazka = Privyazka.FromRightToLeft;
            }
        }

        /// <summary>
        /// Коллекция видов деталей
        /// </summary>
        public ObservableCollection<string> TypeCollection { get => _typeCollection ?? (_typeCollection = new ObservableCollection<string>(_typeList)); }

        /// <summary>
        /// Коллекция привязок
        /// </summary>
        public ObservableCollection<string> PrivyazkaCollection { get => new ObservableCollection<string>(_privyazkaList); }

        //public ProcessGeneration ProcessGeneration { get => this._processGeneration; set => this._processGeneration = value; }

        public List<ProcessGeneration> ProcessGenerations
        {
            get => this._processGenerations ?? (this._processGenerations = new List<ProcessGeneration>());
            set => this._processGenerations = value;
        }

        #endregion

        #region Constructor

        public MainPageViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            App.Current.Log += new EventHandler<LogEventArgs>(SelectAppLogger);
        }

        #endregion

        #region Commands

        //RelayCommand _selectFilePathCommand;
        //public RelayCommand SelectFilePathCommand
        //{
        //    get
        //    {
        //        return _selectFilePathCommand ??
        //            (_selectFilePathCommand = new RelayCommand(obj =>
        //            {
        //                //_openFile = new OpenFileDialog
        //                //{
        //                //    InitialDirectory = @"D:\",
        //                //    RestoreDirectory = false,
        //                //    Multiselect = false,
        //                //};

        //                //if (_openFile.ShowDialog() == DialogResult.OK)
        //                //{
        //                //    FilePath = _openFile.FileName;
        //                //}
        //            }));
        //    }
        //}

        public RelayCommand RefreshRobotCommand
        {
            get
            {
                return _refreshRobotCommand ??
                    (_refreshRobotCommand = new RelayCommand(obj =>
                    {
                        //if(this.DetalObject is Plita)
                        //    Task.Run(async () => await ((Plita)this.DetalObject).OnChange());

                        ProcessGeneration processGeneration = new ProcessGeneration(FilePath);
                        processGeneration.Log += new EventHandler<LogEventArgs>(WreteLog);
                        processGeneration.LogError += new EventHandler<LogErrorEventArgs>(WreteLogError);
                        processGeneration.Status += new EventHandler<Libr.Client.StatusEventArgs>(StatusChange);
                        this.ProcessGenerations.Add(processGeneration);

                        processGeneration.NameGenerator = this.GeneratorName;
                        processGeneration.FileName = this.ProgrammName;

                        switch (Convert.ToInt32(obj))
                        {
                            case 1:
                                processGeneration.OpenConnection(Host1, Properties.Settings.Default.Port1);
                                break;

                            case 2:
                                processGeneration.OpenConnection(Host2, Properties.Settings.Default.Port2);
                                break;
                        }

                        Task.Run(async () => await processGeneration.Generation_Select(this.PathControllerField)).Wait();

                        processGeneration.Dispose();
                        this.ProcessGenerations.Remove(processGeneration);
                    }));
            }
        }

        public RelayCommand RunProgrammCommand
        {
            get
            {
                return _runProgrammCommand ??
                    (_runProgrammCommand = new RelayCommand(obj =>
                    {
                        //if (!object.Equals(this.ProcessGeneration, null) && this.ProcessGeneration.Connection.SocketConnection.Connected)
                        //{
                        //    switch (Convert.ToInt32(obj))
                        //    {
                        //        case 1:
                        //            if (this.ProcessGeneration.Connection.Host == this.Host1 && this.ProcessGeneration.Connection.Port == Properties.Settings.Default.Port1)
                        //                Task.Run(async () => await this.ProcessGeneration.Run());
                        //            break;

                        //        case 2:
                        //            if (this.ProcessGeneration.Connection.Host == this.Host2 && this.ProcessGeneration.Connection.Port == Properties.Settings.Default.Port2)
                        //                Task.Run(async () => await this.ProcessGeneration.Run());
                        //            break;
                        //    }
                        //}
                        //else
                        //    switch (Convert.ToInt32(obj))
                        //    {
                        //        case 1:
                        //            Task.Run(async () => await Run(Host1, Properties.Settings.Default.Port1));
                        //            break;

                        //        case 2:
                        //            Task.Run(async () => await Run(Host2, Properties.Settings.Default.Port2));
                        //            break;
                        //    }

                        switch (Convert.ToInt32(obj))
                        {
                            case 1:
                                if (this.ProcessGenerations.FindIndex(p => p.Connection.Host == this.Host1 && p.Connection.Port == Properties.Settings.Default.Port1) >= 0)
                                    Task.Run(async () => await this.ProcessGenerations.Find(p => p.Connection.Host == this.Host1 && p.Connection.Port == Properties.Settings.Default.Port1).Run());
                                else
                                {
                                    this.Connection(Host1, Properties.Settings.Default.Port1);
                                    Task.Run(async () => await this.ProcessGenerations.Find(p => p.Connection.Host == this.Host1 && p.Connection.Port == Properties.Settings.Default.Port1).Run());
                                }
                                break;

                            case 2:
                                if (this.ProcessGenerations.FindIndex(p => p.Connection.Host == this.Host2 && p.Connection.Port == Properties.Settings.Default.Port2) >= 0)
                                    Task.Run(async () => await this.ProcessGenerations.Find(p => p.Connection.Host == this.Host2 && p.Connection.Port == Properties.Settings.Default.Port2).Run());
                                else
                                {
                                    this.Connection(Host2, Properties.Settings.Default.Port2);
                                    Task.Run(async () => await this.ProcessGenerations.Find(p => p.Connection.Host == this.Host2 && p.Connection.Port == Properties.Settings.Default.Port2).Run());
                                }
                                break;
                        }
                    }));
            }
        }

        public RelayCommand PauseProgrammCommand
        {
            get
            {
                return _pauseProgrammCommand ??
                    (_pauseProgrammCommand = new RelayCommand(obj =>
                    {
                        switch (Convert.ToInt32(obj))
                        {
                            case 1:
                                if (this.ProcessGenerations.FindIndex(p => p.Connection.Host == this.Host1 && p.Connection.Port == Properties.Settings.Default.Port1) >= 0)
                                    Task.Run(async () => await this.ProcessGenerations.Find(p => p.Connection.Host == this.Host1 && p.Connection.Port == Properties.Settings.Default.Port1).Pause());
                                break;

                            case 2:
                                if (this.ProcessGenerations.FindIndex(p => p.Connection.Host == this.Host2 && p.Connection.Port == Properties.Settings.Default.Port2) >= 0)
                                    Task.Run(async () => await this.ProcessGenerations.Find(p => p.Connection.Host == this.Host2 && p.Connection.Port == Properties.Settings.Default.Port2).Pause());
                                break;
                        }
                    }));
            }
        }

        public RelayCommand StopProgrammCommand
        {
            get
            {
                return _stopProgrammCommand ??
                    (_stopProgrammCommand = new RelayCommand(obj =>
                    {
                        switch (Convert.ToInt32(obj))
                        {
                            case 1:
                                if (this.ProcessGenerations.FindIndex(p => p.Connection.Host == this.Host1 && p.Connection.Port == Properties.Settings.Default.Port1) >= 0)
                                {
                                    Task.Run(async () => await this.ProcessGenerations.Find(p => p.Connection.Host == this.Host1 && p.Connection.Port == Properties.Settings.Default.Port1).Stop()).Wait();
                                    this.ProcessGenerations.Find(p => p.Connection.Host == this.Host1 && p.Connection.Port == Properties.Settings.Default.Port1).Dispose();
                                    this.ProcessGenerations.Remove(this.ProcessGenerations.Find(p => p.Connection.Host == this.Host1 && p.Connection.Port == Properties.Settings.Default.Port1));
                                }
                                else
                                {
                                    this.Connection(Host1, Properties.Settings.Default.Port1);
                                    Task.Run(async () => await this.ProcessGenerations.Find(p => p.Connection.Host == this.Host1 && p.Connection.Port == Properties.Settings.Default.Port1).Stop()).Wait();
                                    this.ProcessGenerations.Find(p => p.Connection.Host == this.Host1 && p.Connection.Port == Properties.Settings.Default.Port1).Dispose();
                                    this.ProcessGenerations.Remove(this.ProcessGenerations.Find(p => p.Connection.Host == this.Host1 && p.Connection.Port == Properties.Settings.Default.Port1));
                                }
                                break;

                            case 2:
                                if (this.ProcessGenerations.FindIndex(p => p.Connection.Host == this.Host2 && p.Connection.Port == Properties.Settings.Default.Port2) >= 0)
                                {
                                    Task.Run(async () => await this.ProcessGenerations.Find(p => p.Connection.Host == this.Host2 && p.Connection.Port == Properties.Settings.Default.Port2).Stop()).Wait();
                                    this.ProcessGenerations.Find(p => p.Connection.Host == this.Host2 && p.Connection.Port == Properties.Settings.Default.Port2).Dispose();
                                    this.ProcessGenerations.Remove(this.ProcessGenerations.Find(p => p.Connection.Host == this.Host2 && p.Connection.Port == Properties.Settings.Default.Port2));
                                }
                                else
                                {
                                    this.Connection(Host2, Properties.Settings.Default.Port2);
                                    Task.Run(async () => await this.ProcessGenerations.Find(p => p.Connection.Host == this.Host2 && p.Connection.Port == Properties.Settings.Default.Port2).Stop()).Wait();
                                    this.ProcessGenerations.Find(p => p.Connection.Host == this.Host2 && p.Connection.Port == Properties.Settings.Default.Port2).Dispose();
                                    this.ProcessGenerations.Remove(this.ProcessGenerations.Find(p => p.Connection.Host == this.Host2 && p.Connection.Port == Properties.Settings.Default.Port2));
                                }
                                break;
                        }
                    }));
            }
        }

        public RelayCommand SelectProgrammCommand
        {
            get
            {
                return _selectProgrammCommand ??
                    (_selectProgrammCommand = new RelayCommand(obj =>
                    {
                        if (string.IsNullOrWhiteSpace(GeneratorName) || string.IsNullOrWhiteSpace(ProgrammName))
                        {
                            MessageBox.Show("Не заполнено имя генератора и/или имя сгенерированной программы.", "Остановка", MessageBoxButton.OK);
                            return;
                        }

                        ProcessGeneration processGeneration = new ProcessGeneration(FilePath);
                        processGeneration.Log += new EventHandler<LogEventArgs>(WreteLog);
                        processGeneration.LogError += new EventHandler<LogErrorEventArgs>(WreteLogError);
                        processGeneration.Status += new EventHandler<Libr.Client.StatusEventArgs>(StatusChange);
                        this.ProcessGenerations.Add(processGeneration);

                        processGeneration.NameGenerator = this.GeneratorName;
                        processGeneration.FileName = this.ProgrammName;

                        switch (Convert.ToInt32(obj))
                        {
                            case 1:
                                processGeneration.OpenConnection(Host1, Properties.Settings.Default.Port1);
                                break;

                            case 2:
                                processGeneration.OpenConnection(Host2, Properties.Settings.Default.Port2);
                                break;
                        }
                        Task.Run(async () => await processGeneration.Select(ProgrammName)).Wait();
                        processGeneration.Dispose();
                        this.ProcessGenerations.Remove(processGeneration);
                    }));
            }
        }

        #endregion

        #region Private functions

        private void Connection(string host, int port)
        {
            if (string.IsNullOrWhiteSpace(host) || port == 0000)
            {
                MessageBox.Show("Не заполнен id хоста и/или порт", "Остановка", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            ProcessGeneration processGeneration = new ProcessGeneration(FilePath);
            processGeneration.Log += new EventHandler<LogEventArgs>(WreteLog);
            processGeneration.LogError += new EventHandler<LogErrorEventArgs>(WreteLogError);
            processGeneration.Status += new EventHandler<Libr.Client.StatusEventArgs>(StatusChange);

            processGeneration.OpenConnection(host, port);
            processGeneration.NameGenerator = this.GeneratorName;
            processGeneration.FileName = this.ProgrammName;
            this.ProcessGenerations.Add(processGeneration);
        }

        private async Task ChangeProperies(object sender, EventArgs e)
        {
            _secondThread = new Thread(RunSecondPotok); // Разделение потоков.
            ProcessGeneration processGeneration = new ProcessGeneration(FilePath);
            processGeneration.Log += new EventHandler<LogEventArgs>(WreteLog);
            processGeneration.LogError += new EventHandler<LogErrorEventArgs>(WreteLogError);
            processGeneration.Status += new EventHandler<Libr.Client.StatusEventArgs>(StatusChange);
            this.ProcessGenerations.Add(processGeneration);

            processGeneration.NameGenerator = this.GeneratorName;
            processGeneration.FileName = this.ProgrammName;

            processGeneration.Start(DetalObject, Svarka);
            
            if (processGeneration.ProccesEnd)
            {
                _secondThread.Start();
                processGeneration.OpenConnection(Host1, Properties.Settings.Default.Port1);
                await processGeneration.Generation_Select(Properties.Settings.Default.PathControllerField);
                //await this.ProcessConnection(Host1, Properties.Settings.Default.Port1);
            }
            processGeneration.Dispose();
            this.ProcessGenerations.Remove(processGeneration);
        }

        private void RunSecondPotok() => Task.Run(async () => await SecondPotok());

        private async Task SecondPotok()
        {
            ProcessGeneration processGeneration = new ProcessGeneration(FilePath);
            processGeneration.Log += new EventHandler<LogEventArgs>(WreteLog);
            processGeneration.LogError += new EventHandler<LogErrorEventArgs>(WreteLogError);
            processGeneration.Status += new EventHandler<Libr.Client.StatusEventArgs>(StatusChange);
            this.ProcessGenerations.Add(processGeneration);

            processGeneration.NameGenerator = this.GeneratorName;
            processGeneration.FileName = this.ProgrammName;

            processGeneration.OpenConnection(Host2, Properties.Settings.Default.Port2);
            await processGeneration.Generation_Select(this.PathControllerField);

            processGeneration.Dispose();
            this.ProcessGenerations.Remove(processGeneration);
        }

        private void WreteLog(object sender, LogEventArgs e)
        {
            App.Current.LoggerString += e.Message;
        }

        private void WreteLogError(object sender, LogErrorEventArgs e)
        {
            App.Current.LoggerString += e.Message;
            App.Current.Logger.Error(e.Message);
        }

        /// <summary>
        /// Обработчик собития изменения журнала приложения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAppLogger(object sender, LogEventArgs e)
        {
            this.Logger = ((ForRobot.App)sender).LoggerString;
        }

        /// <summary>
        /// Обработик события изменения статуса
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StatusChange(object sender, Libr.Client.StatusEventArgs e)
        {
            if (this.ProcessGenerations.Count > 0)
            {
                if (((ProcessGeneration)sender).Connection.Host == this.Host1 && ((ProcessGeneration)sender).Connection.Port == Properties.Settings.Default.Port1)
                {
                    this.StatusProcess1 = e.Message;
                }

                if (((ProcessGeneration)sender).Connection.Host == this.Host2 && ((ProcessGeneration)sender).Connection.Port == Properties.Settings.Default.Port2)
                {
                    this.StatusProcess2 = e.Message;
                }
            }
        }

        #endregion
    }
}

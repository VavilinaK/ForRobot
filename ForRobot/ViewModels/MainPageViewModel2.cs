using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ForRobot.Libr;
using ForRobot.Model;
using ForRobot.Model.Detals;
using ForRobot.Views.Pages;

namespace ForRobot.ViewModels
{
    public class MainPageViewModel2 : BaseClass
    {
        #region Private variables

        /// <summary>
        /// Время ожидания
        /// </summary>
        private int ConnectionTimeOut { get => Properties.Settings.Default.ConnectionTimeOut; }

        private Detal _detalObject;

        private string _selectedNameRobot;

        private string _selectedDetalType;

        private string _logger;

        private TabItem _selectedItem;

        private TabItem _selectedMainItem;

        private Page _nowPage;

        /// <summary>
        /// Страница с 2D изображениями
        /// </summary>
        private Page2D _page2D { get; set; } = new Page2D();

        /// <summary>
        /// Страница с 3D изображениями
        /// </summary>
        private Page3D _page3D { get; set; } = new Page3D();

        private Tuple<string, Robot> _selectedRobot;

        private ObservableCollection<Tuple<string, Robot>> _robotsCollection = new ObservableCollection<Tuple<string, Robot>>();

        #region Readonly

        /// <summary>
        /// Обработчик исключений асинхронных комманд
        /// </summary>
        private readonly Action<Exception> _exceptionCallback = new Action<Exception>(e =>
        {
            try
            {
                throw e;
            }
            catch (DivideByZeroException ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                App.Current.LoggerString += ex.Message;
                App.Current.Logger.Error(ex.Message);
            }
        });

        #endregion

        #region Commands

        private RelayCommand _openCodingCommand;
        
        private RelayCommand _standartParametrsCommand;

        private RelayCommand _selectGeneratorProgramCommand;
        
        private RelayCommand _addRobotCommand;

        private RelayCommand _deleteRobotCommand;

        private RelayCommand _upDateConnectionCommand;

        private RelayCommand _changePathOnPCtCommand;

        private RelayCommand _selectRobotCommand;

        private IAsyncCommand _generateProgramCommand;

        private IAsyncCommand _runProgramCommand;

        private IAsyncCommand _pauseProgramCommand;

        private IAsyncCommand _cancelProgramCommand;

        #endregion

        #endregion

        #region Public variables

        public Version Version { get => System.Reflection.Assembly.GetEntryAssembly().GetName().Version; }

        /// <summary>
        /// Флаг выбранной страницы
        /// </summary>
        public string PageNowString
        {
            get
            {
                if (this.PageNow is Page2D) { return "Page2D"; }
                else if (this.PageNow is Page3D) { return "Page3D"; }
                else { return ""; }
            }
            set
            {
                if (value == "Page2D") { this.PageNow = this._page2D; }
                else if (value == "Page3D") { this.PageNow = this._page3D; }
                RaisePropertyChanged("PageNowString");
            }
        }

        public string Logger { get => this._logger; set => Set(ref this._logger, value); }

        /// <summary>
        /// Название сгенерированной программы (зависит от типа детали)
        /// </summary>
        public string ProgrammName
        {
            get
            {
                if (DetalObject is Plita) { return Properties.Settings.Default.PlitaProgramm; }
                else if (DetalObject is PlitaStringer) { return Properties.Settings.Default.PlitaStringerProgramm; }
                else if (DetalObject is PlitaTreygolnik) { return Properties.Settings.Default.PlitaTreugolnikProgramm; }
                else { return ""; }
            }
            set
            {
                if (DetalObject is Plita) { Properties.Settings.Default.PlitaProgramm = value; }
                else if (DetalObject is PlitaStringer) { Properties.Settings.Default.PlitaStringerProgramm = value; }
                else if (DetalObject is PlitaTreygolnik) { Properties.Settings.Default.PlitaTreugolnikProgramm = value; }
                Properties.Settings.Default.Save();
                RaisePropertyChanged(nameof(this.ProgrammName));
            }
        }

        /// <summary>
        /// Путь к программе-генератору (зависит от типа детали)
        /// </summary>
        public string PathGenerator
        {
            get
            {
                if (DetalObject is Plita) { return Properties.Settings.Default.PlitaGenerator; }
                else if (DetalObject is PlitaStringer) { return Properties.Settings.Default.PlitaStringerGenerator; }
                else if (DetalObject is PlitaTreygolnik) { return Properties.Settings.Default.PlitaTreugolnikGenerator; }
                else { return ""; }
            }
            set
            {
                if (DetalObject is Plita) { Properties.Settings.Default.PlitaGenerator = value; }
                else if (DetalObject is PlitaStringer) { Properties.Settings.Default.PlitaStringerGenerator = value; }
                else if (DetalObject is PlitaTreygolnik) { Properties.Settings.Default.PlitaTreugolnikGenerator = value; }
                Properties.Settings.Default.Save();
                RaisePropertyChanged(nameof(this.PathGenerator));
            }
        }

        /// <summary>
        /// Нынешняя страница
        /// </summary>
        public Page PageNow
        {
            get => this._nowPage ?? (this._nowPage = this._page2D);
            set => Set(ref this._nowPage, value);
        }

        /// <summary>
        /// Объект детали
        /// </summary>
        public Detal DetalObject
        {
            get => this._detalObject;
            set
            {
                Set(ref this._detalObject, value);

                if (this.PageNow is Page2D)
                {
                    ((Page2D)this.PageNow).Detal2D = this.DetalObject;
                }
                else if (this.PageNow is Page3D)
                {
                    ((Page3D)this.PageNow).Detal3D = this.DetalObject;
                }
            }
        }

        /// <summary>
        /// выбранный тип детали
        /// </summary>
        public string SelectedDetalType
        {
            get => this._selectedDetalType;
            set
            {
                this._selectedDetalType = value;
                switch (this._selectedDetalType)
                {
                    case string a when a == DetalTypes.Plita:
                        this.DetalObject = GetSavePlita();
                        //((Plita)this.DetalObject).RibsCollection.ItemPropertyChanged += (o, e) => this.SaveDetal();
                        break;

                    case string b when b == DetalTypes.Stringer:
                        DetalObject = GetSavePlitaStringer();
                        break;

                    case string c when c == DetalTypes.Treygolnik:
                        DetalObject = GetSavePlitaTreygolnik();
                        break;
                }

                //this.DetalObject.Change += ChangeProperiesDetal; // Обределение события изменения свойств

                RaisePropertyChanged(nameof(this.SelectedDetalType), nameof(this.PathGenerator), nameof(this.ProgrammName));
            }
        }

        /// <summary>
        /// Выбранная вкладка
        /// </summary>
        public TabItem SelectedItem
        {
            get => _selectedItem;
            set => Set(ref this._selectedItem, value);
        }

        /// <summary>
        /// Выбранная вкладка
        /// </summary>
        public TabItem SelectedMainItem
        {
            get => _selectedItem;
            set => Set(ref this._selectedItem, value);
        }

        /// <summary>
        /// Выбранный робот для просмотра
        /// </summary>
        public Tuple<string, Robot> SelectedRobot { get => this._selectedRobot; set => Set(ref this._selectedRobot, value); }

        #region Управление

        /// <summary>
        /// Выбранный робот для управления
        /// </summary>
        public Robot RobotForControl { get => (this.SelectedNameRobot == "Все" || this.SelectedNameRobot == null) ? null : this.RobotsCollection.Where(p => p.Item1 == this.SelectedNameRobot).Select(item => item.Item2).ToList<Robot>()[0]; }

        /// <summary>
        /// Выбранное имя робота для управления
        /// </summary>
        public string SelectedNameRobot
        {
            get => this._selectedNameRobot;
            set
            {
                this._selectedNameRobot = value;
                RaisePropertyChanged(nameof(this.SelectedNameRobot), nameof(this.RobotForControl));
            }
        }

        public ObservableCollection<string> RobotNamesCollection { get => new ObservableCollection<string>(new List<string>() { "Все" }.Union(this.RobotsCollection.Select(item => item.Item1)).ToList<string>()); }

        #endregion

        #region Collections

        /// <summary>
        /// Коллекция видов деталей
        /// </summary>
        public ObservableCollection<string> DetalTypeCollection
        {
            get
            {
                List<string> detalTypesList = new List<string>();
                foreach (var f in typeof(ForRobot.Model.Detals.DetalTypes).GetFields())
                {
                    detalTypesList.Add(f.GetValue(null).ToString());
                }
                return new ObservableCollection<string>(detalTypesList);
            }
        }
        
        /// <summary>
        /// Коллекция роботов для просмотра
        /// </summary>
        public ObservableCollection<Tuple<string, Robot>> RobotsCollection
        {
            get => this._robotsCollection;
            set => Set(ref this._robotsCollection, value);
        }

        #endregion

        #region Commands

        public RelayCommand OpenCodingCommand
        {
            get
            {
                return _openCodingCommand ??
                    (_openCodingCommand = new RelayCommand(obj =>
                    {
                        if(((System.Windows.Controls.Primitives.ToggleButton)obj).IsChecked == true)
                        {
                            using (ForRobot.Views.Windows.InputWindow _inputWindow = new ForRobot.Views.Windows.InputWindow("Введите коэффициент для ВСЕХ позиций"))
                            {
                                if (_inputWindow.ShowDialog() == true)
                                {
                                    StringBuilder Sb = new StringBuilder();
                                    using (var hash = SHA256.Create())
                                    {
                                        Encoding enc = Encoding.UTF8;
                                        byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(_inputWindow.Answer));
                                        foreach (byte b in result)
                                            Sb.Append(b.ToString("x2"));
                                    }
                                    if (!Equals(Sb.ToString(), Properties.Settings.Default.PinCode))
                                        ((System.Windows.Controls.Primitives.ToggleButton)obj).IsChecked = false;
                                }
                            }
                        }
                    }));
            }
        }

        /// <summary>
        /// Сброс свойств до стандартных
        /// </summary>
        public RelayCommand StandartParametrsCommand
        {
            get
            {
                return _standartParametrsCommand ??
                    (_standartParametrsCommand = new RelayCommand(obj =>
                    {
                        if (DetalObject is Plita)
                        {
                            this.DetalObject = new Plita(DetalType.Plita)
                            {
                                ScoseType = ((Plita)this.DetalObject).ScoseType,
                                DiferentDistance = (((Plita)this.DetalObject).ScoseType == ScoseTypes.Rect) ? ((Plita)this.DetalObject).DiferentDistance : false
                            };
                            //((Plita)this.DetalObject).RibsCollection.ItemPropertyChanged += (o, e) => this.SaveDetal();
                        }
                        else if (DetalObject is PlitaStringer) { this.DetalObject = new PlitaStringer(DetalType.Stringer); }
                        else if (DetalObject is PlitaTreygolnik) { this.DetalObject = new PlitaTreygolnik(DetalType.Treygolnik); }
                        //this.DetalObject.Change += ChangeProperiesDetal;
                        //SaveDetal();
                    }));
            }
        }

        /// <summary>
        /// Команда выбора программы генератора
        /// </summary>
        public RelayCommand SelectGeneratorProgramCommand
        {
            get
            {
                return _selectGeneratorProgramCommand ??
                    (_selectGeneratorProgramCommand = new RelayCommand(obj =>
                    {
                        using(OpenFileDialog openFileDialog = new OpenFileDialog() { RestoreDirectory = true, InitialDirectory = Directory.GetCurrentDirectory(), CheckFileExists = true, CheckPathExists = true, Multiselect = false })
                        {
                            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                                return;

                            this.PathGenerator = openFileDialog.FileName;
                        }
                    }));
            }
        }

        /// <summary>
        /// Добавление робота
        /// </summary>
        public RelayCommand AddRobotCommand
        {
            get
            {
                return _addRobotCommand ??
                    (_addRobotCommand = new RelayCommand(obj =>
                    {
                        if (obj as string == "loading" && Properties.Settings.Default.SaveRobots != null && Properties.Settings.Default.SaveRobots.Count > 0)
                        {
                            for (int i = 0; i < Properties.Settings.Default.SaveRobots.Count; i++)
                            {
                                this.AddRobot(JsonConvert.DeserializeObject<Robot>(Properties.Settings.Default.SaveRobots[i]));
                            }
                            //return;
                        }
                        else
                            this.AddRobot(new Robot("0.0.0.0", 0000));
                        this.SelectedRobot = this.RobotsCollection.Last();
                        this.SelectedNameRobot = this.RobotNamesCollection[0];
                    }));
            }
        }

        /// <summary>
        /// Удаление робота
        /// </summary>
        public RelayCommand DeleteRobotCommand
        {
            get
            {
                return _deleteRobotCommand ??
                    (_deleteRobotCommand = new RelayCommand(obj =>
                    {
                        if(this.RobotsCollection.Count>0 && (!this.SelectedRobot.Item2.IsConnection || 
                           System.Windows.MessageBox.Show($"Удалить робота с соединением {this.SelectedRobot.Item2.Host}:{this.SelectedRobot.Item2.Port}?", "Удаление", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK))
                        {
                            this.RobotsCollection.Remove(this.SelectedRobot);
                            if (this.RobotsCollection.Count > 0)
                            {
                                this.SelectedRobot = this.RobotsCollection.Last();
                                this.SelectedNameRobot = this.RobotNamesCollection[0];
                            }
                            RaisePropertyChanged(nameof(this.RobotNamesCollection));
                        }
                    }));
            }
        }

        /// <summary>
        /// Повторное соединение
        /// </summary>
        public RelayCommand UpDateConnectionCommand
        {
            get
            {
                return _upDateConnectionCommand ??
                    (_upDateConnectionCommand = new RelayCommand(obj =>
                    {
                        this.SelectedRobot.Item2.OpenConnection(this.ConnectionTimeOut);
                    }));
            }
        }

        /// <summary>
        /// Изменение папки робота
        /// </summary>
        public RelayCommand ChangePatnOnPCCommand
        {
            get
            {
                return _changePathOnPCtCommand ??
                    (_changePathOnPCtCommand = new RelayCommand(obj =>
                    {
                        using (var fbd = new FolderBrowserDialog())
                        {
                            DialogResult result = fbd.ShowDialog();

                            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                            {
                                for(int i=0; i < this.RobotsCollection.Count; i++)
                                {
                                    this.RobotsCollection.Select(item => item.Item2).ToList<Robot>()[i].PathProgramm = Path.Combine(fbd.SelectedPath, $"R{i + 1}");
                                }
                            }
                        }
                    }));
            }
        }

        /// <summary>
        /// Выбор робота в ListBox
        /// </summary>
        public RelayCommand SelectRobotCommand
        {
            get
            {
                return _selectRobotCommand ??
                    (_selectRobotCommand = new RelayCommand(obj =>
                    {
                        //this.SelectedItem.Header = "Роботы";
                    }));
            }
        }

        #region Управление

        /// <summary>
        /// Команда генерации программы и её выпор на роботе/ах
        /// </summary>
        public IAsyncCommand GenerateProgramCommand
        {
            get
            {
                return _generateProgramCommand ??
                    (_generateProgramCommand = new AsyncRelayCommand(async obj =>
                    {
                        await GenerationProgram();
                    }, _exceptionCallback));
            }
        }

        /// <summary>
        /// Команда запуска программы на роботе/ах
        /// </summary>
        public IAsyncCommand RunProgramCommand
        {
            get
            {
                return _runProgramCommand ??
                    (_runProgramCommand = new AsyncRelayCommand(async obj =>
                    {
                        if(this.SelectedNameRobot == "Все")
                            foreach(var robot in this.RobotsCollection.Select(item => item.Item2))
                            {
                                await Task.Run(() => robot.Run());
                            }
                        else
                            await Task.Run(() => this.RobotForControl.Run());
                    }, _exceptionCallback));
            }
        }

        /// <summary>
        /// Команда остановки программы на роботе/ах
        /// </summary>
        public IAsyncCommand PauseProgramCommand
        {
            get
            {
                return _pauseProgramCommand ??
                    (_pauseProgramCommand = new AsyncRelayCommand(async obj =>
                    {
                        if (this.SelectedNameRobot == "Все")
                            foreach (var robot in this.RobotsCollection.Select(item => item.Item2))
                            {
                                await Task.Run(() => robot.Pause());
                            }
                        else
                            await Task.Run(() => this.RobotForControl.Pause());
                    }, _exceptionCallback));
            }
        }

        /// <summary>
        /// Аннулирование программы
        /// </summary>
        public IAsyncCommand CancelProgramCommand
        {
            get
            {
                return _cancelProgramCommand ??
                    (_cancelProgramCommand = new AsyncRelayCommand(async obj =>
                    {
                        if (this.SelectedNameRobot == "Все")
                            foreach (var robot in this.RobotsCollection.Select(item => item.Item2))
                            {
                                await Task.Run(() => robot.Cancel());
                            }
                        else
                            await Task.Run(() => this.RobotForControl.Cancel());
                    }, _exceptionCallback));
            }
        }

        #endregion

        #endregion

        #endregion

        #region Constructor

        public MainPageViewModel2()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            if (Properties.Settings.Default.SaveRobots == null)
                Properties.Settings.Default.SaveRobots = new System.Collections.Specialized.StringCollection();

            App.Current.Log += new EventHandler<LogEventArgs>(SelectAppLogger);
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Добавление робота
        /// </summary>
        private void AddRobot(Robot robot)
        {
            robot.ChangeRobot += new EventHandler(this.ChangeRobot);
            robot.Log += new EventHandler<ForRobot.Libr.LogEventArgs>(this.WreteLog);
            robot.LogError += new EventHandler<ForRobot.Libr.LogErrorEventArgs>(WreteLogError);
            robot.PathProgramm = (this.RobotsCollection.Count > 0) ? Path.Combine(Directory.GetParent(this.RobotsCollection.Last().Item2.PathProgramm).ToString(), $"R{this.RobotsCollection.Count + 1}") : Path.Combine(Directory.GetCurrentDirectory(), $"R{this.RobotsCollection.Count + 1}");
            //robot.ConnectionTimeOut = this.ConnectionTimeOut;
            robot.OpenConnection(this.ConnectionTimeOut);
            this.RobotsCollection.Add(new Tuple<string, Robot>($"Робот {this.RobotsCollection.Count + 1}", robot));
            RaisePropertyChanged(nameof(this.RobotNamesCollection));

            //Themes.ToolBarTrayForRobot item = new Themes.ToolBarTrayForRobot() { NameRobot = $"Робот {Items.Count + 1}" };
            //((ToolBarViewModel)item.DataContext).ChangeRobot += new EventHandler(this.ChangeRobot);
            //((ToolBarViewModel)item.DataContext).Log += new EventHandler<LogEventArgs>(WreteLog);
            //((ToolBarViewModel)item.DataContext).LogError += new EventHandler<LogErrorEventArgs>(WreteLogError);
            //((ToolBarViewModel)item.DataContext).OpenConnection(robot.Host, robot.Port, (this.ConnectionTimeOut * 1000));
            //((ToolBarViewModel)item.DataContext).Robot.PathProgramm = robot.PathProgramm;
            //((ToolBarViewModel)item.DataContext).Robot.PathControllerFolder = robot.PathControllerFolder;
            //((ToolBarViewModel)item.DataContext).Send += SendFile;
            //((ToolBarViewModel)item.DataContext).SelectProgramm += SelectFile;
            //this.Items.Add(item);
        }

        private bool _isRead = false;
        /// <summary>
        /// При изменении свойств роботов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeRobot(object sender, EventArgs e)
        {
            if (!this._isRead)
            {
                Properties.Settings.Default.SaveRobots.Clear();
                foreach (var r in this.RobotsCollection.Select(item => item.Item2))
                {
                    Properties.Settings.Default.SaveRobots.Add(r.Json);
                    Properties.Settings.Default.Save();
                }
            }
        }

        /// <summary>
        /// Системное сообщение
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WreteLog(object sender, LogEventArgs e)
        {
            App.Current.LoggerString += e.Message;
            App.Current.Logger.Trace(e.Message);
        }

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        private void SelectAppLogger(object sender, LogEventArgs e) => this.Logger = ((ForRobot.App)sender).LoggerString;

        #region Async

        private async Task GenerationProgram()
        {
            string foldForGenerate = Directory.GetParent(this.RobotsCollection.First().Item2.PathProgramm).ToString();
            
            // Запись Json-файла
            JObject jObject1 = JObject.Parse(this.DetalObject.Json);
            File.WriteAllText(Path.Combine(new FileInfo(foldForGenerate).DirectoryName, $"{this.ProgrammName}.json"), jObject1.ToString());

            // Генерация программы.
            Generation generationProcess = new Generation(foldForGenerate, this.ProgrammName);
            generationProcess.Log += new EventHandler<LogEventArgs>(WreteLog);
            generationProcess.LogError += new EventHandler<LogErrorEventArgs>(WreteLogError);
            generationProcess.Start(this.DetalObject);
            //if (!generationProcess.ProccesEnd(dat.Robot.PathProgramm))
            //    return;
        }

        #endregion

        #region Deserialize Properties

        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            ContractResolver = new ForRobot.Libr.Json.SaveAttributesResolver(),
            Formatting = Formatting.Indented
        };

        /// <summary>
        /// Настройки плиты с рёбрами
        /// </summary>
        /// <returns></returns>
        private Plita GetSavePlita() => string.IsNullOrEmpty(Properties.Settings.Default.SavePlita) ? new Plita(DetalType.Plita) : JsonConvert.DeserializeObject<Plita>(Properties.Settings.Default.SavePlita, this._jsonSettings);

        /// <summary>
        /// Настройки плиты со стрингером
        /// </summary>
        /// <returns></returns>
        private PlitaStringer GetSavePlitaStringer() => string.IsNullOrEmpty(Properties.Settings.Default.SavePlitaStringer) ? new PlitaStringer(DetalType.Stringer) : JsonConvert.DeserializeObject<PlitaStringer>(Properties.Settings.Default.SavePlitaStringer, this._jsonSettings);

        /// <summary>
        /// Настройки плиты треугольником
        /// </summary>
        /// <returns></returns>
        private PlitaTreygolnik GetSavePlitaTreygolnik() => string.IsNullOrEmpty(Properties.Settings.Default.SavePlitaTreygolnik) ? new PlitaTreygolnik(DetalType.Treygolnik) : JsonConvert.DeserializeObject<PlitaTreygolnik>(Properties.Settings.Default.SavePlitaTreygolnik, this._jsonSettings);

        #endregion

        #endregion
    }
}

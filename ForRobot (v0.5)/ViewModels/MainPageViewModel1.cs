using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Text.Json;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ForRobot.Libr;
using ForRobot.Model;

namespace ForRobot.ViewModels
{
    public class MainPageViewModel1 : BaseClass
    {
        #region Private variables

        private string _logger;

        private Detal _detal;

        private Svarka _svarka;

        //private ObservableCollection<Themes.ToolBarTrayForRobot> _items = new ObservableCollection<Themes.ToolBarTrayForRobot>() { new Themes.ToolBarTrayForRobot() };
        private ObservableCollection<Themes.ToolBarTrayForRobot> _items;

        private ObservableCollection<string> _typeCollection;

        private Generation _generation;

        private OpenFileDialog _openFileDialog { get; set; } = new OpenFileDialog()
        {
            RestoreDirectory = true,
            InitialDirectory = Directory.GetCurrentDirectory(),
            CheckFileExists = true,
            CheckPathExists = true,
            Multiselect = false
        };

        //public List<Tuple<string, int>> HostAndPortList;

        //private List<Tuple<string, int>> _settingsList
        //{
        //    get => Properties.Settings.Default.HostAndPort.Cast<Tuple<string, int>>().ToList();
        //    //set
        //    //{
        //    //    Properties.Settings.Default.HostAndPort.Cast<Tuple<string, int>>().ToList().Add(new Tuple<string, int>(value));
        //    //}
        //}

        #region Readonly

        private static readonly List<string> _typeList = new List<string> { "Плита с ребром", "Плита со стрингером", "Плита треугольником" };
        private static readonly List<string> _privyazkaList = new List<string> { "Вправо", "Влево" };

        #endregion

        #region Commands

        private RelayCommand _pathGenerationCommand;

        private RelayCommand _addItemCommand;

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

        /// <summary>
        /// Название программы-генератора (зависит от типа детали)
        /// </summary>
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

                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(this.ProgrammName))
                    this._generation = new Generation(value, this.ProgrammName);

                RaisePropertyChanged("GeneratorName");
            }
        }

        /// <summary>
        /// Название сгенерированной программы (зависит от типа детали)
        /// </summary>
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

                if (!string.IsNullOrWhiteSpace(this.GeneratorName) && !string.IsNullOrWhiteSpace(value))
                    this._generation = new Generation(this.GeneratorName, value);
            }
        }

        /// <summary>
        /// Время ожидани подключения
        /// </summary>
        public int ConnectionTimeOut
        {
            get => Properties.Settings.Default.ConnectionTimeOut;
            set
            {
                Properties.Settings.Default.ConnectionTimeOut = value;
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
                this.DetalObject.Change += ChangeProperies; // Обределение события изменения свойств
                this.Svarka = new Svarka();
                this.Svarka.Change += ChangeProperies; // Обределение события изменения свойств
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

        public Detal DetalObject
        {
            get => this._detal ?? (this._detal = new Detal());
            set => Set(ref this._detal, value);
        }

        public Svarka Svarka
        {
            get => this._svarka ?? (this._svarka = new Svarka());
            set => Set(ref this._svarka, value);
        }

        /// <summary>
        /// Коллекция элементов UserControl
        /// </summary>
        public ObservableCollection<Themes.ToolBarTrayForRobot> Items
        {
            get
            {

                if (Equals(this._items, null))
                {
                    this._items = new ObservableCollection<Themes.ToolBarTrayForRobot>();
                    for(int i = 0; i < Properties.Settings.Default.SaveData.Count; i++)
                    {
                        Robot robot = JsonSerializer.Deserialize<Robot>(Properties.Settings.Default.SaveData[i]);
                        AddItem(robot);
                    }
                }
                return this._items;
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

        #region Commands

        /// <summary>
        /// Выбор пути до программы-генератора
        /// </summary>
        public RelayCommand SelectPathCommand
        {
            get
            {
                return _pathGenerationCommand ??
                    (_pathGenerationCommand = new RelayCommand(obj =>
                    {
                        if (this._openFileDialog.ShowDialog() == DialogResult.Cancel)
                            return;

                        this.GeneratorName = this._openFileDialog.FileName;
                    }));
            }
        }

        /// <summary>
        /// Добавление в Items
        /// </summary>
        public RelayCommand AddItemCommand
        {
            get
            {
                return _addItemCommand ??
                    (_addItemCommand = new RelayCommand(obj =>
                    {
                        AddItem(new Robot("0.0.0.0", 0000));
                    }));
            }
        }

        #endregion

        #endregion

        #region Constructor

        public MainPageViewModel1()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            if (Properties.Settings.Default.SaveData == null)
                Properties.Settings.Default.SaveData = new System.Collections.Specialized.StringCollection();

            App.Current.Log += new EventHandler<LogEventArgs>(SelectAppLogger);
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Добавление UserControl
        /// </summary>
        private void AddItem(Robot robot)
        {
            Themes.ToolBarTrayForRobot item = new Themes.ToolBarTrayForRobot() { NameRobot = $"Робот {Items.Count + 1}" };
            ((ToolBarViewModel)item.DataContext).HostAndPort += new EventHandler(this.HostAndPort);
            ((ToolBarViewModel)item.DataContext).Log += new EventHandler<LogEventArgs>(WreteLog);
            ((ToolBarViewModel)item.DataContext).LogError += new EventHandler<LogErrorEventArgs>(WreteLogError);
            ((ToolBarViewModel)item.DataContext).OpenConnection(robot.Host, robot.Port, (this.ConnectionTimeOut * 1000));
            ((ToolBarViewModel)item.DataContext).PathToController = robot.PathControllerField;
            ((ToolBarViewModel)item.DataContext).Send += SendFile;
            this.Items.Add(item);
        }

        /// <summary>
        /// При изменении id хоста или порта
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HostAndPort(object sender, EventArgs e)
        {
            Properties.Settings.Default.SaveData.Clear();
            foreach (var t in this.Items)
            {
                var dat = (ToolBarViewModel)t.DataContext;
                Properties.Settings.Default.SaveData.Add(dat.Robot.Json);
            }
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Изменение свойства Detal или Svarka
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task ChangeProperies(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GeneratorName))
            {
                System.Windows.MessageBox.Show("Не заполнено имя программы-генератора.", "Остановка", MessageBoxButton.OK);
                return;
            }
            if(string.IsNullOrWhiteSpace(ProgrammName))
            {
                System.Windows.MessageBox.Show("Не заполнено имя сгенерированной программы.", "Остановка", MessageBoxButton.OK);
                return;
            }

            Generation generationProcess = new Generation(this.GeneratorName, this.ProgrammName);
            generationProcess.Log += new EventHandler<LogEventArgs>(WreteLog);
            generationProcess.LogError += new EventHandler<LogErrorEventArgs>(WreteLogError);

            generationProcess.Start(this.DetalObject, this.Svarka);

            if (generationProcess.ProccesEnd)
                foreach (var item in this.Items)
                {
                    var dat = (ToolBarViewModel)item.DataContext;
                    await Task.Run(() => dat.Robot.CopyToController(new FileInfo(this.GeneratorName).DirectoryName, this.ProgrammName));
                    await Task.Run(() => dat.Robot.Copy("KRC:\\R1\\Program\\", this.ProgrammName));
                    await Task.Run(() => dat.Robot.SelectProgramm(Path.Combine("KRC:\\R1\\Program\\", string.Join("", this.ProgrammName, ".src"))));
                }
        }

        /// <summary>
        /// Отправка файлов программы в директорию робота и выбор
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SendFile(object sender, EventArgs e)
        {
            await Task.Run(() => ((ToolBarViewModel)sender).Robot.Copy("KRC:\\R1\\Program\\", this.ProgrammName));
            await Task.Run(() => ((ToolBarViewModel)sender).Robot.SelectProgramm(Path.Combine("KRC:\\R1\\Program\\", string.Join("", this.ProgrammName, ".src"))));
        }

        /// <summary>
        /// Системное сообщение
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void WreteLog(object sender, LogEventArgs e) => App.Current.LoggerString += e.Message;

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void WreteLogError(object sender, LogErrorEventArgs e)
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

        #endregion
    }
}

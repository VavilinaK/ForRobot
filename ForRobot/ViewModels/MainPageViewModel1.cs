using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;

using ForRobot.Libr;
using ForRobot.Model;
using ForRobot.Views.Pages;

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

        private TabItem _selectedItem;

        //public List<Tuple<string, int>> HostAndPortList;

        //private List<Tuple<string, int>> _settingsList
        //{
        //    get => Properties.Settings.Default.ChangeRobot.Cast<Tuple<string, int>>().ToList();
        //    //set
        //    //{
        //    //    Properties.Settings.Default.ChangeRobot.Cast<Tuple<string, int>>().ToList().Add(new Tuple<string, int>(value));
        //    //}
        //}

        #region Readonly

        private static readonly List<string> _typeList = new List<string> { "Плита с ребром", "Плита со стрингером", "Плита треугольником" };
        private static readonly List<string> _privyazkaList = new List<string> { "Вправо", "Влево" };

        #endregion

        #region Commands

        private RelayCommand _pathGenerationCommand;

        private RelayCommand _addItemCommand;

        private RelayCommand _standartParametrsCommand;

        private RelayCommand _editPinCodeCommand;


        #endregion

        #endregion

        #region Public variables

        public Page2D Page2D { get; set; }

        public Page3D Page3D { get; set; }

        //public Page PageNow
        //{
        //    get
        //    {

        //    }
        //}

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
        /// Путь к программе-генератору (зависит от типа детали)
        /// </summary>
        public string PathGenerator
        {
            get
            {
                if (DetalObject is PlitaStringer) { return Properties.Settings.Default.PlitaStringerGenerator; }
                else if (DetalObject is PlitaTreygolnik) { return Properties.Settings.Default.PlitaTreugolnikGenerator; }
                else if (DetalObject is Plita) { return Properties.Settings.Default.PlitaGenerator; }
                else { return ""; }
            }
            set
            {
                if (DetalObject is PlitaStringer) { Properties.Settings.Default.PlitaStringerGenerator = value; }
                else if (DetalObject is PlitaTreygolnik) { Properties.Settings.Default.PlitaTreugolnikGenerator = value; }
                else if (DetalObject is Plita) { Properties.Settings.Default.PlitaGenerator = value; }
                Properties.Settings.Default.Save();

                if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(this.ProgrammName))
                    this._generation = new Generation(value, this.ProgrammName);

                RaisePropertyChanged("PathGenerator");
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
                else if (DetalObject is PlitaTreygolnik) { return Properties.Settings.Default.PlitaTreugolnikProgramm; }
                else if (DetalObject is Plita) { return Properties.Settings.Default.PlitaProgramm; }
                else { return ""; }
            }
            set
            {
                if (DetalObject is PlitaStringer) { Properties.Settings.Default.PlitaStringerProgramm = value; }
                else if (DetalObject is PlitaTreygolnik) { Properties.Settings.Default.PlitaTreugolnikProgramm = value; }
                else if (DetalObject is Plita) { Properties.Settings.Default.PlitaProgramm = value; }
                Properties.Settings.Default.Save();

                if (!string.IsNullOrWhiteSpace(this.PathGenerator) && !string.IsNullOrWhiteSpace(value))
                    this._generation = new Generation(this.PathGenerator, value);
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
        /// Пин-код
        /// </summary>
        public string PinCode
        {
            private get => Properties.Settings.Default.PinCode;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    StringBuilder Sb = new StringBuilder();

                    using (var hash = SHA256.Create())
                    {
                        Encoding enc = Encoding.UTF8;
                        byte[] result = hash.ComputeHash(enc.GetBytes(value));

                        foreach (byte b in result)
                            Sb.Append(b.ToString("x2"));
                    }

                    Properties.Settings.Default.PinCode = Sb.ToString();
                    Properties.Settings.Default.Save();
                }
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
                if (value == "Плита с ребром") { DetalObject = GetSavePlita(); }
                if (value == "Плита со стрингером") { DetalObject = GetSavePlitaStringer(); }
                if (value == "Плита треугольником") { DetalObject = GetSavePlitaTreygolnik(); }
                this.DetalObject.Change += ChangeProperiesDetal; // Обределение события изменения свойств
                this.Svarka.Change += ChangeProperiesSvarka; // Обределение события изменения свойств
                RaisePropertyChanged("PathGenerator");
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
        /// Выбранная вкладка
        /// </summary>
        public TabItem SelectedItem
        {
            get => _selectedItem;
            set => Set(ref this._selectedItem, value);
        }

        public Detal DetalObject
        {
            get => this._detal ?? (this._detal = new Detal());
            set => Set(ref this._detal, value);
        }

        public Svarka Svarka
        {
            get => this._svarka ?? (string.IsNullOrEmpty(Properties.Settings.Default.SaveSvarka) ? this._svarka = new Svarka() : this._svarka = GetSaveSvarka());
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
                    for(int i = 0; i < Properties.Settings.Default.SaveConnection.Count; i++)
                    {
                        Robot robot = JsonSerializer.Deserialize<Robot>(Properties.Settings.Default.SaveConnection[i]);
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

                        this.PathGenerator = this._openFileDialog.FileName;
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

        /// <summary>
        /// Сброс свойствдо стандартных
        /// </summary>
        public RelayCommand StandartParametrsCommand
        {
            get
            {
                return _standartParametrsCommand ??
                    (_standartParametrsCommand = new RelayCommand(obj =>
                    {
                        if (DetalObject is Plita) { this.DetalObject = new Plita(); }
                        else if (DetalObject is PlitaStringer) { this.DetalObject = new PlitaStringer(); }
                        else if (DetalObject is PlitaTreygolnik) { this.DetalObject = new PlitaTreygolnik(); }
                        SaveDetal();
                    }));
            }
        }

        /// <summary>
        /// Редактирование пин кода
        /// </summary>
        public RelayCommand EditPinCodeCommand
        {
            get
            {
                return _editPinCodeCommand ??
                    (_editPinCodeCommand = new RelayCommand(obj =>
                    {
                        System.Windows.Controls.TextBox textBox = obj as System.Windows.Controls.TextBox;

                        StringBuilder Sb = new StringBuilder();
                        using (var hash = SHA256.Create())
                        {
                            Encoding enc = Encoding.UTF8;
                            byte[] result = hash.ComputeHash(enc.GetBytes(Microsoft.VisualBasic.Interaction.InputBox("Введите пин-код", "Редактирование пин-кода", "",
                                (int)(App.Current.MainWindowView.Left + (App.Current.MainWindowView.Width / 2) - 200), 
                                (int)(App.Current.MainWindowView.Top + (App.Current.MainWindowView.Height / 2) - 100))));

                            foreach (byte b in result)
                                Sb.Append(b.ToString("x2"));
                        }

                        if (Sb.ToString() == this.PinCode)
                            textBox.IsEnabled = true;
                    }));
            }
        }

        private RelayCommand _generationCommand;

        /// <summary>
        /// Генерация программы
        /// </summary>
        public RelayCommand GenerationCommand
        {
            get
            {
                return _generationCommand ??
                    (_generationCommand = new RelayCommand(obj =>
                    {
                        Task.Run(async () => await GenerationProgram());
                        //GenerationProgram();
                        //await Task.Run(async () => await GenerationProgram());
                    }));
            }
        }

        //private IAsyncCommand _generationCommand;

        ///// <summary>
        ///// Генерация программы
        ///// </summary>
        //public IAsyncCommand GenerationCommand
        //{
        //    get
        //    {
        //        return _generationCommand ??
        //            (_generationCommand = new AsyncRelayCommand(async obj =>
        //            {
        //                //Task.Run(async () => await GenerationProgram());
        //                //GenerationProgram();
        //                await Task.Run(async () => await GenerationProgram());
        //            }, _exceptionCallback));
        //    }
        //}

        //// обработчик исключений
        //private readonly Action<Exception> _exceptionCallback = new Action<Exception>(e => {
        //    try
        //    {
        //        throw e;
        //    }
        //    catch (DivideByZeroException ex)
        //    {
        //        // обрабатываю сгенерированное исключение
        //        System.Windows.MessageBox.Show(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        // это должно ронять программу, и не теряться глубоко в асинхронных тасках
        //        // и оно работает (но это не точно)
        //        throw ex;
        //    }
        //});


        #endregion

        #endregion

        #region Constructor

        public MainPageViewModel1()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            if (Properties.Settings.Default.SaveConnection == null)
                Properties.Settings.Default.SaveConnection = new System.Collections.Specialized.StringCollection();

            App.Current.Log += new EventHandler<LogEventArgs>(SelectAppLogger);
        }

        #endregion

        #region Private functions

        private Plita GetSavePlita() => string.IsNullOrEmpty(Properties.Settings.Default.SavePlita) ? new Plita() : JsonSerializer.Deserialize<Plita>(Properties.Settings.Default.SavePlita);

        private PlitaStringer GetSavePlitaStringer() => string.IsNullOrEmpty(Properties.Settings.Default.SavePlitaStringer) ? new PlitaStringer() : JsonSerializer.Deserialize<PlitaStringer>(Properties.Settings.Default.SavePlitaStringer);

        private PlitaTreygolnik GetSavePlitaTreygolnik() => string.IsNullOrEmpty(Properties.Settings.Default.SavePlitaTreygolnik) ? new PlitaTreygolnik() : JsonSerializer.Deserialize<PlitaTreygolnik>(Properties.Settings.Default.SavePlitaTreygolnik);

        private static Svarka GetSaveSvarka() => JsonSerializer.Deserialize<Svarka>(Properties.Settings.Default.SaveSvarka);
        
        /// <summary>
        /// Добавление UserControl
        /// </summary>
        private void AddItem(Robot robot)
        {
            Themes.ToolBarTrayForRobot item = new Themes.ToolBarTrayForRobot() { NameRobot = $"Робот {Items.Count + 1}" };
            ((ToolBarViewModel)item.DataContext).ChangeRobot += new EventHandler(this.ChangeRobot);
            ((ToolBarViewModel)item.DataContext).Log += new EventHandler<LogEventArgs>(WreteLog);
            ((ToolBarViewModel)item.DataContext).LogError += new EventHandler<LogErrorEventArgs>(WreteLogError);
            ((ToolBarViewModel)item.DataContext).OpenConnection(robot.Host, robot.Port, (this.ConnectionTimeOut * 1000));
            ((ToolBarViewModel)item.DataContext).Robot.PathProgramm = robot.PathProgramm;
            ((ToolBarViewModel)item.DataContext).Robot.PathControllerFolder = robot.PathControllerFolder;
            ((ToolBarViewModel)item.DataContext).Send += SendFile;
            this.Items.Add(item);
        }

        /// <summary>
        /// При изменении свойств роботов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeRobot(object sender, EventArgs e)
        {
            Properties.Settings.Default.SaveConnection.Clear();
            foreach (var t in this.Items)
            {
                var dat = (ToolBarViewModel)t.DataContext;
                Properties.Settings.Default.SaveConnection.Add(dat.Robot.Json);
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Изменение свойств детали
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeProperiesDetal(object sender, EventArgs e)
        {
            //await Task.Run(async () => await GenerationProgram());
            SaveDetal();
        }

        /// <summary>
        /// Изменение свойств сварки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeProperiesSvarka(object sender, EventArgs e)
        {
            //await Task.Run(async () => await GenerationProgram());
            SaveSvarka();
        }

        /// <summary>
        /// Генерация программы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task GenerationProgram()
        {
            if (string.IsNullOrWhiteSpace(PathGenerator))
            {
                System.Windows.MessageBox.Show("Не заполнен путь к программе-генератору.", "Остановка", MessageBoxButton.OK);
                return;
            }
            //if(string.IsNullOrWhiteSpace(ProgrammName))
            //{
            //    System.Windows.MessageBox.Show("Не заполнено имя сгенерированной программы.", "Остановка", MessageBoxButton.OK);
            //    return;
            //}

            // Запись Json-файла
            JObject jObject1 = JObject.Parse(this.DetalObject.Json);
            JObject jObject2 = JObject.Parse(this.Svarka.Json);
            JObject result = new JObject();
            result.Merge(jObject1);
            result.Merge(jObject2);
            File.WriteAllText(Path.Combine(new FileInfo(this.PathGenerator).DirectoryName, $"{this.ProgrammName}.json"), result.ToString());

            // Генерация программы.
            Generation generationProcess = new Generation(this.PathGenerator, this.ProgrammName, new FileInfo(this.PathGenerator).DirectoryName);
            generationProcess.Log += new EventHandler<LogEventArgs>(WreteLog);
            generationProcess.LogError += new EventHandler<LogErrorEventArgs>(WreteLogError);

            generationProcess.Start(this.DetalObject, this.Svarka);

            foreach (var item in this.Items)
                if (generationProcess.ProccesEnd(((ToolBarViewModel)item.DataContext).Robot.PathProgramm))
                {
                    var dat = (ToolBarViewModel)item.DataContext;
                    await Task.Run(() => dat.Robot.CopyToController());
                    //await Task.Run(() => dat.Robot.Copy(, this.ProgrammName));
                    //await Task.Run(() => dat.Robot.SelectProgramm(Path.Combine("KRC:\\R1\\Program\\", string.Join("", this.ProgrammName, ".src"))));
                }
        }

        /// <summary>
        /// Сохранение изменений Detal
        /// </summary>
        private void SaveDetal()
        {
            if (DetalObject is Plita) { Properties.Settings.Default.SavePlita = this.DetalObject.Json; }
            else if (DetalObject is PlitaStringer) { Properties.Settings.Default.SavePlitaStringer = ""; }
            else if (DetalObject is PlitaTreygolnik) { Properties.Settings.Default.SavePlita = ""; }
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Сохранение изменений Svarka
        /// </summary>
        private void SaveSvarka()
        {
            Properties.Settings.Default.SaveSvarka = this.Svarka.Json;
            Properties.Settings.Default.Save();
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

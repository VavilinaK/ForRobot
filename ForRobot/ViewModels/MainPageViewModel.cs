using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Text;
//using System.Text.Json;
using System.ComponentModel;
using System.Threading;
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
    public class MainPageViewModel : BaseClass
    {
        #region Private variables

        /// <summary>
        /// Страница с 2D изображениями
        /// </summary>
        private Page2D Page2D { get; set; } = new Page2D();

        /// <summary>
        /// Страница с 3D изображениями
        /// </summary>
        private Page3D Page3D { get; set; } = new Page3D();

        private string _logger;

        private bool _isRead = false;

        private Detal _detal;

        //private ObservableCollection<Themes.ToolBarTrayForRobot> _items = new ObservableCollection<Themes.ToolBarTrayForRobot>() { new Themes.ToolBarTrayForRobot() };
        private ObservableCollection<Themes.ToolBarTrayForRobot> _items;

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

        private Page _nowPage;

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

        private RelayCommand _pathGenerationCommand;

        private RelayCommand _addItemCommand;

        private RelayCommand _standartParametrsCommand;

        private RelayCommand _editPinCodeCommand;

        private RelayCommand _helpCommand;
        
        private IAsyncCommand _generationCommand;

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
                if (value == "Page2D") { this.PageNow = this.Page2D; }
                else if (value == "Page3D") { this.PageNow = this.Page3D; }
                RaisePropertyChanged("PageNowString");
            }
        }

        /// <summary>
        /// Нынешняя страница
        /// </summary>
        public Page PageNow
        {
            get => this._nowPage ?? (this._nowPage = this.Page2D);
            set => Set(ref this._nowPage, value);
        }

        public string Logger { get => this._logger; set => Set(ref this._logger, value); }

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

                if (!string.IsNullOrWhiteSpace(this.PathGenerator) && !string.IsNullOrWhiteSpace(value))
                    this._generation = new Generation(this.PathGenerator, value);

                RaisePropertyChanged("ProgrammName");
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
                if (DetalObject is Plita) { return "Настил с ребром"; }
                else if (DetalObject is PlitaStringer) { return "Настил со стрингером"; }
                else if (DetalObject is PlitaTreygolnik) { return "Настил треугольником"; }
                else { return ""; }
            }
            set
            {
                if (value == "Настил с ребром")
                {
                    DetalObject = GetSavePlita();
                    ((Plita)this.DetalObject).RibsCollection.ItemPropertyChanged += (o, e) => this.SaveDetal();
                }
                if (value == "Настил со стрингером") { DetalObject = GetSavePlitaStringer(); }
                if (value == "Настил треугольником") { DetalObject = GetSavePlitaTreygolnik(); }

                this.DetalObject.Change += ChangeProperiesDetal; // Обределение события изменения свойств

                RaisePropertyChanged("SelectedType");
                RaisePropertyChanged("PathGenerator");
                RaisePropertyChanged("ProgrammName");
            }
        }

        /// <summary>
        /// Выбранная продольная привязка
        /// </summary>
        //public string SelectedLongitudinalPrivyazka
        //{
        //    set
        //    {
        //        if (value == "Вправо")
        //            DetalObject.LongitudinalPrivyazka = Privyazka.FromLeftToRight;
        //        if (value == "Влево")
        //            DetalObject.LongitudinalPrivyazka = Privyazka.FromRightToLeft;
        //    }
        //}

        /// <summary>
        /// Выбранная поперечная привязка
        /// </summary>
        //public string SelectedTransversePrivyazka
        //{
        //    set
        //    {
        //        if (value == "Вправо")
        //            DetalObject.TransversePrivyazka = Privyazka.FromLeftToRight;
        //        if (value == "Влево")
        //            DetalObject.TransversePrivyazka = Privyazka.FromRightToLeft;
        //    }
        //}

        /// <summary>
        /// Выбранная вкладка
        /// </summary>
        public TabItem SelectedItem
        {
            get => _selectedItem;
            set => Set(ref this._selectedItem, value);
        }

        /// <summary>
        /// Параметры детали
        /// </summary>
        public Detal DetalObject
        {
            get => this._detal ?? (this._detal = new Detal());
            set
            {
                Set(ref this._detal, value);

                if (this.PageNow is Page2D)
                {
                    ((Page2D)this.PageNow).Detal2D = this.DetalObject;
                }
                else if (this.PageNow is Page3D)
                {
                    ((Page3D)this.PageNow).Detal3D = this.DetalObject;
                }

                //Task.Run(async () => await this.ChangeDetalImage(this.DetalObject));
            }
        }

        /// <summary>
        /// Коллекция элементов UserControl
        /// </summary>
        public ObservableCollection<Themes.ToolBarTrayForRobot> Items { get => this._items; set => Set(ref this._items, value); }

        /// <summary>
        /// Коллекция видов деталей
        /// </summary>
        public ObservableCollection<string> TypeCollection { get => new ObservableCollection<string>(new List<string> { "Настил с ребром", "Настил со стрингером", "Настил треугольником" }); }

        /// <summary>
        /// Коллекция привязок
        /// </summary>
        public ObservableCollection<string> PrivyazkaCollection { get => new ObservableCollection<string>(new List<string> { "Вправо", "Влево" }); }

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
                        if(obj as string == "loading")
                        {
                            this.Items = new ObservableCollection<Themes.ToolBarTrayForRobot>();
                            for (int i = 0; i < Properties.Settings.Default.SaveRobots.Count; i++)
                            {
                                AddItem(JsonConvert.DeserializeObject<Robot>(Properties.Settings.Default.SaveRobots[i]));
                            }
                        }
                        else
                            AddItem(new Robot("0.0.0.0", 0000));
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
                            ((Plita)this.DetalObject).RibsCollection.ItemPropertyChanged += (o, e) => this.SaveDetal();
                        }
                        else if (DetalObject is PlitaStringer) { this.DetalObject = new PlitaStringer(DetalType.Stringer); }
                        else if (DetalObject is PlitaTreygolnik) { this.DetalObject = new PlitaTreygolnik(DetalType.Treygolnik); }
                        this.DetalObject.Change += ChangeProperiesDetal;
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

        /// <summary>
        /// Вызов справки
        /// </summary>
        public RelayCommand HelpCommand
        {
            get
            {
                return _helpCommand ??
                    (_helpCommand = new RelayCommand(obj =>
                    {
                        Help.ShowHelp(null, Path.Combine(System.Windows.Forms.Application.StartupPath, @"Help\HelpInterface.chm"));
                    }));
            }
        }

        /// <summary>
        /// Генерация программы
        /// </summary>
        public IAsyncCommand GenerationCommand
        {
            get
            {
                return _generationCommand ??
                    (_generationCommand = new AsyncRelayCommand(async obj =>
                    {
                        await GenerationProgram();
                    }, _exceptionCallback));
            }
        }

        #endregion

        #endregion

        #region Constructor

        public MainPageViewModel()
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
            //((ToolBarViewModel)item.DataContext).Robot.PathControllerFolder = robot.PathControllerFolder;
            ((ToolBarViewModel)item.DataContext).Send += SendFile;
            ((ToolBarViewModel)item.DataContext).SelectProgramm += SelectFile;
            this.Items.Add(item);
        }

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
                foreach (var t in this.Items)
                {
                    var dat = (ToolBarViewModel)t.DataContext;
                    Properties.Settings.Default.SaveRobots.Add(dat.Robot.Json);
                    Properties.Settings.Default.Save();
                }
            }
        }

        /// <summary>
        /// Изменение свойств детали
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeProperiesDetal(object sender, EventArgs e) => this.SaveDetal();

        /// <summary>
        /// Сохранение изменений Detal
        /// </summary>
        private void SaveDetal()
        {
            switch (this.DetalObject)
            {
                case Plita plita:
                    Properties.Settings.Default.SavePlita = this.DetalObject.JsonForSave;
                    break;

                case PlitaStringer plitaStringer:
                    Properties.Settings.Default.SavePlitaStringer = "";
                    break;

                case PlitaTreygolnik plitaTreygolnik:
                    Properties.Settings.Default.SavePlita = "";
                    break;
            }
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Системное сообщение
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void WreteLog(object sender, LogEventArgs e)
        {
            App.Current.LoggerString += e.Message;
            App.Current.Logger.Trace(e.Message);
        }

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

        #region Async

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

            // Запись Json-файла
            JObject jObject1 = JObject.Parse(this.DetalObject.Json);
            File.WriteAllText(Path.Combine(new FileInfo(this.PathGenerator).DirectoryName, $"{this.ProgrammName}.json"), jObject1.ToString());

            // Генерация программы.
            Generation generationProcess = new Generation(this.PathGenerator, this.ProgrammName);
            generationProcess.Log += new EventHandler<LogEventArgs>(WreteLog);
            generationProcess.LogError += new EventHandler<LogErrorEventArgs>(WreteLogError);

            foreach (var item in this.Items)
            {
                var dat = (ToolBarViewModel)item.DataContext;

                if (string.IsNullOrWhiteSpace(dat.Robot.PathProgramm))
                    generationProcess.PathProgramm = this.PathGenerator;
                else
                    generationProcess.PathProgramm = new FileInfo(dat.Robot.PathProgramm).DirectoryName;
            }

            generationProcess.Start(this.DetalObject);

            foreach (var item in this.Items)
            {
                var dat = (ToolBarViewModel)item.DataContext;
                //if (!generationProcess.ProccesEnd(dat.Robot.PathProgramm))
                //    return;
            }

            foreach (var item in this.Items)
            {
                var dat = (ToolBarViewModel)item.DataContext;

                if (dat.Robot.IsConnection)
                {
                    if (string.IsNullOrWhiteSpace(dat.Robot.PathProgramm) && Equals(System.Windows.MessageBox.Show($"{dat.Robot.Connection.Host}:{dat.Robot.Connection.Port} Не выбрана папка программы", "", MessageBoxButton.OK, MessageBoxImage.Stop), MessageBoxResult.OK))
                        return;

                    //await Task.Run(() => dat.Robot.DeleteProgramm("pc"));
                    //await Task.Run(() => dat.Robot.CopyToPC(string.Join("", this.ProgrammName, ".src")));
                    //await Task.Run(() => dat.Robot.DeleteProgramm("controller"));
                    //if (await Task.Run<bool>(() => dat.Robot.Copy(this.ProgrammName)))
                    //    await Task.Run(() => dat.Robot.SelectProgramm(string.Join("", this.ProgrammName, ".src")));
                }
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
            var dat = (ToolBarViewModel)sender;

            if (dat.Robot.IsConnection)
            {
                if (string.IsNullOrWhiteSpace(dat.Robot.PathProgramm) && Equals(System.Windows.MessageBox.Show($"{dat.Robot.Connection.Host}:{dat.Robot.Connection.Port} Не выбрана папка программы", "", MessageBoxButton.OK, MessageBoxImage.Stop), MessageBoxResult.OK))
                    return;

                //await Task.Run(() => dat.Robot.DeleteProgramm("controller"));
                if (await Task.Run<bool>(() => dat.Robot.Copy(this.ProgrammName)))
                    await Task.Run(() => dat.Robot.SelectProgramm(string.Join("", this.ProgrammName, ".src")));
            }
        }

        /// <summary>
        /// Выбор сгенерированной программы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task SelectFile(object sender, EventArgs e) => await Task.Run(() => ((ToolBarViewModel)sender).Robot.SelectProgramm(Path.Combine(string.Join("", this.ProgrammName, ".src"))));

        /// <summary>
        /// Изменение изображения робота
        /// </summary>
        /// <returns></returns>
        //private async Task ChangeDetalImage(Detal detal)
        //{
        //    await Task.Run(() => (this.PageNow is Page2D) ? (((Page2D)this.PageNow).Detal2D = detal) : (((Page3D)this.PageNow).Detal3D = detal));

        //    //if (this.PageNow is Page2D)
        //    //{
        //    //    ((Page2D)this.PageNow).Detal2D = this.DetalObject;
        //    //}
        //    //else if (this.PageNow is Page3D)
        //    //{
        //    //    ((Page3D)this.PageNow).Detal3D = this.DetalObject;
        //    //}
        //}

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

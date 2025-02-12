using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Configuration;
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
        
        private bool _sendingGeneratedFiles = true;

        /// <summary>
        /// Время ожидания
        /// </summary>
        private int ConnectionTimeOut { get => 3; }

        private Detal _detalObject = new Detal();

        private string _selectedNameRobot;

        private string _selectedDetalType;

        //private string _selectedWeldingSchema;

        private string _logger;
        
        private TabItem _selectedItem;        

        private Robot _selectedRobot;
        
        private ObservableCollection<Robot> _robotsCollection = new ObservableCollection<Robot>();
    
        /// <summary>
        /// Токен отмены задачи запуска программы на роботе (нужен при зажатии клавиши)
        /// </summary>
        private System.Threading.CancellationTokenSource _runCancelTokenSource;

        private ForRobot.Libr.ConfigurationProperties.RobotConfigurationSection RobotConfig { get; set; } = ConfigurationManager.GetSection("robot") as ForRobot.Libr.ConfigurationProperties.RobotConfigurationSection;

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

        private RelayCommand _importCommand;

        private RelayCommand _exportCommand;
        
        private RelayCommand _standartParametrsCommand;
        
        private RelayCommand _addRobotCommand;

        private RelayCommand _deleteRobotCommand;

        private RelayCommand _renameRobotCommand;

        private RelayCommand _upDateConnectionCommand;

        private RelayCommand _upDateFilesCommand;

        private RelayCommand _changePathOnPCCommand;

        private RelayCommand _selectFolderCommand;

        private RelayCommand _selectRobotCommand;

        private static RelayCommand _openImageCommand;
        
        private RelayCommand _helpCommand;

        private RelayCommand _propertiesCommand;


        private IAsyncCommand _generateProgramCommand;

        private IAsyncCommand _dropFilesCommand;

        private IAsyncCommand _runProgramCommandAsync;

        private IAsyncCommand _retentionRunButtonCommandAsync;

        private IAsyncCommand _pauseProgramCommand;

        private IAsyncCommand _cancelProgramCommand;

        private IAsyncCommand _selectGeneratProgramCommand;

        private IAsyncCommand _selectFileCommand;

        private IAsyncCommand _deleteFileCommand;

        #endregion

        #endregion

        #region Public variables

        public Version Version { get => System.Reflection.Assembly.GetEntryAssembly().GetName().Version; }

        private double _statusColumnWidth = Properties.Settings.Default.StatusColumnWidth;
        public double StatusColumnWidth
        {
            get => this._statusColumnWidth;
            set
            {
                this._statusColumnWidth = value;
                Properties.Settings.Default.StatusColumnWidth = this._statusColumnWidth;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Отправляются ли сгенерированные файлы на робота/ов
        /// </summary>
        public bool SendingGeneratedFiles { get => this._sendingGeneratedFiles; set => Set(ref this._sendingGeneratedFiles, value); }

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
        /// Выбранный тип детали
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
                        ((Plita)this.DetalObject).RibsCollection.ItemPropertyChanged += (o, e) => this.SaveDetal();
                        //foreach (var item in ((Plita)this.DetalObject).WeldingSchema) item.Change += (o, e) => 
                        //                                                                          {
                        //                                                                              if (((Plita)this.DetalObject).SelectedWeldingSchema != ForRobot.Model.Detals.WeldingSchemas.GetDescription(ForRobot.Model.Detals.WeldingSchemas.SchemasTypes.Edit))
                        //                                                                                  ((Plita)this.DetalObject).SelectedWeldingSchema = ForRobot.Model.Detals.WeldingSchemas.GetDescription(ForRobot.Model.Detals.WeldingSchemas.SchemasTypes.Edit);
                        //                                                                              this.SaveDetal();
                        //                                                                          };
                        break;

                    case string b when b == DetalTypes.Stringer:
                        DetalObject = GetSavePlitaStringer();
                        break;

                    case string c when c == DetalTypes.Treygolnik:
                        DetalObject = GetSavePlitaTreygolnik();
                        break;
                }
                this.DetalObject.Change += (s, o) => { SaveDetal(); }; // Обределение события изменения свойств
                //foreach (var item in this.DetalObject.WeldingSchema) item.Change += (s, o) => { SaveDetal(); };
                RaisePropertyChanged(nameof(this.SelectedDetalType), nameof(this.ProgrammName));
            }
        }

        ///// <summary>
        ///// Нынешняя страница
        ///// </summary>
        //public Page PageNow { get => this._nowPage ?? (this._nowPage = this._page2D); set => Set(ref this._nowPage, value); }

        /// <summary>
        /// Объект детали
        /// </summary>
        public Detal DetalObject { get => this._detalObject; set => Set(ref this._detalObject, value); }

        /// <summary>
        /// Выбранная вкладка
        /// </summary>
        public TabItem SelectedItem { get => _selectedItem; set => Set(ref this._selectedItem, value); }

        /// <summary>
        /// Выбранный робот для просмотра
        /// </summary>
        public Robot SelectedRobot { get => this._selectedRobot; set => Set(ref this._selectedRobot, value); }

        #region Control

        /// <summary>
        /// Имена роботов для управления
        /// </summary>
        public ObservableCollection<string> RobotNamesCollection { get => new ObservableCollection<string>(new List<string>() { "Все" }.Union(this.RobotsCollection.Select(item => item.Name)).ToList<string>()); }
        
        /// <summary>
        /// Выбранный робот для управления
        /// </summary>
        public Robot RobotForControl { get => (this.SelectedNameRobot == "Все" || this.SelectedNameRobot == null) ? null : this.RobotsCollection.Where(p => p.Name == this.SelectedNameRobot).Select(item => item).ToList<Robot>().First(); }

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
        /// Коллекция возможных схем сварки
        /// </summary>
        public ObservableCollection<string> WeldingSchemaCollection
        {
            get
            {
                var Descriptions = typeof(ForRobot.Model.Detals.WeldingSchemas.SchemasTypes).GetFields().Select(field => field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).SingleOrDefault() as System.ComponentModel.DescriptionAttribute);
                List<string> DescriptionList = Descriptions.Where(item => item != null).Select(item => item.Description).ToList<string>();
                return new ObservableCollection<string>(DescriptionList);
            }
        }

        /// <summary>
        /// Коллекция всех введённых роботов
        /// </summary>
        public ObservableCollection<Robot> RobotsCollection { get => this._robotsCollection; set => Set(ref this._robotsCollection, value); }
        
        #endregion

        #region Commands

        /// <summary>
        /// Открытие панели управления
        /// </summary>
        public RelayCommand OpenCodingCommand
        {
            get
            {
                return _openCodingCommand ??
                    (_openCodingCommand = new RelayCommand(obj =>
                    {
                        var h = ((System.Windows.Controls.TabControl)obj).SelectedItem as System.Windows.Controls.TabItem;
                        if (h.Header.ToString() == "Управление")
                        {
                            string pass = "";
                            using (ForRobot.Views.Windows.InputWindow _inputWindow = new ForRobot.Views.Windows.InputWindow("Введите пин-код") { Title = "Управление процессом на роботе" })
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
                                    pass = Sb.ToString(); 
                                }
                            }
                            if (!Equals(pass, Properties.Settings.Default.PinCode))
                                ((System.Windows.Controls.TabControl)obj).SelectedItem = ((System.Windows.Controls.TabControl)obj).Items[0];
                        }
                    }));
            }
        }

        /// <summary>
        /// Импорт параметров программы
        /// </summary>
        public RelayCommand ImportCommand
        {
            get
            {
                return _importCommand ??
                    (_importCommand = new RelayCommand(obj =>
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog()
                        {
                            Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt",
                            Title = "Импорт параметров программы"
                        };

                        if (openFileDialog.ShowDialog() == DialogResult.Cancel && string.IsNullOrEmpty(openFileDialog.FileName))
                            return;

                        this.DetalObject = JsonConvert.DeserializeObject<Plita>(JObject.Parse(File.ReadAllText(openFileDialog.FileName), _jsonLoadSettings).ToString(), this._jsonSettings);
                        this.SaveDetal();

                        string message = $"{DateTime.Now.ToString("HH:mm:ss")} Импортированы параметры программы из файла {openFileDialog.FileName}\n";
                        App.Current.LoggerString = message + App.Current.LoggerString;
                        App.Current.Logger.Trace(message + $"Содержание файла {openFileDialog.FileName}:\n" + File.ReadAllText(openFileDialog.FileName) + "\n");
                    }));
            }
        }

        /// <summary>
        /// Экспорт параметров программы
        /// </summary>
        public RelayCommand ExportCommand
        {
            get
            {
                return _exportCommand ??
                    (_exportCommand = new RelayCommand(obj =>
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog()
                        {
                            Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt",
                            Title = "Экспорт параметров программы"
                        };

                        if (saveFileDialog.ShowDialog() == DialogResult.Cancel && string.IsNullOrEmpty(saveFileDialog.FileName))
                            return;

                        File.WriteAllText(saveFileDialog.FileName, this.DetalObject.JsonForSave);
                        if (File.Exists(saveFileDialog.FileName))
                        {
                            string message = $"{DateTime.Now.ToString("HH:mm:ss")} Параметры программы экспортированы в файл {saveFileDialog.FileName}\n";
                            App.Current.LoggerString = message + App.Current.LoggerString;
                            App.Current.Logger.Trace(message + $"Содержание файла {saveFileDialog.FileName}:\n" + this.DetalObject.JsonForSave + "\n");
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
                        switch (this.SelectedDetalType)
                        {
                            case string a when a == DetalTypes.Plita:
                                this.DetalObject = new Plita(DetalType.Plita)
                                {
                                    ScoseType = ((Plita)this.DetalObject).ScoseType,
                                    DiferentDistance = ((Plita)this.DetalObject).DiferentDistance,
                                    ParalleleRibs = ((Plita)this.DetalObject).ParalleleRibs,
                                    DiferentDissolutionLeft = ((Plita)this.DetalObject).DiferentDissolutionLeft,
                                    DiferentDissolutionRight = ((Plita)this.DetalObject).DiferentDissolutionRight
                                };
                                ((Plita)this.DetalObject).RibsCollection.ItemPropertyChanged += (o, e) => this.SaveDetal();
                                break;

                            case string b when b == DetalTypes.Stringer:
                                this.DetalObject = new PlitaStringer(DetalType.Stringer);
                                break;

                            case string c when c == DetalTypes.Treygolnik:
                                this.DetalObject = new PlitaTreygolnik(DetalType.Treygolnik);
                                break;
                        }
                        this.DetalObject.Change += (s, o) => { this.SaveDetal(); }; // Обределение события изменения свойств
                        this.SaveDetal();
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
                        this.AddRobot(this.GetNewRobot());
                        this.SelectedRobot = this.RobotsCollection.Last();
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
                        Robot robot = (Robot)obj;
                        if (System.Windows.MessageBox.Show($"Удалить робота с соединением {robot.Host}:{robot.Port}?", "Удаление", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
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
        /// Переименование робота
        /// </summary>
        public RelayCommand RenameRobotCommand
        {
            get
            {
                return _renameRobotCommand ??
                    (_renameRobotCommand = new RelayCommand(obj =>
                    {
                        Robot robot = (Robot)obj;
                        using (ForRobot.Views.Windows.InputWindow _inputWindow = new ForRobot.Views.Windows.InputWindow("Введите новое название для робота") { Title = "Переименование робота" })
                        {
                            if (_inputWindow.ShowDialog() == true)
                            {
                                robot.Name = _inputWindow.Answer;
                            }
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
                        this.SelectedRobot.OpenConnection(this.ConnectionTimeOut);
                    }));
            }
        }

        /// <summary>
        /// Обновление содержимого робота
        /// </summary>
        public RelayCommand UpDateFilesCommand
        {
            get
            {
                return _upDateFilesCommand ??
                    (_upDateFilesCommand = new RelayCommand(obj =>
                    {
                        Task.Run(async () => await this.SelectedRobot.GetFilesAsync());                        
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
                return _changePathOnPCCommand ??
                    (_changePathOnPCCommand = new RelayCommand(obj =>
                    {
                        using (var fbd = new FolderBrowserDialog())
                        {
                            DialogResult result = fbd.ShowDialog();
                            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                            {
                                for(int i=0; i < this.RobotsCollection.Count; i++)
                                {
                                    this.RobotsCollection.ToList<Robot>()[i].PathProgramm = Path.Combine(fbd.SelectedPath, $"R{i + 1}");
                                }
                            }
                        }
                    }));
            }
        }

        /// <summary>
        /// Выбор папки на контроллер
        /// </summary>
        public RelayCommand SelectFolderCommand
        {
            get
            {
                return _selectFolderCommand ??
                    (_selectFolderCommand = new RelayCommand(obj =>
                    {
                        this.SelectedRobot.PathControllerFolder = Path.Combine(ForRobot.Libr.Client.JsonRpcConnection.DefaulRoot, obj as string);
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
                        this.SelectedRobot = this.RobotsCollection.Where(item => item == obj).First();
                    }));
            }
        }

        /// <summary>
        /// Открытие изображения детали
        /// </summary>
        public static RelayCommand OpenImageCommand
        {
            get
            {
                return _openImageCommand ??
                    (_openImageCommand = new RelayCommand(obj =>
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create((obj as Image).Source as BitmapImage));

                        string filePath = Path.Combine(Path.GetTempPath(), "Параметры_детали.png");

                        using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                        {
                            encoder.Save(fileStream);
                        }

                        ProcessStartInfo Info = new ProcessStartInfo()
                        {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            FileName = "explorer.exe",
                            WindowStyle = ProcessWindowStyle.Normal,
                            Arguments = filePath
                        };
                        Process.Start(Info);
                    }));
            }
        }

        /// <summary>
        /// Открытие chm-справки
        /// </summary>
        public RelayCommand HelpCommand
        {
            get
            {
                return _helpCommand ??
                    (_helpCommand = new RelayCommand(obj =>
                    {
                        Help.ShowHelp(null, "Help/HelpManual.chm");
                    }));
            }
        }

        /// <summary>
        /// Открытие окна настроек
        /// </summary>
        public RelayCommand PropertiesCommand
        {
            get
            {
                return _propertiesCommand ??
                    (_propertiesCommand = new RelayCommand(obj =>
                    {
                        if (object.Equals(App.Current.PropertiesWindow, null))
                        {
                            App.Current.PropertiesWindow = new Views.Windows.PropertiesWindow();
                            App.Current.PropertiesWindow.Closed += (a, b) => App.Current.PropertiesWindow = null;
                            App.Current.PropertiesWindow.Owner = App.Current.MainWindowView;
                            App.Current.PropertiesWindow.Show();
                        }
                    }));
            }
        }
        
        #region Async Commands

        /// <summary>
        /// Команда генерации программы и её выбор на роботе/ах
        /// </summary>
        public IAsyncCommand GenerateProgramCommandAsync
        {
            get
            {
                return _generateProgramCommand ??
                    (_generateProgramCommand = new AsyncRelayCommand(async obj =>
                    {
                        try
                        {
                            // Сброс фокуса перед генерацией.
                            System.Windows.Input.Keyboard.ClearFocus();
                            System.Windows.Input.FocusManager.SetFocusedElement(System.Windows.Input.FocusManager.GetFocusScope(obj as FrameworkElement), null);

                            string foldForGenerate = Directory.GetParent(this.RobotsCollection.First().PathProgramm).ToString(); // Путь для генерации скриптом.

                            // Запись Json-файла
                            JObject jObject = JObject.Parse(this.DetalObject.Json);

                            int[] sumRobots;
                            if (this.SelectedNameRobot == "Все")
                            { 
                                sumRobots = new int[this.RobotsCollection.Count];
                                for (int i=0; i<this.RobotsCollection.Count(); i++)
                                {
                                    sumRobots[i] = i + 1;
                                }
                            }
                            else
                                sumRobots = new int[1] { this.RobotsCollection.IndexOf(this.RobotsCollection.Where(p => p.Name == this.SelectedNameRobot).ToArray()[0]) + 1 };
                            jObject.Add("robots", JToken.FromObject(sumRobots)); // Запись в json-строку выбранных для генерации роботов (не зависит от подключения).

                            var sch = WeldingSchemas.GetSchema(this.DetalObject.WeldingSchema);
                            jObject.Add("welding_sequence", JToken.FromObject(sch));

                            File.WriteAllText(Path.Combine(foldForGenerate, $"{this.ProgrammName}.json"), jObject.ToString());
                            if (File.Exists(Path.Combine(foldForGenerate, $"{this.ProgrammName}.json")))
                                App.Current.Logger.Trace($"{DateTime.Now.ToString("HH:mm:ss")} Сгенерирован файл {Path.Combine(foldForGenerate, $"{this.ProgrammName}.json")}, содержащий:\n" + jObject.ToString() + "\n");

                            // Генерация программы.
                            Generation generationProcess = new Generation(this.ProgrammName, foldForGenerate);
                            generationProcess.Log += new EventHandler<LogEventArgs>(WreteLog);
                            generationProcess.LogError += new EventHandler<LogErrorEventArgs>(WreteLogError);
                            generationProcess.Start(this.DetalObject);
                            
                            // Проверка успеха генерации
                            if (this.SelectedNameRobot == "Все")
                            {
                                for (int i = 0; i < this.RobotsCollection.Count(); i++)
                                    generationProcess.ProccesEnd(this.RobotsCollection[i].PathProgramm);
                            }
                            else
                            {
                                if (!generationProcess.ProccesEnd(this.RobotForControl.PathProgramm))
                                    return;
                            }

                            if (!this.SendingGeneratedFiles)
                                return;

                            Exception ex;
                            if (this.RobotsCollection.Count > 0 && string.IsNullOrWhiteSpace(this.RobotsCollection[0].PathProgramm))
                            {
                                ex = new Exception($"{DateTime.Now.ToString("HH:mm:ss")} Отказ в передачи файлов: не выбрана папка программы.\n");
                                WreteLogError(this, new LogErrorEventArgs(ex.Message, ex));
                                System.Windows.MessageBox.Show(String.Format("{0}", ex.Message), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            if (this.SelectedNameRobot == "Все")
                                for (int i = 0; i < this.RobotsCollection.Count(); i++)
                                {
                                    if (System.Windows.MessageBox.Show($"Передать файлы программы?\n" +
                                        $"Из папки {this.RobotsCollection[i].PathControllerFolder} будут удалены все файлы.", $"{this.RobotsCollection[i].Name}", MessageBoxButton.OKCancel, MessageBoxImage.Question,
                                        MessageBoxResult.OK, System.Windows.MessageBoxOptions.DefaultDesktopOnly) != MessageBoxResult.OK)
                                        continue;

                                    if (!this.RobotsCollection[i].IsConnection)
                                    {
                                        ex = new StreamJsonRpc.ConnectionLostException($"{DateTime.Now.ToString("HH:mm:ss")} {this.RobotsCollection[i].Host}:{this.RobotsCollection[i].Port} Отказ в передачи файлов: отсутствует соединение с {i + 1} роботом.\n");
                                        WreteLogError(this, new LogErrorEventArgs(ex.Message, ex));
                                        System.Windows.MessageBox.Show(String.Format("{0}", ex.Message), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                        continue;
                                    }

                                    await Task.Run<bool>(() => this.RobotsCollection[i].DeleteFileOnPC());

                                    if (!await Task.Run<bool>(() => this.RobotsCollection[i].CopyToPC(string.Join("", this.ProgrammName, ".src"))))
                                        continue;

                                    await this.RobotsCollection[i].GetFilesAsync();

                                    for(int y = 0; y < this.RobotsCollection[i].Files.Count; y++)
                                    {
                                        var item = this.RobotsCollection[i].Files[y];
                                        var folder = item.Search(this.RobotsCollection[i].PathControllerFolder.Split(new char[] { '\\' }).Last());

                                        if (folder == null)
                                            continue;

                                        foreach (var child in folder.Children.Where(f => f.Type == Model.Controls.FileTypes.DataList || f.Type == Model.Controls.FileTypes.Program))
                                        {
                                            await Task.Run(() => this.RobotsCollection[i].DeleteFile(Path.Combine(ForRobot.Libr.Client.JsonRpcConnection.DefaulRoot, child.Path)));
                                        }
                                    }

                                    if (!await Task.Run<bool>(() => this.RobotsCollection[i].Copy(this.ProgrammName)))
                                        continue;

                                    await Task.Run(() => this.RobotsCollection[i].SelectProgramByName(string.Join("", this.ProgrammName, ".src")));
                                }
                            else if(this.RobotForControl.IsConnection)
                            {
                                if (System.Windows.MessageBox.Show($"Передать файлы программы?\n" +
                                    $"Из папки {this.RobotForControl.PathControllerFolder} будут удалены все файлы.", this.SelectedNameRobot, MessageBoxButton.OKCancel, MessageBoxImage.Question,
                                    MessageBoxResult.OK, System.Windows.MessageBoxOptions.DefaultDesktopOnly) != MessageBoxResult.OK)
                                    return;

                                await Task.Run<bool>(() => this.RobotForControl.DeleteFileOnPC());

                                if (!await Task.Run<bool>(() => this.RobotForControl.CopyToPC(string.Join("", this.ProgrammName, ".src"))))
                                    return;

                                for (int i = 0; i < this.RobotForControl.Files.Count; i++)
                                {
                                    var item = this.RobotForControl.Files[i];
                                    var folder = item.Search(this.RobotForControl.PathControllerFolder.Split(new char[] { '\\' }).Last());

                                    if (folder == null)
                                        continue;

                                    foreach (var child in folder.Children.Where(f => f.Type == Model.Controls.FileTypes.DataList || f.Type == Model.Controls.FileTypes.Program))
                                    {
                                        await Task.Run(() => this.RobotForControl.DeleteFile(Path.Combine(ForRobot.Libr.Client.JsonRpcConnection.DefaulRoot, child.Path)));
                                    }
                                }

                                if (!await Task.Run<bool>(() => this.RobotForControl.Copy(this.ProgrammName)))
                                    return;

                                await Task.Run(() => this.RobotForControl.SelectProgramByName(string.Join("", this.ProgrammName, ".src")));
                            }
                            else
                            {
                                ex = new StreamJsonRpc.ConnectionLostException($"{DateTime.Now.ToString("HH:mm:ss")} {this.RobotForControl.Host}:{this.RobotForControl.Port} Отказ в передачи файлов: отсутствует соединение с выбранным роботом.\n");
                                WreteLogError(this, new LogErrorEventArgs(ex.Message, ex));
                                System.Windows.MessageBox.Show(String.Format("{0}", ex.Message), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch(Exception e)
                        {
                            App.Current.LoggerString += e.Message;
                            App.Current.Logger.Error(e.Message, e);
                        }
                    }, _exceptionCallback));
            }
        }

        /// <summary>
        /// Команда отправки файлов на робота/ов
        /// </summary>
        public IAsyncCommand DropFilesCommandAsync
        {
            get
            {
                return _dropFilesCommand ??
                  (_dropFilesCommand = new AsyncRelayCommand(async obj =>
                  {
                      System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog()
                      {
                          Filter = "Source Code or Data files (*.src, *.dat)|*.src;*.dat|Data files (*.dat)|*.dat|Source Code File (*.src)|*src",
                          Title = $"Отправка файла/ов на {this.RobotsCollection.IndexOf(this.SelectedRobot)} робота",
                          Multiselect = true
                      };

                      if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel && (string.IsNullOrEmpty(openFileDialog.FileName) || string.IsNullOrEmpty(openFileDialog.FileNames[0])))
                          return;

                      foreach (var path in openFileDialog.FileNames)
                      {
                          ForRobot.Model.Controls.File file = new Model.Controls.File(path);
                          string tempFile = Path.Combine(Robot.PathOfTempFolder, file.Name);

                          if (!this.SelectedRobot.CopyToPC(file.Path, tempFile))
                              continue;

                          if (!this.SelectedRobot.Copy(tempFile, Path.Combine(this.SelectedRobot.PathControllerFolder, file.Name)))
                              continue;
                      }

                      await this.SelectedRobot.GetFilesAsync();

                      foreach(var file in this.SelectedRobot.Files)
                      {
                          foreach (var path in openFileDialog.FileNames)
                          {
                              file.Search(Path.GetFileName(path)).IsCopy = true;
                          }
                      }
                      openFileDialog.Dispose();
                  }, this._exceptionCallback));
            }
        }

        /// <summary>
        /// Команда запуска программы на роботе/ах
        /// </summary>
        public IAsyncCommand RunProgramCommandAsync
        {
            get
            {
                return _runProgramCommandAsync ??
                    (_runProgramCommandAsync = new AsyncRelayCommand(async obj =>
                    {
                        Robot robot = (Robot)obj;
                        //if (robot.Pro_State == "#P_RESET" && System.Windows.MessageBox.Show($"Запустить программу {robot.RobotProgramName}?",
                        //                                                                    $"{this.RobotsCollection.Where(item => item == robot).Select(item => item.Name).First()}", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK,
                        //                                                                    System.Windows.MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK ||

                        //    robot.Pro_State == "#P_END" && System.Windows.MessageBox.Show($"Перезапустить программу {robot.RobotProgramName}?",
                        //                                                                  $"{this.RobotsCollection.Where(item => item == robot).Select(item => item.Name).First()}", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK,
                        //                                                                  System.Windows.MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK ||
                        //    robot.Pro_State == "#P_STOP")
                        //{
                        //    await Task.Run(() => robot.Run());
                        //}

                        this._runCancelTokenSource = new System.Threading.CancellationTokenSource();
                        if (robot.Pro_State == "#P_RESET" || robot.Pro_State == "#P_END" || robot.Pro_State == "#P_STOP")
                        {
                            while (!this._runCancelTokenSource.IsCancellationRequested)
                            {
                                await Task.Delay(new TimeSpan(0, 0, 0, 0, 1000), this._runCancelTokenSource.Token);
                                await Task.Run(() => robot.Run(), this._runCancelTokenSource.Token);
                            }
                        }
                    }, _exceptionCallback));
            }
        }

        /// <summary>
        /// Команда удержания кнопки запуска
        /// </summary>
        public IAsyncCommand RetentionRunButtonCommandAsync
        {
            get
            {
                return _retentionRunButtonCommandAsync ??
                    (_retentionRunButtonCommandAsync = new AsyncRelayCommand(async obj =>
                    {
                        //Robot robot = (Robot)obj;
                        //await Task.Run(() => robot.Run());

                        this._runCancelTokenSource.Cancel();
                    }, _exceptionCallback));
            }
        }

        /// <summary>
        /// Команда остановки программы на роботе/ах
        /// </summary>
        public IAsyncCommand PauseProgramCommandAsync
        {
            get
            {
                return _pauseProgramCommand ??
                    (_pauseProgramCommand = new AsyncRelayCommand(async obj =>
                    {
                        Robot robot = (Robot)obj;
                        await Task.Run(() => robot.Pause());
                    }, _exceptionCallback));
            }
        }

        /// <summary>
        /// Аннулирование программы
        /// </summary>
        public IAsyncCommand CancelProgramCommandAsync
        {
            get
            {
                return _cancelProgramCommand ??
                    (_cancelProgramCommand = new AsyncRelayCommand(async obj =>
                    {
                        Robot robot = (Robot)obj;

                        if (robot.Pro_State == "#P_ACTIVE" && System.Windows.MessageBox.Show($"Прервать выполнение программы {robot.RobotProgramName}?",
                                                                                            $"{this.RobotsCollection.Where(item => item == robot).Select(item => item.Name).First()}", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK,
                                                                                            System.Windows.MessageBoxOptions.DefaultDesktopOnly) != MessageBoxResult.OK)
                            return;

                        await Task.Run(() => robot.Cancel());
                    }, _exceptionCallback));
            }
        }
        
        /// <summary>
        /// Выбор сгенерированной программы
        /// </summary>
        public IAsyncCommand SelectGeneratProgramCommandAsync
        {
            get
            {
                return _selectGeneratProgramCommand ??
                    (_selectGeneratProgramCommand = new AsyncRelayCommand(async obj =>
                    {
                        if (this.SelectedNameRobot == "Все")
                            foreach (var robot in this.RobotsCollection)
                            {
                                await Task.Run(() => robot.SelectProgramByName(string.Join("", this.ProgrammName, ".src")));
                            }
                        else
                            await Task.Run(() => this.RobotForControl.SelectProgramByName(string.Join("", this.ProgrammName, ".src")));

                    }, _exceptionCallback));
            }
        }

        /// <summary>
        /// Выбор файла программы
        /// </summary>
        public IAsyncCommand SelectFileCommandAsync
        {
            get
            {
                return _selectFileCommand ??
                    (_selectFileCommand = new AsyncRelayCommand(async obj =>
                    {
                        string sFilePath = obj as string;
                        await Task.Run(() => this.SelectedRobot.SelectProgramByPath(sFilePath));
                    }, _exceptionCallback));
            }
        }

        /// <summary>
        /// Удаление узла в дереве навигации
        /// </summary>
        public IAsyncCommand DeleteFileCommandAsync
        {
            get
            {
                return _deleteFileCommand ??
                    (_deleteFileCommand = new AsyncRelayCommand(async obj =>
                    {
                        List<string> checkedFiles;
                        if(obj == null)
                        {
                            checkedFiles = new List<string>();
                            foreach (var file in this.SelectedRobot.Files)
                            {
                                Stack<ForRobot.Model.Controls.IFile> stack = new Stack<Model.Controls.IFile>();
                                stack.Push(file);
                                ForRobot.Model.Controls.IFile current;
                                do
                                {
                                    current = stack.Pop();
                                    ObservableCollection<ForRobot.Model.Controls.IFile> files = current.Children;

                                    if (current.IsCheck)
                                        checkedFiles.Add(current.Path);

                                    foreach(var f in files)
                                        stack.Push(f);
                                }
                                while (stack.Count > 0);
                            }
                        }
                        else
                            checkedFiles = new List<string>() { obj as string };

                        foreach (string path in checkedFiles)
                        {
                            await Task.Run(() => this.SelectedRobot.DeleteFile(Path.Combine(ForRobot.Libr.Client.JsonRpcConnection.DefaulRoot, path)));
                        }
                        await this.SelectedRobot.GetFilesAsync();
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

            if (Properties.Settings.Default.SaveRobots.Count > 0)
            {
                for (int i = 0; i < Properties.Settings.Default.SaveRobots.Count; i++)
                {
                    this.AddRobot(JsonConvert.DeserializeObject<Robot>(Properties.Settings.Default.SaveRobots[i]));
                }
            }
            else
                this.AddRobot(this.GetNewRobot());
        }

        #endregion

        #region Private functions

        private Robot GetNewRobot()
        {
            return new Robot()
            {
                PathProgramm = (this.RobotsCollection.Count > 0) ?
                                            Path.Combine(Directory.GetParent(this.RobotsCollection.Last().PathProgramm).ToString(), $"R{this.RobotsCollection.Count + 1}")
                                            : Path.Combine(this.RobotConfig.PathForGeneration, $"R{this.RobotsCollection.Count + 1}"),
                PathControllerFolder = this.RobotConfig.PathControllerFolder
            };
        }

        /// <summary>
        /// Добавление робота
        /// </summary>
        private void AddRobot(Robot robot)
        {
            if (string.IsNullOrEmpty(robot.Name))
                robot.Name = $"Робот {this.RobotsCollection.Count + 1}";

            robot.Log += new EventHandler<ForRobot.Libr.LogEventArgs>(this.WreteLog);
            robot.LogError += new EventHandler<ForRobot.Libr.LogErrorEventArgs>(WreteLogError);
            robot.OpenConnection(this.ConnectionTimeOut * 1000);           
            robot.ChangeRobot += (s, e) => 
            {
                Properties.Settings.Default.SaveRobots.Clear();
                foreach (var r in this.RobotsCollection)
                {
                    Properties.Settings.Default.SaveRobots.Add(r.Json);
                    Properties.Settings.Default.Save();
                }
            };
            this.RobotsCollection.Add(robot);
            RaisePropertyChanged(nameof(this.RobotsCollection), nameof(this.RobotNamesCollection));
        }

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
        private void WreteLog(object sender, LogEventArgs e)
        {
            App.Current.LoggerString = e.Message + App.Current.LoggerString;
            App.Current.Logger.Trace(e.Message);
        }

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WreteLogError(object sender, LogErrorEventArgs e)
        {
            App.Current.LoggerString = e.Message + App.Current.LoggerString;
            App.Current.Logger.Error(e.Message);
        }

        /// <summary>
        /// Обработчик собития изменения журнала приложения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAppLogger(object sender, LogEventArgs e) => this.Logger = ((ForRobot.App)sender).LoggerString;

        ///// <summary>
        ///// Отправка на файлов на ПК робота
        ///// </summary>
        ///// <param name="robot"></param>
        //private bool FileToPC(Robot robot)
        //{
        //    try
        //    {

        //    }
        //    catch(Exception ex)
        //    {
        //        this.LogErrorMessage(ex.Message, ex);
        //        return false;
        //    }
        //    return true;
        //}

        #region Deserialize Properties

        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            ContractResolver = new ForRobot.Libr.Json.SaveAttributesResolver(),
            Formatting = Formatting.Indented,
            
        };

        private readonly JsonLoadSettings _jsonLoadSettings = new JsonLoadSettings()
        {
            CommentHandling = CommentHandling.Ignore
        };

        /// <summary>
        /// Настройки плиты с рёбрами
        /// </summary>
        /// <returns></returns>
        private Plita GetSavePlita() => string.IsNullOrEmpty(Properties.Settings.Default.SavePlita) ? new Plita(DetalType.Plita) : JsonConvert.DeserializeObject<Plita>(JObject.Parse(Properties.Settings.Default.SavePlita, _jsonLoadSettings).ToString(), this._jsonSettings);

        /// <summary>
        /// Настройки плиты со стрингером
        /// </summary>
        /// <returns></returns>
        private PlitaStringer GetSavePlitaStringer() => string.IsNullOrEmpty(Properties.Settings.Default.SavePlitaStringer) ? new PlitaStringer(DetalType.Stringer) : JsonConvert.DeserializeObject<PlitaStringer>(JObject.Parse(Properties.Settings.Default.SavePlitaStringer, _jsonLoadSettings).ToString(), this._jsonSettings);

        /// <summary>
        /// Настройки плиты треугольником
        /// </summary>
        /// <returns></returns>
        private PlitaTreygolnik GetSavePlitaTreygolnik() => string.IsNullOrEmpty(Properties.Settings.Default.SavePlitaTreygolnik) ? new PlitaTreygolnik(DetalType.Treygolnik) : JsonConvert.DeserializeObject<PlitaTreygolnik>(JObject.Parse(Properties.Settings.Default.SavePlitaTreygolnik, _jsonLoadSettings).ToString(), this._jsonSettings);

        #endregion

        #endregion
    }
}

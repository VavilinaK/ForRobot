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

        private bool _isRead = false;

        /// <summary>
        /// Время ожидания
        /// </summary>
        private int ConnectionTimeOut { get => 3; }

        private Detal _detalObject = new Detal();

        private string _selectedNameRobot;

        private string _selectedDetalType;

        private string _logger;
        
        private TabItem _selectedItem;

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

        private RelayCommand _upDateFilesCommand;

        private RelayCommand _changePathOnPCtCommand;

        private RelayCommand _selectRobotCommand;

        private RelayCommand _helpCommand;

        private RelayCommand _propertiesCommand;

        private IAsyncCommand _generateProgramCommand;

        private IAsyncCommand _runProgramCommand;

        private IAsyncCommand _pauseProgramCommand;

        private IAsyncCommand _cancelProgramCommand;

        private IAsyncCommand _selectGeneratProgramCommand;

        private IAsyncCommand _selectFileCommand;

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
        public Page PageNow { get => this._nowPage ?? (this._nowPage = this._page2D); set => Set(ref this._nowPage, value); }

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
                        ((Plita)this.DetalObject).RibsCollection.ItemPropertyChanged += (o, e) => this.SaveDetal();
                        break;

                    case string b when b == DetalTypes.Stringer:
                        DetalObject = GetSavePlitaStringer();
                        break;

                    case string c when c == DetalTypes.Treygolnik:
                        DetalObject = GetSavePlitaTreygolnik();
                        break;
                }

                this.DetalObject.Change += ChangeProperiesDetal; // Обределение события изменения свойств

                RaisePropertyChanged(nameof(this.SelectedDetalType), nameof(this.PathGenerator), nameof(this.ProgrammName));
            }
        }

        /// <summary>
        /// Выбранная вкладка
        /// </summary>
        public TabItem SelectedItem { get => _selectedItem; set => Set(ref this._selectedItem, value); }

        ///// <summary>
        ///// Выбранная вкладка
        ///// </summary>
        //public TabItem SelectedMainItem
        //{
        //    get => _selectedItem;
        //    set => Set(ref this._selectedItem, value);
        //}

        /// <summary>
        /// Выбранный робот для просмотра
        /// </summary>
        public Tuple<string, Robot> SelectedRobot { get => this._selectedRobot; set => Set(ref this._selectedRobot, value); }

        #region Управление

        /// <summary>
        /// Имена роботов для управления
        /// </summary>
        public ObservableCollection<string> RobotNamesCollection { get => new ObservableCollection<string>(new List<string>() { "Все" }.Union(this.RobotsCollection.Select(item => item.Item1)).ToList<string>()); }
        
        /// <summary>
        /// Выбранный робот для управления
        /// </summary>
        public Robot RobotForControl { get => (this.SelectedNameRobot == "Все" || this.SelectedNameRobot == null) ? null : this.RobotsCollection.Where(p => p.Item1 == this.SelectedNameRobot).Select(item => item.Item2).ToList<Robot>().First(); }

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
        /// Коллекция роботов для просмотра
        /// </summary>
        public ObservableCollection<Tuple<string, Robot>> RobotsCollection { get => this._robotsCollection; set => Set(ref this._robotsCollection, value); }

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
                        if(((System.Windows.Controls.Primitives.ToggleButton)obj).IsChecked == true)
                        {
                            using (ForRobot.Views.Windows.InputWindow _inputWindow = new ForRobot.Views.Windows.InputWindow("Введите пин-код") { Title = "Управление программой" })
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
                            this._isRead = true;
                            for (int i = 0; i < Properties.Settings.Default.SaveRobots.Count; i++)
                            {
                                this.AddRobot(JsonConvert.DeserializeObject<Robot>(Properties.Settings.Default.SaveRobots[i]));
                            }
                            this._isRead = false;
                        }
                        else
                            this.AddRobot(new Robot());

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
        /// Обновление содержимого робота
        /// </summary>
        public RelayCommand UpDateFilesCommand
        {
            get
            {
                return _upDateFilesCommand ??
                    (_upDateFilesCommand = new RelayCommand(obj =>
                    {
                        this.SelectedRobot.Item2.GetFiles();
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
                        //if(obj is Tuple<string, Robot>)
                        //{
                            this.SelectedRobot = this.RobotsCollection.Where(item => item == obj).First();
                        //    return;
                        //}

                        //if (obj is System.Windows.Controls.ComboBox)
                        //{
                        //    ((System.Windows.Controls.ComboBox)obj).Focus();
                        //    return;
                        //}
                        //this.SelectedItem.Header = "Роботы";
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
        /// Открытие окна настроик
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

        #region Async

        /// <summary>
        /// Команда генерации программы и её выбор на роботе/ах
        /// </summary>
        public IAsyncCommand GenerateProgramCommand
        {
            get
            {
                return _generateProgramCommand ??
                    (_generateProgramCommand = new AsyncRelayCommand(async obj =>
                    {
                        //await GenerationProgram();
                        string foldForGenerate = Directory.GetParent(this.RobotsCollection.First().Item2.PathProgramm).ToString();

                        // Запись Json-файла
                        JObject jObject1 = JObject.Parse(this.DetalObject.Json);
                        File.WriteAllText(Path.Combine(foldForGenerate, $"{this.ProgrammName}.json"), jObject1.ToString());

                        // Генерация программы.
                        Generation generationProcess = new Generation(this.PathGenerator, this.ProgrammName, foldForGenerate);
                        generationProcess.Log += new EventHandler<LogEventArgs>(WreteLog);
                        generationProcess.LogError += new EventHandler<LogErrorEventArgs>(WreteLogError);
                        generationProcess.Start(this.DetalObject);

                        if (!generationProcess.ProccesEnd())
                            return;

                        if (this.SelectedNameRobot == "Все")
                            foreach (var robot in this.RobotsCollection.Where(item => item.Item2.IsConnection))
                            {
                                if (string.IsNullOrWhiteSpace(robot.Item2.PathProgramm))
                                {
                                    System.Windows.MessageBox.Show("Не выбрана папка программы", $"{robot.Item1}", MessageBoxButton.OK, MessageBoxImage.Stop);
                                    return;
                                }
                                await Task.Run(() => robot.Item2.DeleteProgramOnPC());
                                await Task.Run(() => robot.Item2.CopyToPC(string.Join("", this.ProgrammName, ".src")));

                                if (System.Windows.MessageBox.Show($"Удалить все файлы из {robot.Item2.PathControllerFolder}?", $"{robot.Item1}", MessageBoxButton.OKCancel, MessageBoxImage.Question,
                                    MessageBoxResult.OK, System.Windows.MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                                    await Task.Run(() => robot.Item2.DeleteProgramm());
                                else
                                    continue;

                                if (System.Windows.MessageBox.Show($"Копировать файлы программы в {robot.Item2.PathControllerFolder}?", $"{robot.Item1}", MessageBoxButton.OKCancel, MessageBoxImage.Question,
                                        MessageBoxResult.OK, System.Windows.MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK && await Task.Run<bool>(() => robot.Item2.Copy(this.ProgrammName)))
                                    await Task.Run(() => robot.Item2.SelectProgramm(string.Join("", this.ProgrammName, ".src")));
                                else
                                    continue;
                            }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(this.RobotForControl.PathProgramm))
                            {
                                System.Windows.MessageBox.Show("Не выбрана папка программы", $"{this.SelectedNameRobot}", MessageBoxButton.OK, MessageBoxImage.Stop);
                                return;
                            }
                            await Task.Run(() => this.RobotForControl.DeleteProgramOnPC());
                            await Task.Run(() => this.RobotForControl.CopyToPC(string.Join("", this.ProgrammName, ".src")));

                            if (System.Windows.MessageBox.Show($"Удалить все файлы из {this.RobotForControl.PathControllerFolder}?", $"{this.SelectedNameRobot}", MessageBoxButton.OKCancel, MessageBoxImage.Question,
                                MessageBoxResult.OK, System.Windows.MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK)
                                await Task.Run(() => this.RobotForControl.DeleteProgramm());
                            else
                                return;

                            if (System.Windows.MessageBox.Show($"Копировать файлы программы в {this.RobotForControl.PathControllerFolder}?", $"{this.SelectedNameRobot}", MessageBoxButton.OKCancel, MessageBoxImage.Question,
                                MessageBoxResult.OK, System.Windows.MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK && await Task.Run<bool>(() => this.RobotForControl.Copy(this.ProgrammName)))
                                await Task.Run(() => this.RobotForControl.SelectProgramm(string.Join("", this.ProgrammName, ".src")));
                            else
                                return;
                        }

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
                        if (this.SelectedNameRobot == "Все")
                            foreach (var robot in this.RobotsCollection.Select(item => item.Item2))
                            {
                                if (robot.Pro_State == "#P_RESET" && System.Windows.MessageBox.Show($"Запустить программу {robot.RobotProgramName}?",
                                                                                                                                      $"{this.RobotsCollection.Where(item => item.Item2 == robot).Select(item => item.Item1).First()}", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK,
                                                                                                                                      System.Windows.MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK ||
                                    robot.Pro_State == "#P_END" && System.Windows.MessageBox.Show($"Перезапустить программу {robot.RobotProgramName}?",
                                                                                                  $"{this.RobotsCollection.Where(item => item.Item2 == robot).Select(item => item.Item1)}", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK,
                                                                                                  System.Windows.MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK ||
                                    robot.Pro_State == "#P_STOP")
                                {
                                    await Task.Run(() => robot.Run());
                                }
                            }
                        else if (this.RobotForControl.Pro_State == "#P_RESET" && System.Windows.MessageBox.Show($"Запустить программу {this.RobotForControl.RobotProgramName}?",
                                                                                                           $"{this.SelectedNameRobot}", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK,
                                                                                                           System.Windows.MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK ||
                            this.RobotForControl.Pro_State == "#P_END" && System.Windows.MessageBox.Show($"Перезапустить программу {this.RobotForControl.RobotProgramName}?",
                                                                                                         $"{this.SelectedNameRobot}", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK,
                                                                                                         System.Windows.MessageBoxOptions.DefaultDesktopOnly) == MessageBoxResult.OK ||
                            this.RobotForControl.Pro_State == "#P_STOP")
                        {
                            await Task.Run(() => this.RobotForControl.Run());
                        }
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

        /// <summary>
        /// Выбор файла программы
        /// </summary>
        public IAsyncCommand SelectGeneratProgramCommand
        {
            get
            {
                return _selectGeneratProgramCommand ??
                    (_selectGeneratProgramCommand = new AsyncRelayCommand(async obj =>
                    {
                        if (this.SelectedNameRobot == "Все")
                            foreach (var robot in this.RobotsCollection.Select(item => item.Item2))
                            {
                                await Task.Run(() => robot.SelectProgramm(string.Join("", this.ProgrammName, ".src")));
                            }
                        else
                            await Task.Run(() => this.RobotForControl.SelectProgramm(string.Join("", this.ProgrammName, ".src")));

                    }, _exceptionCallback));
            }
        }

        /// <summary>
        /// Выбор сгенерированной программы
        /// </summary>
        public IAsyncCommand SelectFileCommand
        {
            get
            {
                return _selectFileCommand ??
                    (_selectFileCommand = new AsyncRelayCommand(async obj =>
                    {
                        ForRobot.Model.Controls.File file = (ForRobot.Model.Controls.File)obj;
                        if (file.Type == FileTypes.Program)
                            await Task.Run(() => this.SelectedRobot.Item2.SelectProgramm(file.Name));

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
            robot.OpenConnection(this.ConnectionTimeOut);
            this.RobotsCollection.Add(new Tuple<string, Robot>($"Робот {this.RobotsCollection.Count + 1}", robot));
            RaisePropertyChanged(nameof(this.RobotNamesCollection));
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
                foreach (var r in this.RobotsCollection.Select(item => item.Item2))
                {
                    Properties.Settings.Default.SaveRobots.Add(r.Json);
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

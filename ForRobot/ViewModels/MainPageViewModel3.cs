using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.Configuration;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using GalaSoft.MvvmLight.Messaging;

using HelixToolkit.Wpf;

using ForRobot.Libr;
using ForRobot.Services;
using ForRobot.Model;
using ForRobot.Model.Detals;

namespace ForRobot.ViewModels
{
    public class MainPageViewModel3 : BaseClass
    {
        #region Private variables

        private readonly IModelExporter _modelExporter = new ExporterService();
        private readonly IFileDialogService _fileDialogService = new FileDialogService();

        private bool _isVisibleGridLinesVisual3D = true;

        private object _activeContent;

        private string _programName
        {
            get
            {
                switch (this.SelectedFile.Detal)
                {
                    case Plita plita:
                        return App.Current.Settings.PlitaProgramName;

                    case PlitaStringer plitaStringer:
                        return App.Current.Settings.PlitaStringerProgramName;

                    case PlitaTreygolnik plitaTreygolnik:
                        return App.Current.Settings.PlitaTreugolnikProgramName;

                    default:
                        return string.Empty;
                }
            }
        }

        private string _scriptName
        {
            get
            {
                switch (this.SelectedFile.Detal)
                {
                    case Plita plita:
                        return App.Current.Settings.PlitaScriptName;

                    case PlitaStringer plitaStringer:
                        return App.Current.Settings.PlitaStringerScriptName;

                    case PlitaTreygolnik plitaTreygolnik:
                        return App.Current.Settings.PlitaTreugolnikScriptName;

                    default:
                        return string.Empty;
                }
            }
        }

        private Model.File3D.File3D _selectedFile;

        private object _selectedObject;

        private Robot _selectedRobot;

        private ObservableCollection<Robot> _robotsCollection = new ObservableCollection<Robot>();
        private ObservableCollection<Model.File3D.SceneItem> _sceneItemsCollection = new ObservableCollection<Model.File3D.SceneItem>();
        private ObservableCollection<AppMessage> _messagesCollection = new ObservableCollection<AppMessage>();

        /// <summary>
        /// Обработчик исключений асинхронных комманд
        /// </summary>
        private static readonly Action<Exception> _exceptionCallback = new Action<Exception>(e =>
        {
            try
            {
                throw e;
            }
            catch (DivideByZeroException ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            catch (System.Threading.Tasks.TaskCanceledException ex)
            {
                //App.Current.Logger.Error(new Exception(ex.TargetSite.DeclaringType.ToString() + "\t|\t" + ex.TargetSite + "\t|\t" + ex), ex.Message);
            }
            catch (Exception ex)
            {
                App.Current.Logger.Error(ex, ex.Message);
            }
        });

        private System.Windows.Controls.TreeViewItem _selectedItem;

        #region Commands
        
        private RelayCommand _openedFileCommand;
        private RelayCommand _saveAsFileCommand;
        private RelayCommand _addRobotCommand;
        private RelayCommand _deleteRobotCommand;
        private RelayCommand _changePathOnPCCommand;

        private IAsyncCommand _generateProgramCommandAsync;
        private IAsyncCommand _retentionRunButtonCommandAsync;

        #endregion

        #endregion

        #region Public variables

        public bool IsVisibleGridLinesVisual3D { get => this._isVisibleGridLinesVisual3D; set => Set(ref this._isVisibleGridLinesVisual3D, value, false); }

        public Version Version { get => System.Reflection.Assembly.GetEntryAssembly().GetName().Version; }

        /// <summary>
        /// Выбранный файл
        /// </summary>
        public Model.File3D.File3D SelectedFile
        {
            get => this._selectedFile;
            set => Set(ref this._selectedFile, value, false);
        }

        /// <summary>
        /// Выбранный робот
        /// </summary>
        public Robot SelectedRobot { get => this._selectedRobot; set => Set(ref this._selectedRobot, value, false); }

        /// <summary>
        /// Имя выбранного робота для генерации
        /// </summary>
        public string SelectedRobotsName { get; set; }
        
        public object ActiveContent
        {
            get => this._activeContent;
            set
            {
                this._activeContent = value;
                this.UpdateSelectedDocument();
            }
        }

        /// <summary>
        /// Выбранный 3D объект
        /// </summary>
        public object SelectedObject
        {
            get => this._selectedObject;
            set
            {
                switch (value)
                {
                    case HelixToolkit.Wpf.GridLinesVisual3D gridLinesVisual3D:
                        return;

                    //case HelixToolkit.Wpf.BoundingBoxVisual3D boundingBoxVisual3D:
                    //    return;

                    case ForRobot.Model.File3D.Annotation annotation:
                        GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Libr.Behavior.FindElementByTagMessage((value as Model.File3D.Annotation).PropertyName));
                        break;

                    default:
                        Set(ref this._selectedObject, value, false);
                        break;
                }
            }
        }

        public System.Windows.Controls.TreeViewItem SelectedItem { get => this._selectedItem; set => Set(ref this._selectedItem, value, false); }

        #region Collections

        /// <summary>
        /// Коллекция типов скосов настила
        /// </summary>
        public ObservableCollection<string> ScoseTypesCollection
        {
            get
            {
                var Descriptions = typeof(ForRobot.Model.Detals.ScoseTypes).GetFields().Select(field => field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).SingleOrDefault() as System.ComponentModel.DescriptionAttribute);
                List<string> DescriptionList = Descriptions.Where(item => item != null).Select(item => item.Description).ToList<string>();
                return new ObservableCollection<string>(DescriptionList);
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
        /// Коллекция всех добаленнных роботов
        /// </summary>
        public ObservableCollection<Robot> RobotsCollection { get => this._robotsCollection; set => Set(ref this._robotsCollection, value); }
        /// <summary>
        /// Коллекция названияй роботов для генерации
        /// </summary>
        public ObservableCollection<string> RobotNamesCollection { get => new ObservableCollection<string>(new List<string>() { "Все" }.Union(this.RobotsCollection.Select(item => item.Name)).ToList<string>()); }
        /// <summary>
        /// Коллекция сообщений
        /// </summary>
        public ObservableCollection<Model.File3D.SceneItem> SceneItemsCollection { get => this._sceneItemsCollection; set => Set(ref this._sceneItemsCollection, value, false); }
        /// <summary>
        /// Коллекция сообщений
        /// </summary>
        public ObservableCollection<AppMessage> MessagesCollection { get => this._messagesCollection; set => Set(ref this._messagesCollection, value); }

        #endregion

        #region Commands

        /// <summary>
        /// Выгрузка макета
        /// </summary>
        public ICommand LoadedCommand { get; } = new RelayCommand(_ => Messenger.Default.Send(new Libr.Behavior.LoadLayoutMessage()));
        
        /// <summary>
        /// Сброс фокуса
        /// </summary>
        public ICommand LostFocusCommand { get; } = new RelayCommand(obj => LostFocus(obj as FrameworkElement));

        /// <summary>
        /// Создание файла
        /// </summary>
        public ICommand CreateNewFileCommand { get; } = new RelayCommand(_ => App.Current.WindowsAppService.OpenCreateWindow());

        /// <summary>
        /// Открытие файла программы
        /// </summary>
        public ICommand OpenedFileCommand { get => _openedFileCommand ?? (_openedFileCommand = new RelayCommand(_ => Open3DFile())); }

        /// <summary>
        /// Сохранение файла программы
        /// </summary>
        public ICommand SaveFileCommand { get; } = new RelayCommand(obj => SaveFile(obj as Model.File3D.File3D));

        /// <summary>
        /// Сохранение файла программы как
        /// </summary>
        public ICommand SaveAsFileCommand { get => _saveAsFileCommand ?? (_saveAsFileCommand = new RelayCommand(obj => SaveFileAs(obj as Model.File3D.File3D))); }

        /// <summary>
        /// Сохранение всех файлов
        /// </summary>
        public ICommand SaveAllFilesCommand { get; } = new RelayCommand(obj => SaveAllFile(obj as ObservableCollection<Model.File3D.File3D>));

        /// <summary>
        /// Сброс параметров детали до стандартных
        /// </summary>
        public ICommand StandartParametrsCommand { get; } = new RelayCommand(obj => GetStandartParametrs(obj as Model.File3D.File3D));

        /// <summary>
        /// Маштабирование модели
        /// </summary>
        public ICommand ZoomCommand { get; } = new RelayCommand(obj => ZoomViewPort(obj as double?));

        /// <summary>
        /// Показывает скрытые панели
        /// </summary>
        public ICommand CollapedCommand { get; } = new RelayCommand(obj => CollapedLayoutAnchorable(obj as string));

        /// <summary>
        /// Добавление робота
        /// </summary>
        public ICommand AddRobotCommand { get => this._addRobotCommand ?? (this._addRobotCommand = new RelayCommand(_ => this.AddNewRobot())); }

        /// <summary>
        /// Удаление робота
        /// </summary>
        public ICommand DeleteRobotCommand { get => _deleteRobotCommand ?? (_deleteRobotCommand = new RelayCommand(obj => this.DeleteRobot(obj as Robot))); }

        /// <summary>
        /// Повторное открытие соединения робота
        /// </summary>
        public ICommand ConnectedRobotCommand { get; } = new RelayCommand(obj => ReconnectionRobot(obj as Robot));

        /// <summary>
        /// Разрыв соединения с роботом
        /// </summary>
        public ICommand DisconnectedRobotCommand { get; } = new RelayCommand(obj => DisconnectedRobot(obj as Robot));

        /// <summary>
        /// Переименование робота
        /// </summary>
        public ICommand RenameRobotCommand { get; } = new RelayCommand(obj => RenameRobot(obj as Robot));

        /// <summary>
        /// Изменение папки робота
        /// </summary>
        public ICommand ChangePatnOnPCCommand { get => this._changePathOnPCCommand ?? (this._changePathOnPCCommand = new RelayCommand(_ => this.ChangePathPCFolder())); }

        /// <summary>
        /// Открытие окна настроек
        /// </summary>
        public ICommand PropertiesCommand { get; } = new RelayCommand(_ => App.Current.WindowsAppService.OpenPropertiesWindow());

        /// <summary>
        /// Открытие chm-справки
        /// </summary>
        public ICommand HelpCommand { get; } = new RelayCommand(_ => Help.ShowHelp(null, "Help/HelpManual.chm"));

        #region Async

        /// <summary>
        /// Команда генерации программы и её выбор на роботе/ах
        /// </summary>
        public ICommand GenerateProgramCommandAsync
        {
            get => this._generateProgramCommandAsync ?? (this._generateProgramCommandAsync = new AsyncRelayCommand(async _ => await Generation(), _exceptionCallback));
        }

        /// <summary>
        /// Команда запуска программы на роботе/ах
        /// </summary>
        public ICommand RunProgramCommandAsync { get; } = new AsyncRelayCommand(async obj => await RunSelectedProgram(obj as Robot), obj => CanProgramRun(obj as Robot), _exceptionCallback);

        /// <summary>
        /// Команда удержания кнопки запуска
        /// </summary>
        public ICommand RetentionRunButtonCommandAsync
        {
            get => _retentionRunButtonCommandAsync ??
                    (_retentionRunButtonCommandAsync = new AsyncRelayCommand(async obj =>
                    {
                        Robot robot = (Robot)obj;
                        await Task.Run(() => robot.RunCancelTokenSource?.Cancel());
                    }, _exceptionCallback));
        }

        /// <summary>
        /// Команда остановки программы на роботе/ах
        /// </summary>
        public ICommand PauseProgramCommandAsync { get; } = new AsyncRelayCommand(async obj => await StopProgramOnRobot(obj as Robot), obj => CanProgramStop(obj as Robot), _exceptionCallback);

        /// <summary>
        /// Аннулирование программы
        /// </summary>
        public IAsyncCommand CancelProgramCommandAsync { get; } = new AsyncRelayCommand(async obj => await CancelProgramOnRobot(obj as Robot), obj => CanProgramCancel(obj as Robot), _exceptionCallback);

        #endregion

        #endregion Commands

        #endregion Public variables

        #region Constructor

        public MainPageViewModel3()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            if (Properties.Settings.Default.SaveRobots == null)
                Properties.Settings.Default.SaveRobots = new System.Collections.Specialized.StringCollection();

            Libr.Logger.LoggingEvent += (s, o) =>
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => this.MessagesCollection.Add(new Model.AppMessage(o))));
            };

            if (Properties.Settings.Default.SaveRobots.Count > 0)
            {
                for (int i = 0; i < Properties.Settings.Default.SaveRobots.Count; i++)
                {
                    this.RobotsCollection.Add(this.GetNewRobot(JsonConvert.DeserializeObject<Robot>(Properties.Settings.Default.SaveRobots[i])));
                }
            }
            else
                this.RobotsCollection.Add(this.GetNewRobot());

            this.SelectedRobot = this.RobotsCollection[0];

            // Если нет открываемых файлов, проверяет - нужно ли создать файл детали.
            if (App.Current.OpenedFiles.Count == 0 && App.Current.Settings.CreatedDetalFile)
            {
                string programName = string.Empty;
                switch (App.Current.Settings.StartedDetalType)
                {
                    case string a when a == DetalTypes.Plita:
                        programName = App.Current.Settings.PlitaProgramName;
                        break;

                    case string b when b == DetalTypes.Stringer:
                        programName = App.Current.Settings.PlitaStringerProgramName;
                        break;

                    case string c when c == DetalTypes.Stringer:
                        programName = App.Current.Settings.PlitaTreugolnikProgramName;
                        break;
                }
                App.Current.OpenedFiles.Add(new Model.File3D.File3D(Detal.GetDetal(App.Current.Settings.StartedDetalType),
                                                                    Path.Combine(Path.GetTempPath(), programName)));
            }
        }

        #endregion

        #region Private functions

        #region Static

        private static bool CanProgramRun(Robot robot) => (robot.Pro_State == ProcessStatuses.Reset || robot.Pro_State == ProcessStatuses.End || robot.Pro_State == ProcessStatuses.Stop) && robot.IsConnection;
        private static bool CanProgramStop(Robot robot) => robot.Pro_State == ProcessStatuses.Active && robot.IsConnection;
        private static bool CanProgramCancel(Robot robot) => (robot.Pro_State == ProcessStatuses.Reset || robot.Pro_State == ProcessStatuses.End || robot.Pro_State == ProcessStatuses.Stop) && robot.IsConnection;

        /// <summary>
        /// Сброс фокуса на заданный элемент
        /// </summary>
        /// <param name="frameworkElement"></param>
        private static void LostFocus(FrameworkElement frameworkElement)
        {
            System.Windows.Input.Keyboard.ClearFocus();
            System.Windows.Input.FocusManager.SetFocusedElement(System.Windows.Input.FocusManager.GetFocusScope(frameworkElement), null);
        }

        private static void SaveFile(Model.File3D.File3D file)
        {
            if (file == null)
                return;

            if (!file.IsSaved)
                file.Save();
        }

        private static void SaveAllFile(IEnumerable<Model.File3D.File3D> files)
        {
            if (files == null)
                return;

            foreach (var file in files.Where(item => !item.IsSaved))
            {
                file.Save();
            }
        }

        /// <summary>
        /// Возврат к стандартным параметрам детали
        /// </summary>
        /// <param name="file">Выбранный файл</param>
        private static void GetStandartParametrs(Model.File3D.File3D file) => file.Detal = Model.File3D.File3D.StandartParamertrs(file.Detal);

        /// <summary>
        /// Приближение/отдаление камеры в HelixViewport3D
        /// </summary>
        /// <param name="step">Шаг приближения/отдаления</param>
        private static void ZoomViewPort(double? step = null)
        {
            if (step == null)
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Libr.Behavior.ZoomMessage());
            else
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Libr.Behavior.ZoomMessage((double)step));
        }

        /// <summary>
        /// Открытие скрытых панелей
        /// </summary>
        /// <param name="contentId"></param>
        private static void CollapedLayoutAnchorable(string contentId) => GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Libr.Behavior.CollapedLayoutAnchorableMessage(contentId));

        /// <summary>
        /// Повторное соединение с роботом
        /// </summary>
        /// <param name="robot"></param>
        private static void ReconnectionRobot(Robot robot) { if (!robot.IsConnection) robot.OpenConnection(); }

        /// <summary>
        /// Закрытие соединения с роботом
        /// </summary>
        /// <param name="robot"></param>
        private static void DisconnectedRobot(Robot robot) { if (robot.IsConnection) robot.CloseConnection(); }

        /// <summary>
        /// Переименование робота
        /// </summary>
        /// <param name="robot"></param>
        private static void RenameRobot(Robot robot)
        {
            using (ForRobot.Views.Windows.InputWindow _inputWindow = new ForRobot.Views.Windows.InputWindow("Введите новое название для робота") { Title = "Переименование робота" })
            {
                if (_inputWindow.ShowDialog() == true)
                {
                    robot.Name = _inputWindow.Answer;
                }
            }
        }

        #endregion Static
        
        private void SaveFileAs(Model.File3D.File3D file)
        {
            try
            {
                string filePath = this._fileDialogService.SaveFileDialog(null, file.Path, Exporters.Filter);
                if (filePath != null)
                {
                    _modelExporter.Export(file.CurrentModel, filePath);
                }
            }
            catch (Exception ex)
            {
                App.Current.Logger.Error(ex);
            }
        }

        /// <summary>
        /// Открытие уже существующего файла
        /// </summary>
        private void Open3DFile()
        {
            string filePath = this._fileDialogService.OpenFileDialog(null, null, Model.File3D.File3D.FilterForFileDialog + "|All Files|*.*", string.Empty);

            if (string.IsNullOrEmpty(filePath))
                return;

            var file = new Model.File3D.File3D(filePath);
            App.Current.OpenedFiles.Add(file);
            this.SelectedFile = file;
        }

        /// <summary>
        /// Добавление нового робота
        /// </summary>
        private void AddNewRobot()
        {
            this.RobotsCollection.Add(this.GetNewRobot());
            this.SelectedRobot = this.RobotsCollection.Last();
        }

        /// <summary>
        /// Удаление робота
        /// </summary>
        /// <param name="robot"></param>
        private void DeleteRobot(Robot robot)
        {
            if (robot == null)
                return;

            if (robot.Host == Robot.DefaultHost ||
                System.Windows.MessageBox.Show($"Удалить робота с соединением {robot.Host}:{robot.Port}?", robot.Name, MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                this.RobotsCollection.Remove(robot);
                if (this.RobotsCollection.Count > 0)
                {
                    this.SelectedRobot = this.RobotsCollection.Last();
                    this.SelectedRobotsName = this.RobotNamesCollection[0];
                }
            }
        }

        /// <summary>
        /// Изменение для роботов путя генерации на ПК
        /// </summary>
        private void ChangePathPCFolder()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    for (int i = 0; i < this.RobotsCollection.Count; i++)
                    {
                        this.RobotsCollection.ToList<Robot>()[i].PathProgramm = Path.Combine(fbd.SelectedPath, $"R{i + 1}");
                    }
                }
            }
        }

        /// <summary>
        /// Обновление выбранного LayoutDocumentPane.
        /// </summary>
        private void UpdateSelectedDocument()
        {
            if (ActiveContent is Model.File3D.File3D file)
            {
                this.SelectedFile = file;
                this.SelectedObject = null; // Снимает выделение с объекта HelixViewport3D.
            }
        }

        /// <summary>
        /// Запись детали в json файла
        /// </summary>
        /// <param name="detal"></param>
        /// <param name="path"></param>
        private void WriteJsonFile(Detal detal, string path)
        {
            JObject jObject = JObject.Parse(detal.Json);
            int[] sumRobots;
            if (this.SelectedRobotsName == "Все")
            {
                sumRobots = new int[this.RobotsCollection.Count];
                for (int i = 0; i < this.RobotsCollection.Count(); i++)
                {
                    sumRobots[i] = i + 1;
                }
            }
            else
                sumRobots = new int[1] { this.RobotsCollection.IndexOf(this.RobotsCollection.Where(p => p.Name == this.SelectedRobotsName).ToArray()[0]) + 1 };
            jObject.Add("robots", JToken.FromObject(sumRobots)); // Запись в json-строку выбранных для генерации роботов (не зависит от подключения).

            switch (detal)
            {
                case Plita p:
                    var plita = (Plita)detal;
                    var sch = WeldingSchemas.GetSchema(plita.WeldingSchema);
                    jObject.Add("welding_sequence", JToken.FromObject(sch)); // Запись в json-строку схему сварки настила.
                    break;
            }

            File.WriteAllText(path, jObject.ToString());

            if (File.Exists(path))
                App.Current.Logger.Info(new Exception($"Содержание файла {path}:\n" + jObject.ToString()),
                                        $"Сгенерирован файл {path}");
        }

        /// <summary>
        /// Возврат робота с инициализированными собитиями и открытым соединением
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        private Robot GetNewRobot(Robot robot = null)
        {
            if (robot == null)
            {
                robot = new Robot()
                {
                    PathProgramm = (this.RobotsCollection.Count > 0) ?
                            Path.Combine(Directory.GetParent(this.RobotsCollection.Last().PathProgramm).ToString(), $"R{this.RobotsCollection.Count + 1}")
                            : Path.Combine(App.Current.Settings.PathFolderOfGeneration, $"R{this.RobotsCollection.Count + 1}"),
                    PathControllerFolder = App.Current.Settings.ControlerFolder,
                    ConnectionTimeOutMilliseconds = Convert.ToInt32(App.Current.Settings.ConnectionTimeOut) * 1000
                };
            }

            if (string.IsNullOrEmpty(robot.Name))
                robot.Name = $"Соединение {this.RobotsCollection.Count + 1}";

            robot.Log += new EventHandler<ForRobot.Libr.LogEventArgs>(this.WreteLog);
            robot.LogError += new EventHandler<ForRobot.Libr.LogErrorEventArgs>(WreteLogError);

            robot.ChangeRobot += (s, e) =>
            {
                Properties.Settings.Default.SaveRobots.Clear();
                foreach (var r in this.RobotsCollection)
                {
                    Properties.Settings.Default.SaveRobots.Add(r.Json);
                    Properties.Settings.Default.Save();
                }
            };
            return robot;
        }
                
        #region Async

        private async Task Generation()
        {
            string foldForGenerate = Directory.GetParent(this.RobotsCollection.First().PathProgramm).ToString(); // Путь для генерации скриптом.

            string programName = this._programName;

            if (App.Current.Settings.AskNameFile)
            {
                ForRobot.Views.Windows.InputWindow inputWindow = new ForRobot.Views.Windows.InputWindow("Введите имя генерируемой программы:");
                inputWindow.ShowDialog();

                if (string.IsNullOrEmpty(inputWindow.Answer))
                    return;
                else
                    programName = inputWindow.Answer;
            }

            // Запись Json-файла.
            this.WriteJsonFile(this.SelectedFile.Detal, Path.Combine(foldForGenerate, $"{programName}.json"));

            // Генерация программы.
            switch (this.SelectedFile.Detal)
            {
                case Plita plita:
                    App.Current.Logger.Info("Начат процесс генерации программы для плиты с рёбрами . . .");
                    break;

                case PlitaStringer plitaStringer:
                    App.Current.Logger.Info("Начат процесс генерации программы для плиты со стрингером . . .");
                    break;

                case PlitaTreygolnik plitaTreygolnik:
                    App.Current.Logger.Info("Начат процесс генерации программы для плиты треугольником . . .");
                    break;
            }

            new GenerationService(foldForGenerate, programName, this._scriptName).Start(this.SelectedFile.Detal);

            // Проверка успеха генерации
            if (this.SelectedRobotsName == "Все")
            {
                for (int i = 0; i < this.RobotsCollection.Count(); i++)
                {
                    string robotPath = this.RobotsCollection[i].PathProgramm;

                    if (File.Exists(Path.Combine(robotPath, string.Join("", programName, ".src"))))
                        App.Current.Logger.Info($"Файл {string.Join("", programName, ".src")} сгенерирован в {robotPath}");
                    else
                        App.Current.Logger.Error($"Файл {Path.Combine(robotPath, string.Join("", programName, ".src"))} не найден");
                }
            }
            else
            {
                string robotPath = this.RobotsCollection.Where(item => item.Name == this.SelectedRobotsName).First().PathProgramm;

                if (File.Exists(Path.Combine(robotPath, string.Join("", programName, ".src"))))
                    App.Current.Logger.Info($"Файл {string.Join("", programName, ".src")} сгенерирован в {robotPath}");
                else
                    App.Current.Logger.Error($"Файл {Path.Combine(robotPath, string.Join("", programName, ".src"))} не найден");
            }

            if (!App.Current.Settings.SendingGeneratedFiles)
                //|| System.Windows.MessageBox.Show("Отправить сгенерированные файлы на робота/ов?\nВсе файлы в папке назначения будут удалены.", "Генерация", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, System.Windows.MessageBoxOptions.DefaultDesktopOnly) != MessageBoxResult.OK)
                return;

            if (this.RobotsCollection.Count > 0 && string.IsNullOrWhiteSpace(this.RobotsCollection[0].PathProgramm))
                throw new Exception("Отказ в передачи файлов: не выбрана папка программы.");

            if (this.SelectedRobotsName == "Все")
            {
                foreach (var robot in this.RobotsCollection)
                {
                    await this.CopyFileOnRobot(robot);
                }
            }
            else
            {
                await this.CopyFileOnRobot(this.RobotsCollection.Where(p => p.Name == this.SelectedRobotsName).Select(item => item).ToList<Robot>().First());
            }
        }

        /// <summary>
        /// Передача файлов на робота
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        private async Task CopyFileOnRobot(Robot robot)
        {
            if (!robot.IsConnection)
            {
                App.Current.Logger.Error($"{robot.Host}:{robot.Port}\tОтказ в передачи файлов: отсутствует соединение");
                return;
            }

            await Task.Run<bool>(() => robot.DeleteFileOnPC());

            if (!await Task.Run<bool>(() => robot.CopyToPC(string.Join("", this._programName, ".src"))))
                return;

            await robot.GetFilesAsync();

            for (int i = 0; i < robot.Files.Children.Count; i++)
            {
                var item = robot.Files[i];
                var folder = item.Search(this.RobotsCollection[i].PathControllerFolder.Split(new char[] { '\\' }).Last());

                if (folder == null)
                    continue;

                foreach (var child in folder.Children.Where(f => f.Type == Model.Controls.FileTypes.DataList || f.Type == Model.Controls.FileTypes.Program))
                {
                    await Task.Run(() => this.RobotsCollection[i].DeleteFile(child.Path));
                }
            }

            await robot.GetFilesAsync();

            if (!await Task.Run<bool>(() => robot.Copy(this._programName)))
                return;

            await Task.Run(() => robot.SelectProgramByName(string.Join("", this._programName, ".src")));

            await robot.GetFilesAsync();
        }

        /// <summary>
        /// Запуск выбранной на роботе программы
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        private static async Task RunSelectedProgram(Robot robot)
        {
            robot.RunCancelTokenSource = new System.Threading.CancellationTokenSource();
            await robot.PeriodicTask(async () => await Task.Run(() => robot.Run()), new TimeSpan(0, 0, 0, 0, 1000), robot.RunCancelTokenSource.Token);
        }

        /// <summary>
        /// Остановка программы на роботе
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        private static async Task StopProgramOnRobot(Robot robot) => await Task.Run(() => robot.Pause());

        /// <summary>
        /// Аннулирование программы на роботе
        /// </summary>
        /// <param name="robot"></param>
        /// <returns></returns>
        private static async Task CancelProgramOnRobot(Robot robot)
        {
            if (robot.Pro_State == ProcessStatuses.Active && System.Windows.MessageBox.Show(string.Format("Прервать выполнение программы {0}?", robot.RobotProgramName), robot.Name, 
                                                                                            MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK,
                                                                                            System.Windows.MessageBoxOptions.DefaultDesktopOnly) != MessageBoxResult.OK)
                return;

            await Task.Run(() => robot.Cancel());
        }

        #endregion Async

        #region Logging

        /// <summary>
        /// Системное сообщение
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WreteLog(object sender, LogEventArgs e) => App.Current.Logger.Info(e.Message);

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WreteLogError(object sender, LogErrorEventArgs e) => App.Current.Logger.Error(e.Exception, e.Message);

        #endregion

        #endregion

        #region Public functions

        public void Select(Visual3D visual) => this.SelectedObject = visual;

        #endregion
    }
}

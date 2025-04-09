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

        //private ForRobot.Libr.ConfigurationProperties.RobotConfigurationSection RobotConfig { get; set; } = ConfigurationManager.GetSection("robot") as ForRobot.Libr.ConfigurationProperties.RobotConfigurationSection;

        private ObservableCollection<Model.File3D.SceneItem> _sceneItemsCollection = new ObservableCollection<Model.File3D.SceneItem>();
        private ObservableCollection<AppMessage> _messagesCollection = new ObservableCollection<AppMessage>();

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
        
        //private RelayCommand _createNewFileCommand;
        private RelayCommand _openedFileCommand;
        private RelayCommand _saveFileCommand;
        private RelayCommand _saveAsFileCommand;
        private RelayCommand _saveAllFilesCommand;
        private RelayCommand _standartParametrsCommand;
        private RelayCommand _zoomCommand;
        private RelayCommand _сollapedCommand;

        //private RelayCommand _closeFileCommand;
        private RelayCommand _addRobotCommand;
        private RelayCommand _deleteRobotCommand;
        private RelayCommand _connectedRobotCommand;
        private RelayCommand _disconnectedRobotCommand;
        private RelayCommand _renameRobotCommand;
        private RelayCommand _changePathOnPCCommand;
        private RelayCommand _upDateFilesCommand;
        private RelayCommand _selectFolderCommand;
        //private RelayCommand _retentionRunButtonCommand;
        private RelayCommand _propertiesCommand;
        private RelayCommand _helpCommand;

        private IAsyncCommand _generateProgramCommandAsync;
        private IAsyncCommand _selectFileCommandAsunc;
        private IAsyncCommand _deleteFileCommandAsync;
        private IAsyncCommand _runProgramCommandAsync;
        private IAsyncCommand _retentionRunButtonCommandAsync;

        private IAsyncCommand _pauseProgramCommand;
        private IAsyncCommand _cancelProgramCommand;
        private IAsyncCommand _dropFilesCommandAsync;

        #endregion

        #endregion

        #region Public variables

        //public IHelixViewport3D Viewport { get; set; }

        public Version Version { get => System.Reflection.Assembly.GetEntryAssembly().GetName().Version; }

        /// <summary>
        /// Отправляются ли сгенерированные файлы на робота/ов
        /// </summary>
        public bool SendingGeneratedFiles
        {
            get => Properties.Settings.Default.SendingGeneratedFiles;
            set
            {
                Properties.Settings.Default.SendingGeneratedFiles = value;
                Properties.Settings.Default.Save();
                RaisePropertyChanged(nameof(this.SendingGeneratedFiles));
            }
        }

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

                    case ForRobot.Model.File3D.Annotation annotation:
                        GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Libr.Behavior.FindElementByTagMessage((value as Model.File3D.Annotation).PropertyName));
                        break;

                    default:
                        Set(ref this._selectedObject, value);
                        break;
                }
            }
        }

        public System.Windows.Controls.TreeViewItem SelectedItem {
            get => this._selectedItem;
            set => Set(ref this._selectedItem, value); }

        #region Collections

        /// <summary>
        /// Коллекция типов скосов настила
        /// </summary>
        public ObservableCollection<string> ScoseTypesCollection
        {
            get
            {
                var Descriptions = typeof(ForRobot.Model.Detals.ScoseTypes).GetFields().Select(field => field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).SingleOrDefault() as System.ComponentModel.DescriptionAttribute);
                //var Descriptions = typeof(ForRobot.Model.Detals.ScoseTypes).GetFields().Where(field => (field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).SingleOrDefault() as System.ComponentModel.DescriptionAttribute).Description == "Прямоугольник");
                List<string> DescriptionList = Descriptions.Where(item => item != null).Select(item => item.Description).ToList<string>();
                //return typeof(ForRobot.Model.Detals.ScoseTypes).GetFields().ToList();
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
        public ObservableCollection<Model.File3D.SceneItem> SceneItemsCollection { get => this._sceneItemsCollection; set => Set(ref this._sceneItemsCollection, value); }

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
        public ICommand CreateNewFileCommand { get; } = new RelayCommand(_ => OpenCreateFileWindow());

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
        //{
        //    get
        //    {
        //        return _standartParametrsCommand ??
        //            (_standartParametrsCommand = new RelayCommand(obj =>
        //            {
        //                ForRobot.Model.File3D.File3D file = obj as ForRobot.Model.File3D.File3D;
        //                Detal detal = file.Detal;

        //                switch (detal.DetalType)
        //                {
        //                    case string a when a == DetalTypes.Plita:
        //                        file.Detal = new Plita(DetalType.Plita)
        //                        {
        //                            ScoseType = ((Plita)detal).ScoseType,
        //                            DiferentDistance = ((Plita)detal).DiferentDistance,
        //                            ParalleleRibs = ((Plita)detal).ParalleleRibs,
        //                            DiferentDissolutionLeft = ((Plita)detal).DiferentDissolutionLeft,
        //                            DiferentDissolutionRight = ((Plita)detal).DiferentDissolutionRight
        //                        };

        //                        file.CurrentModel.Children.Add(Model.File3D.File3D.GetModel3D((Plita)file.Detal));
        //                        file.ModelChangedEvent += (s, o) => file.CurrentModel.Children.Add(Model.File3D.File3D.GetModel3D((s as ForRobot.Model.File3D.File3D).Detal as Plita));


        //                        //this.SelectedFile.Detal.ChangeProperty += (s, o) =>
        //                        //{
        //                        //    //Task.Run(() => { this.CurrentModel = Plita.GetModel3D((Plita)s); });
        //                        //    this.SelectedFile.CurrentModel = Plita.GetModel3D((Plita)s);
        //                        //};
        //                        //((Plita)this.SelectedFile.Detal).RibsCollection.ItemPropertyChanged += (s, o) =>
        //                        //{
        //                        //    this.SelectedFile.CurrentModel = Plita.GetModel3D((Plita)s);
        //                        //};
        //                        //this.SelectedFile.CurrentModel = Plita.GetModel3D((Plita)this.SelectedFile.Detal);
        //                        break;

        //                    case string b when b == DetalTypes.Stringer:
        //                        file.Detal = new PlitaStringer(DetalType.Stringer);
        //                        break;

        //                    case string c when c == DetalTypes.Treygolnik:
        //                        file.Detal = new PlitaTreygolnik(DetalType.Treygolnik);
        //                        break;
        //                }
        //                RaisePropertyChanged(nameof(this.SelectedFile.Detal));
        //            }));
        //    }
        //}

        /// <summary>
        /// Маштабирование модели
        /// </summary>
        public RelayCommand ZoomCommand
        {
            get
            {
                return _zoomCommand ??
                    (_zoomCommand = new RelayCommand(obj =>
                    {
                        if (obj == null)
                            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Libr.Behavior.ZoomMessage());
                        else
                            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Libr.Behavior.ZoomMessage((double)obj));
                    }));
            }
        }

        /// <summary>
        /// Показывает скрытые панели
        /// </summary>
        public RelayCommand CollapedCommand
        {
            get
            {
                return _сollapedCommand ??
                    (_сollapedCommand = new RelayCommand(obj =>
                    {
                        GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Libr.Behavior.CollapedLayoutAnchorableMessage((string)obj));
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
                        this.RobotsCollection.Add(this.GetNewRobot());
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
                        if (robot.Host == Robot.DefaultHost ||
                            System.Windows.MessageBox.Show($"Удалить робота с соединением {robot.Host}:{robot.Port}?", robot.Name, MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                        {
                            this.RobotsCollection.Remove(robot);
                            if (this.RobotsCollection.Count > 0)
                            {
                                this.SelectedRobot = this.RobotsCollection.Last();
                                this.SelectedRobotsName = this.RobotNamesCollection[0];
                            }
                            RaisePropertyChanged(nameof(this.RobotNamesCollection));
                        }
                    }));
            }
        }

        /// <summary>
        /// Повторное открытие соединения робота
        /// </summary>
        public RelayCommand ConnectedRobotCommand
        {
            get
            {
                return _connectedRobotCommand ??
                    (_connectedRobotCommand = new RelayCommand(obj =>
                    {
                        Robot robot = (Robot)obj;
                        robot.OpenConnection();
                    }));
            }
        }

        /// <summary>
        /// Разрыв соединения с роботом
        /// </summary>
        public RelayCommand DisconnectedRobotCommand
        {
            get
            {
                return _disconnectedRobotCommand ??
                    (_disconnectedRobotCommand = new RelayCommand(obj =>
                    {
                        Robot robot = (Robot)obj;
                        robot.CloseConnection();
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
                                for (int i = 0; i < this.RobotsCollection.Count; i++)
                                {
                                    this.RobotsCollection.ToList<Robot>()[i].PathProgramm = Path.Combine(fbd.SelectedPath, $"R{i + 1}");
                                }
                            }
                        }
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

        ///// <summary>
        ///// Команда удержания кнопки запуска
        ///// </summary>
        //public RelayCommand RetentionRunButtonCommand
        //{
        //    get
        //    {
        //        return _retentionRunButtonCommand ??
        //            (_retentionRunButtonCommand = new RelayCommand(obj =>
        //            {
        //                Robot robot = (Robot)obj;
        //                robot.RunCancelTokenSource.Cancel();
        //            }));
        //    }
        //}

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

        #region Async

        /// <summary>
        /// Команда генерации программы и её выбор на роботе/ах
        /// </summary>
        public IAsyncCommand GenerateProgramCommandAsync
        {
            get
            {
                return _generateProgramCommandAsync ??
                    (_generateProgramCommandAsync = new AsyncRelayCommand(async obj =>
                    {
                        // Сброс фокуса перед генерацией.
                        LostFocus(obj as FrameworkElement);

                        string foldForGenerate = Directory.GetParent(this.RobotsCollection.First().PathProgramm).ToString(); // Путь для генерации скриптом.

                        // Запись Json-файла
                        this.WriteJsonFile(this.SelectedFile.Detal, Path.Combine(foldForGenerate, $"{this._programName}.json"));

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

                        new GenerationService(foldForGenerate, this._programName, this._scriptName).Start(this.SelectedFile.Detal);

                        // Проверка успеха генерации
                        if (this.SelectedRobotsName == "Все")
                        {
                            for (int i = 0; i < this.RobotsCollection.Count(); i++)
                            {
                                string robotPath = this.RobotsCollection[i].PathProgramm;

                                if (File.Exists(Path.Combine(robotPath, string.Join("", this._programName, ".src"))))
                                    App.Current.Logger.Info($"Файл {string.Join("", this._programName, ".src")} сгенерирован в {robotPath}");
                                else
                                    App.Current.Logger.Error($"Файл {Path.Combine(robotPath, string.Join("", this._programName, ".src"))} не найден");
                            }
                        }
                        else
                        {
                            if (!File.Exists(Path.Combine(this.RobotsCollection.Where(item => item.Name == this.SelectedRobotsName).First().PathProgramm, string.Join("", this._programName, ".src"))))
                                return;
                        }

                        if (System.Windows.MessageBox.Show("Отправить сгенерированные файлы на робота/ов?\nВсе файлы в папке назначения будут удалены.", "Генерация", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK, System.Windows.MessageBoxOptions.DefaultDesktopOnly) != MessageBoxResult.OK)
                            return;

                        if (this.RobotsCollection.Count > 0 && string.IsNullOrWhiteSpace(this.RobotsCollection[0].PathProgramm))
                            throw new Exception("Отказ в передачи файлов: не выбрана папка программы.");

                        if (this.SelectedRobotsName == "Все")
                        {
                            foreach(var robot in this.RobotsCollection)
                            {
                                await this.CopyFileOnRobot(robot);
                            }
                        }
                        else
                        {
                            await this.CopyFileOnRobot(this.RobotsCollection.Where(p => p.Name == this.SelectedRobotsName).Select(item => item).ToList<Robot>().First());
                        }
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
                return _selectFileCommandAsunc ??
                    (_selectFileCommandAsunc = new AsyncRelayCommand(async obj =>
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
                return _deleteFileCommandAsync ??
                    (_deleteFileCommandAsync = new AsyncRelayCommand(async obj =>
                    {
                        List<string> checkedFiles;
                        if (obj == null)
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

                                    foreach (var f in files)
                                        stack.Push(f);
                                }
                                while (stack.Count > 0);
                            }
                        }
                        else
                            checkedFiles = new List<string>() { obj as string };

                        var task = Task.Run(async () => 
                        {
                            foreach(string path in checkedFiles)
                            {
                                await Task.Run(() => this.SelectedRobot.DeleteFile(Path.Combine(ForRobot.Libr.Client.JsonRpcConnection.DefaulRoot, path)));
                            }
                        });
                        await Task.WhenAll(task);
                    }, _exceptionCallback));
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
                        robot.RunCancelTokenSource = new System.Threading.CancellationTokenSource();
                        await robot.PeriodicTask(async () => {
                            if (robot.Pro_State == "#P_RESET" || robot.Pro_State == "#P_END" || robot.Pro_State == "#P_STOP")
                            {
                                await Task.Run(() => robot.Run());
                            }
                        },
                        new TimeSpan(0, 0, 0, 0, 1000),
                        robot.RunCancelTokenSource.Token);
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
                        Robot robot = (Robot)obj;
                        await Task.Run(() => robot.RunCancelTokenSource.Cancel());
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
        /// Команда отправки файлов на робота/ов
        /// </summary>
        public IAsyncCommand DropFilesCommandAsync
        {
            get
            {
                return _dropFilesCommandAsync ??
                  (_dropFilesCommandAsync = new AsyncRelayCommand(async obj =>
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

                      foreach (var file in this.SelectedRobot.Files)
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

            if (App.Current.OpenedFiles.Count == 0)
            {
                App.Current.OpenedFiles.Add(new Model.File3D.File3D(Detal.GetDetal(DetalTypes.Plita),
                                                                    Path.Combine(Path.GetTempPath(), App.Current.Settings.PlitaProgramName)));
            }
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Сброс фокуса на заданный элемент
        /// </summary>
        /// <param name="frameworkElement"></param>
        private static void LostFocus(FrameworkElement frameworkElement)
        {
            System.Windows.Input.Keyboard.ClearFocus();
            System.Windows.Input.FocusManager.SetFocusedElement(System.Windows.Input.FocusManager.GetFocusScope(frameworkElement), null);
        }

        private static void OpenCreateFileWindow()
        {
            if (object.Equals(App.Current.CreateWindow, null))
            {
                App.Current.CreateWindow = new Views.Windows.CreateWindow();
                App.Current.CreateWindow.Closed += (a, b) => App.Current.CreateWindow = null;
                App.Current.CreateWindow.Owner = App.Current.MainWindowView;
                App.Current.CreateWindow.Show();
            }
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

        private static void GetStandartParametrs(Model.File3D.File3D file)
        {
            Detal detal = file.Detal;

            switch (detal.DetalType)
            {
                case string a when a == DetalTypes.Plita:
                    file.Detal = new Plita(DetalType.Plita)
                    {
                        ScoseType = ((Plita)detal).ScoseType,
                        DiferentDistance = ((Plita)detal).DiferentDistance,
                        ParalleleRibs = ((Plita)detal).ParalleleRibs,
                        DiferentDissolutionLeft = ((Plita)detal).DiferentDissolutionLeft,
                        DiferentDissolutionRight = ((Plita)detal).DiferentDissolutionRight
                    };

                    file.CurrentModel.Children.Add(Model.File3D.File3D.GetModel3D((Plita)file.Detal));
                    file.ModelChangedEvent += (s, o) => file.CurrentModel.Children.Add(Model.File3D.File3D.GetModel3D((s as ForRobot.Model.File3D.File3D).Detal as Plita));
                    break;

                case string b when b == DetalTypes.Stringer:
                    file.Detal = new PlitaStringer(DetalType.Stringer);
                    break;

                case string c when c == DetalTypes.Treygolnik:
                    file.Detal = new PlitaTreygolnik(DetalType.Treygolnik);
                    break;
            }


            //switch (detal.DetalType)
            //{
            //    case string a when a == DetalTypes.Plita:
            //        file.Detal = new Plita(DetalType.Plita)
            //        {
            //            ScoseType = ((Plita)detal).ScoseType,
            //            DiferentDistance = ((Plita)detal).DiferentDistance,
            //            ParalleleRibs = ((Plita)detal).ParalleleRibs,
            //            DiferentDissolutionLeft = ((Plita)detal).DiferentDissolutionLeft,
            //            DiferentDissolutionRight = ((Plita)detal).DiferentDissolutionRight
            //        };

            //        file.CurrentModel.Children.Add(Model.File3D.File3D.GetModel3D((Plita)file.Detal));
            //        file.ModelChangedEvent += (s, o) => file.CurrentModel.Children.Add(Model.File3D.File3D.GetModel3D((s as ForRobot.Model.File3D.File3D).Detal as Plita));


            //        //this.SelectedFile.Detal.ChangeProperty += (s, o) =>
            //        //{
            //        //    //Task.Run(() => { this.CurrentModel = Plita.GetModel3D((Plita)s); });
            //        //    this.SelectedFile.CurrentModel = Plita.GetModel3D((Plita)s);
            //        //};
            //        //((Plita)this.SelectedFile.Detal).RibsCollection.ItemPropertyChanged += (s, o) =>
            //        //{
            //        //    this.SelectedFile.CurrentModel = Plita.GetModel3D((Plita)s);
            //        //};
            //        //this.SelectedFile.CurrentModel = Plita.GetModel3D((Plita)this.SelectedFile.Detal);
            //        break;

            //    case string b when b == DetalTypes.Stringer:
            //        file.Detal = new PlitaStringer(DetalType.Stringer);
            //        break;

            //    case string c when c == DetalTypes.Treygolnik:
            //        file.Detal = new PlitaTreygolnik(DetalType.Treygolnik);
            //        break;
            //}
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

        /// <summary>
        /// Обновление выбранного LayoutDocumentPane.
        /// </summary>
        private void UpdateSelectedDocument ()
        {
            // Фильтруем только LayoutDocument
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

        private async Task CopyFileOnRobot(Robot robot)
        {
            if (!robot.IsConnection)
                App.Current.Logger.Error($"{robot.Host}:{robot.Port}\tОтказ в передачи файлов: отсутствует соединение");

            await Task.Run<bool>(() => robot.DeleteFileOnPC());

            if (!await Task.Run<bool>(() => robot.CopyToPC(string.Join("", this._programName, ".src"))))
                return;

            await robot.GetFilesAsync();

            for (int i = 0; i < robot.Files.Count; i++)
            {
                var item = robot.Files[i];
                var folder = item.Search(this.RobotsCollection[i].PathControllerFolder.Split(new char[] { '\\' }).Last());

                if (folder == null)
                    continue;

                foreach (var child in folder.Children.Where(f => f.Type == Model.Controls.FileTypes.DataList || f.Type == Model.Controls.FileTypes.Program))
                {
                    await Task.Run(() => this.RobotsCollection[i].DeleteFile(Path.Combine(ForRobot.Libr.Client.JsonRpcConnection.DefaulRoot, child.Path)));
                }
            }

            if (!await Task.Run<bool>(() => robot.Copy(this._programName)))
                return;

            await Task.Run(() => robot.SelectProgramByName(string.Join("", this._programName, ".src")));
        }

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

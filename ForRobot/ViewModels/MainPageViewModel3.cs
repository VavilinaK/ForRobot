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

namespace ForRobot.ViewModels
{
    public class MainPageViewModel3 : BaseClass
    {
        #region Private variables

        private Robot _selectedRobot;

        private ObservableCollection<Robot> _robotsCollection = new ObservableCollection<Robot>();

        private ForRobot.Libr.ConfigurationProperties.RobotConfigurationSection RobotConfig { get; set; } = ConfigurationManager.GetSection("robot") as ForRobot.Libr.ConfigurationProperties.RobotConfigurationSection;

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

        #region Commands

        private RelayCommand _createNewFileCommand;
        private RelayCommand _closeFileCommand;
        private RelayCommand _addRobotCommand;
        private RelayCommand _deleteRobotCommand;
        private RelayCommand _connectedRobotCommand;
        private RelayCommand _disconnectedRobotCommand;
        private RelayCommand _renameRobotCommand;
        private RelayCommand _changePathOnPCCommand;
        private RelayCommand _upDateFilesCommand;
        private RelayCommand _selectFolderCommand;
        private RelayCommand _retentionRunButtonCommand;
        private RelayCommand _propertiesCommand;

        private IAsyncCommand _selectFileCommandAsunc;
        private IAsyncCommand _deleteFileCommandAsync;
        private IAsyncCommand _runProgramCommandAsync;
        private IAsyncCommand _pauseProgramCommand;
        private IAsyncCommand _cancelProgramCommand;
        private IAsyncCommand _dropFilesCommandAsync;

        #endregion

        #endregion

        #region Public variables

        public Version Version { get => System.Reflection.Assembly.GetEntryAssembly().GetName().Version; }

        //public Libr.Settings.Settings Settings
        //{
        //    get => ForRobot.App.Settings;
        //    set
        //    {
        //        ForRobot.App.Settings = value;
        //        RaisePropertyChanged(nameof(this.Settings));
        //    }
        //}

        public bool VisibilityAxis { get; set; } = true;

        /// <summary>
        /// Выбранный робот
        /// </summary>
        public Robot SelectedRobot { get => this._selectedRobot; set => Set(ref this._selectedRobot, value); }

        /// <summary>
        /// Имя выбранного робота для генерации
        /// </summary>
        public string SelectedRobotsName { get; set; }

        #region Collections

        ///// <summary>
        ///// Коллекция возможных схем сварки
        ///// </summary>
        //public ObservableCollection<string> WeldingSchemaCollection
        //{
        //    get
        //    {
        //        var Descriptions = typeof(ForRobot.Model.Detals.WeldingSchemas.SchemasTypes).GetFields().Select(field => field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).SingleOrDefault() as System.ComponentModel.DescriptionAttribute);
        //        List<string> DescriptionList = Descriptions.Where(item => item != null).Select(item => item.Description).ToList<string>();
        //        return new ObservableCollection<string>(DescriptionList);
        //    }
        //}

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
        public ObservableCollection<AppMessage> MessagesCollection { get => this._messagesCollection; set => Set(ref this._messagesCollection, value); }

        #endregion

        #region Commands

        /// <summary>
        /// Создание файла
        /// </summary>
        public RelayCommand CreateNewFileCommand
        {
            get
            {
                return _createNewFileCommand ??
                    (_createNewFileCommand = new RelayCommand(obj =>
                    {
                        if (object.Equals(App.Current.CreateWindow, null))
                        {
                            App.Current.CreateWindow = new Views.Windows.CreateWindow();
                            App.Current.CreateWindow.Closed += (a, b) => App.Current.CreateWindow = null;
                            App.Current.CreateWindow.Owner = App.Current.MainWindowView;
                            App.Current.CreateWindow.Show();
                        }
                    }));
            }
        }

        /// <summary>
        /// Закрытие вкладки файла
        /// </summary>
        public RelayCommand CloseFileCommand
        {
            get
            {
                return _closeFileCommand ??
                    (_closeFileCommand = new RelayCommand(obj =>
                    {
                        App.Current.OpenedFiles.Remove(obj as Model.File3D.File3D);
                        //System.Windows.Controls.TabControl tabControl = (System.Windows.Controls.TabControl)(obj as object[])[0];
                        //System.Windows.Controls.TabItem tabItem = (System.Windows.Controls.TabItem)(obj as object[])[1];

                        //tabControl.Items.Remove(tabItem);
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

        /// <summary>
        /// Команда удержания кнопки запуска
        /// </summary>
        public RelayCommand RetentionRunButtonCommand
        {
            get
            {
                return _retentionRunButtonCommand ??
                    (_retentionRunButtonCommand = new RelayCommand(obj =>
                    {
                        Robot robot = (Robot)obj;
                        robot.RunCancelTokenSource.Cancel();
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

        #region Async

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

        #endregion

        #endregion

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
        }

        #endregion

        #region Private functions

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
                            : Path.Combine(this.RobotConfig.PathForGeneration, $"R{this.RobotsCollection.Count + 1}"),
                    PathControllerFolder = this.RobotConfig.PathControllerFolder,
                    ConnectionTimeOutMilliseconds = Convert.ToInt32(App.Current.Settings.ConnectionTimeOut) * 1000
                };
            }

            if (string.IsNullOrEmpty(robot.Name))
                robot.Name = $"Соединение {this.RobotsCollection.Count + 1}";

            robot.Log += new EventHandler<ForRobot.Libr.LogEventArgs>(this.WreteLog);
            robot.LogError += new EventHandler<ForRobot.Libr.LogErrorEventArgs>(WreteLogError);

            Task.Run(() => robot.OpenConnection());

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

        #endregion
    }
}

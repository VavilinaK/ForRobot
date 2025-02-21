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
        
        #region Commands

        private RelayCommand _createNewFileCommand;
        private RelayCommand _closeFileCommand;
        private RelayCommand _addRobotCommand;
        private RelayCommand _deleteRobotCommand;
        private RelayCommand _connectedRobotCommand;
        private RelayCommand _disconnectedRobotCommand;

        #endregion

        #endregion

        #region Public variables

        public Version Version { get => System.Reflection.Assembly.GetEntryAssembly().GetName().Version; }

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
                        System.Windows.Controls.TabControl tabControl = (System.Windows.Controls.TabControl)(obj as object[])[0];
                        System.Windows.Controls.TabItem tabItem = (System.Windows.Controls.TabItem)(obj as object[])[1];

                        tabControl.Items.Remove(tabItem);
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
                        if ( System.Windows.MessageBox.Show($"Удалить робота с соединением {robot.Host}:{robot.Port}?", robot.Name, MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
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
                        //robot.OpenConnection();
                    }));
            }
        }

        #endregion

        #endregion

        #region Constructor

        public MainPageViewModel3()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            if (Properties.Settings.Default.SaveRobots == null)
                Properties.Settings.Default.SaveRobots = new System.Collections.Specialized.StringCollection();

            //App.Current.Log += new EventHandler<LogEventArgs>(SelectAppLogger);

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
                    PathControllerFolder = this.RobotConfig.PathControllerFolder
                };
            }

            if (string.IsNullOrEmpty(robot.Name))
                robot.Name = $"Соединение {this.RobotsCollection.Count + 1}";

            //robot.Log += new EventHandler<ForRobot.Libr.LogEventArgs>(this.WreteLog);
            //robot.LogError += new EventHandler<ForRobot.Libr.LogErrorEventArgs>(WreteLogError);
            //Task.Run(() => robot.OpenConnection(this.ConnectionTimeOut));
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

        #endregion

        #region Public functions

        #endregion
    }
}

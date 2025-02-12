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

        #region Commands

        private RelayCommand _createNewFileCommand;
        private RelayCommand _closeFileCommand;

        #endregion

        #endregion

        #region Public variables

        public Version Version { get => System.Reflection.Assembly.GetEntryAssembly().GetName().Version; }

        public Robot SelectedRobot { get => this._selectedRobot; set => Set(ref this._selectedRobot, value); }

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
                        System.Windows.Controls.TabItem tabItem = (System.Windows.Controls.TabItem)obj;
                    }));
            }
        }

        #endregion

        #endregion

        #region Constructor

        public MainPageViewModel3()
        {
            //if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            //    return;

            if (Properties.Settings.Default.SaveRobots == null)
                Properties.Settings.Default.SaveRobots = new System.Collections.Specialized.StringCollection();

            else if (Properties.Settings.Default.SaveRobots.Count > 0)
            {
                for (int i = 0; i < Properties.Settings.Default.SaveRobots.Count; i++)
                {
                    this.AddRobot(JsonConvert.DeserializeObject<Robot>(Properties.Settings.Default.SaveRobots[i]));
                }
            }

            //App.Current.Log += new EventHandler<LogEventArgs>(SelectAppLogger);
        }

        #endregion

        #region Private functions

        private void AddRobot(Robot robot)
        {
            if (robot.Name == string.Empty)
                robot.Name = $"Робот {this.RobotsCollection.Count + 1}";

            this.RobotsCollection.Add(robot);
            RaisePropertyChanged(nameof(this.RobotsCollection));
        }

        #endregion

        #region Public functions

        #endregion
    }
}

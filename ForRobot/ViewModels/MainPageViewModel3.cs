using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForRobot.ViewModels
{
    public class MainPageViewModel3 : BaseClass
    {
        #region Private variables

        #region Commands

        private RelayCommand _createNewFileCommand;
        private RelayCommand _closeFileCommand;

        #endregion

        #endregion

        #region Public variables

        public Version Version { get => System.Reflection.Assembly.GetEntryAssembly().GetName().Version; }

        #region Collections

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

            //if (Properties.Settings.Default.SaveRobots == null)
            //    Properties.Settings.Default.SaveRobots = new System.Collections.Specialized.StringCollection();

            //App.Current.Log += new EventHandler<LogEventArgs>(SelectAppLogger);
        }

        #endregion

        #region Private functions

        #endregion

        #region Public functions

        #endregion
    }
}

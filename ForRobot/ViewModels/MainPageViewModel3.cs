using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForRobot.ViewModels
{
    public class MainPageViewModel3 : BaseClass
    {
        #region Private variables

        #endregion

        #region Public variables

        public Version Version { get => System.Reflection.Assembly.GetEntryAssembly().GetName().Version; }

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

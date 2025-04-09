using System;
using System.Windows;
using System.ComponentModel;

namespace ForRobot.ViewModels
{
    public class MainPageViewModel4 : BaseClass
    {
        #region Private variables



        #endregion Private variables

        #region Public variables



        #endregion Public variables

        public MainPageViewModel4()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            if (Properties.Settings.Default.SaveRobots == null)
                Properties.Settings.Default.SaveRobots = new System.Collections.Specialized.StringCollection();
        }

        #region Private functions



        #endregion Private functions

        #region Public functions



        #endregion Public functions
    }
}

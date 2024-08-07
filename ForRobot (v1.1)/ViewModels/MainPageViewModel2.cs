using System;
using System.Windows;
using System.ComponentModel;

namespace ForRobot.ViewModels
{
    public class MainPageViewModel2 : BaseClass
    {
        #region Private variables



        #endregion

        #region Public variables

        public Version Version { get => System.Reflection.Assembly.GetEntryAssembly().GetName().Version; }

        #endregion

        #region Constructor

        public MainPageViewModel2()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
        }

        #endregion

        #region Private functions

        #endregion
    }
}

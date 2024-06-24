using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;


namespace ForRobot.ViewModels
{
    public class MainWindowViewModel : BaseClass
    {
        #region Private variables


        #endregion

        #region Public variables

        public WindowState State { get; set; } = WindowState.Maximized;

        public Page NowPage { get; set; } = new Views.Pages.PageMain();

        #endregion

        #region Constructor

        public MainWindowViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
        }

        #endregion

    }
}

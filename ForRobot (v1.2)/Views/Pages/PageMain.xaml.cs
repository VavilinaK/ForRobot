using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace ForRobot.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageMain1.xaml
    /// </summary>
    public partial class PageMain : Page
    {
        #region Private variables

        private ViewModels.MainPageViewModel _viewModel;

        #endregion

        #region Public variables

        public ViewModels.MainPageViewModel ViewModel
        {
            get { return _viewModel ?? (ViewModels.MainPageViewModel)this.DataContext ?? (_viewModel = new ViewModels.MainPageViewModel()); }
        }

        #endregion

        #region Constructr

        public PageMain()
        {
            InitializeComponent();
            if (this.DataContext == null) { this.DataContext = ViewModel; }
        }

        #endregion

        #region Private function

        #endregion
    }
}

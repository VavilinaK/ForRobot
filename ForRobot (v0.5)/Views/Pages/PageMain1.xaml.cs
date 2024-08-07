using System;
using System.Windows.Controls;

namespace ForRobot.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageMain1.xaml
    /// </summary>
    public partial class PageMain1 : Page
    {
        #region Private variables

        private ViewModels.MainPageViewModel1 _viewModel;

        #endregion

        #region Public variables

        public ViewModels.MainPageViewModel1 ViewModel
        {
            get { return _viewModel ?? (ViewModels.MainPageViewModel1)this.DataContext ?? (_viewModel = new ViewModels.MainPageViewModel1()); }
        }

        #endregion

        #region Constructr

        public PageMain1()
        {
            InitializeComponent();
            if (this.DataContext == null) { this.DataContext = ViewModel; }
        }

        #endregion
    }
}

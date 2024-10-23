using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace ForRobot.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageMain2.xaml
    /// </summary>
    public partial class PageMain2 : Page
    {
        #region Private variables

        private ViewModels.MainPageViewModel2 _viewModel;

        #endregion

        #region Public variables

        public ViewModels.MainPageViewModel2 ViewModel
        {
            get { return _viewModel ?? (ViewModels.MainPageViewModel2)this.DataContext ?? (_viewModel = new ViewModels.MainPageViewModel2()); }
        }

        #endregion

        #region Constructr

        public PageMain2()
        {
            InitializeComponent();
            if (this.DataContext == null) { this.DataContext = ViewModel; }
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.UpdateLayout();
            Keyboard.ClearFocus();
            System.Windows.Input.Keyboard.Focus(App.Current.MainWindowView);
        }
    }
}

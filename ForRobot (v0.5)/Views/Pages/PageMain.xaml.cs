using System;
//using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

namespace ForRobot.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageMain.xaml
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
    }
}

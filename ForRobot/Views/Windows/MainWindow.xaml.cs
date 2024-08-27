using System;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

namespace ForRobot.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private variables

        private ViewModels.MainWindowViewModel _viewModel;

        #endregion

        #region Public variables

        public ViewModels.MainWindowViewModel ViewModel
        {
            get { return _viewModel ?? (ViewModels.MainWindowViewModel)this.DataContext ?? (_viewModel = new ViewModels.MainWindowViewModel()); }
        }

        #endregion

        #region Constructr

        public MainWindow()
        {
            InitializeComponent();
            if (this.DataContext == null) { this.DataContext = ViewModel; }
        }

        #endregion

        #region Private functions

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_SHOWME)
            {
                ShowMe();
            }
            return IntPtr.Zero;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.MainFrame.Content is Pages.PageMain2 && ((Pages.PageMain2)this.MainFrame.Content).ViewModel.RobotsCollection.Where(robot => robot.Item2.IsConnection).Count() > 0)
            {
                if (MessageBox.Show($"Закрыть соединение?", "Закрытие окна", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    foreach (var robot in ((Pages.PageMain2)this.MainFrame.Content).ViewModel.RobotsCollection)
                    {
                        robot.Item2.Dispose();
                    }
                }
                else
                    e.Cancel = true;
            }
            else
                e.Cancel = false;
        }

        #endregion

        #region Protected function

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        #endregion

        #region Public functions

        public void ShowMe()
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }
            this.Activate();
        }

        #endregion
    }
}

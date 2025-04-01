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

        /// <summary>
        /// Настройки приложения
        /// </summary>
        private readonly ForRobot.Libr.Settings.Settings Settings = ForRobot.Libr.Settings.Settings.GetSettings();

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            lock (App.Current)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Libr.Behavior.SaveLayoutMessage());

                if (!((System.Windows.Controls.Page)this.MainFrame?.Content is Pages.PageMain2))
                {
                    e.Cancel = false;
                    return;
                }

                switch (this.Settings.ModeClosingApp)
                {
                    case Libr.Settings.ModeClosingApp.Ever:
                        if (((Pages.PageMain2)this.MainFrame.Content).ViewModel.RobotsCollection.Where(robot => robot.IsConnection).Count() > 0)
                        {
                            if (MessageBox.Show($"Закрыть соединение?", "Закрытие приложения", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                            {
                                e.Cancel = true;
                                return;
                            }
                            else
                                break;
                        }
                        else if (MessageBox.Show($"Закрыть приложение?", "Закрытие приложения", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            e.Cancel = true;
                            return;
                        }                        
                        break;

                    case Libr.Settings.ModeClosingApp.HaveConnected:
                        if (((Pages.PageMain2)this.MainFrame.Content).ViewModel.RobotsCollection.Where(robot => robot.IsConnection).Count() > 0 &&
                            MessageBox.Show($"Закрыть соединение?", "Закрытие приложения", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                        {
                            e.Cancel = true;
                            return;
                        }
                        break;
                }

                if (((Pages.PageMain2)this.MainFrame.Content).ViewModel.RobotsCollection.Where(robot => robot.IsConnection).Count() > 0)
                {
                    foreach (var robot in ((Pages.PageMain2)this.MainFrame.Content).ViewModel.RobotsCollection)
                    {
                        robot.Dispose();
                    }
                }
            }
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

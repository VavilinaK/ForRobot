using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using AvalonDock.Layout.Serialization;
using GalaSoft.MvvmLight.Messaging;
using HelixToolkit.Wpf;

using ForRobot.Libr.Behavior;

namespace ForRobot.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageMain3.xaml
    /// </summary>
    public partial class PageMain3 : Page
    {
        private ViewModels.MainPageViewModel3 _viewModel;

        public ViewModels.MainPageViewModel3 ViewModel
        {
            get { return _viewModel ?? (ViewModels.MainPageViewModel3)this.DataContext ?? (_viewModel = new ViewModels.MainPageViewModel3()); }
        }
        
        public PageMain3()
        {
            InitializeComponent();

            Messenger.Default.Register<SaveLayoutMessage>(this, _ => SaveLayout());
            Messenger.Default.Register<LoadLayoutMessage>(this, _ => LoadLayout());

            if (this.DataContext == null) { this.DataContext = ViewModel; }
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var viewport = sender as HelixViewport3D;
            var firstHit = viewport?.Viewport.FindHits(e.GetPosition(viewport))?.FirstOrDefault();
            if (firstHit != null)
                this.ViewModel.Select(firstHit.Visual);
            else
                this.ViewModel.Select(null);
        }       

        private void SaveLayout()
        {
            try
            {
                var serializer = new XmlLayoutSerializer(this.DockingManeger);
                serializer.Serialize(@".\AvalonDock.config");
            }
            catch (Exception ex)
            {
                App.Current.Logger.Error(ex, ex.Message);
                MessageBox.Show($"Ошибка при загрузке макета: {ex.Message}");
            }
        }

        private void LoadLayout()
        {
            try
            {
                if (File.Exists(@".\AvalonDock.config"))
                {
                    var serializer = new XmlLayoutSerializer(this.DockingManeger);
                    serializer.Deserialize(@".\AvalonDock.config");
                }
            }
            catch (Exception ex)
            {
                App.Current.Logger.Error(ex, ex.Message);
                MessageBox.Show($"Ошибка при сохранении макета: {ex.Message}");
            }
        }
    }
}

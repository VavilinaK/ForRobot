using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.Generic;

using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using GalaSoft.MvvmLight.Messaging;
using HelixToolkit.Wpf;

using ForRobot.Libr.Behavior;
using ForRobot.Libr.Messages;

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
            get
            {
                return _viewModel ?? 
                    (ViewModels.MainPageViewModel3)this.DataContext ?? 
                    (_viewModel = new ViewModels.MainPageViewModel3());
            }
        }
        
        public PageMain3()
        {
            InitializeComponent();

            this.Loaded += OnPageLoaded;

            //Messenger.Default.Register<FindElementByTagMessage>(this, message => 
            //{
            //    DockingManager dockingManager = FindChild<DockingManager>(this);

            //    LayoutAnchorable anchorable = FindLayoutAnchorable(dockingManager, "Параметры детали");
            //    anchorable?.Show();

            //    var content = anchorable.Content as FrameworkElement;
            //    if (content == null) return;

            //    TreeView treeView = FindChild<TreeView>(content);
            //    if (treeView == null) return;

            //    ExpandAllTreeViewItems(treeView);

            //    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle, new Action(() =>
            //    {
            //        var targetTemplate = this.Resources["PlitaDopTemplate3Style"] as DataTemplate;
            //        var presenter = FindChild<ContentPresenter>(this);

            //        if (presenter.ContentTemplate == targetTemplate)
            //        {
            //            TextBox textBox = FindElementByTag<TextBox>(presenter, message.TagProperty);
            //            if (textBox != null && !textBox.IsKeyboardFocusWithin)
            //            {
            //                textBox.Focus();
            //                Keyboard.Focus(textBox);
            //                textBox.SelectAll();
            //            }
            //        }
            //    }));
            //}); // Сообщение получения фокуса TextBox при нажатии на соответствующий параметр модели.

            Messenger.Default.Register<SaveLayoutMessage>(this, _ => SaveLayout()); // Сообщение сохранения макета интерфейса.
            Messenger.Default.Register<LoadLayoutMessage>(this, _ => LoadLayout()); // Сообщение выгрузки макета интерфейса.

            if (this.DataContext == null) { this.DataContext = ViewModel; }
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= OnPageLoaded;

            Messenger.Default.Register<SelectLayoutDocumentPane>(this, HandleSelectLayoutDocument);
        }

        private void HandleSelectLayoutDocument(SelectLayoutDocumentPane message)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DockingManager dockingManager = FindChild<DockingManager>(this);
                var layoutDocument = dockingManager?.Layout.Descendents()
                                                           .OfType<LayoutDocument>()
                                                           .FirstOrDefault(d => d.Content == message.SelectedFile);

                if (layoutDocument != null)
                {
                    layoutDocument.IsActive = true;
                }
            }), System.Windows.Threading.DispatcherPriority.Background);

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

        /// <summary>
        /// Сохранение макета AvalonDock
        /// </summary>
        private void SaveLayout()
        {
            try
            {
                var serializer = new XmlLayoutSerializer(this.DockingManeger);
                serializer.Serialize(App.Current.AvalonConfigPath);
            }
            catch (Exception ex)
            {
                App.Current.Logger.Error(ex, ex.Message);
                MessageBox.Show($"Ошибка при загрузке макета: {ex.Message}");
            }
        }

        /// <summary>
        /// Выгрузка макета AvalonDock
        /// </summary>
        private void LoadLayout()
        {
            try
            {
                if (File.Exists(App.Current.AvalonConfigPath))
                {
                    var serializer = new XmlLayoutSerializer(this.DockingManeger);
                    serializer.Deserialize(App.Current.AvalonConfigPath);
                }
            }
            catch (Exception ex)
            {
                App.Current.Logger.Error(ex, ex.Message);
                MessageBox.Show($"Ошибка при сохранении макета: {ex.Message}");
            }
        }

        /// <summary>
        /// Поиск LayoutAnchorable по заголовку
        /// </summary>
        /// <param name="dockingManager"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private static LayoutAnchorable FindLayoutAnchorable(DockingManager dockingManager, string title) => dockingManager?.Layout.Descendents().OfType<LayoutAnchorable>().FirstOrDefault(a => a.Title == title);

        private static T FindChild<T>(DependencyObject parent, Func<T, bool> predicate = null) where T : DependencyObject
        {
            if (parent == null) return null;

            if (parent is T parentAsT && (predicate?.Invoke(parentAsT) ?? true))
                return parentAsT;

            // Если элемент визуальный, проверяем визуальное дерево
            if (parent is Visual || parent is Visual3D)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    var result = FindChild(child, predicate);
                    if (result != null) return result;
                }
            }


            //for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            //{
            //    var child = VisualTreeHelper.GetChild(parent, i);

            //    if (child is T result && (predicate?.Invoke(result) ?? true))
            //        return result;

            //    var descendant = FindVisualChild(child, predicate);
            //    if (descendant != null)
            //        return descendant;
            //}
            return null;
        }

        public static T FindElementByTag<T>(DependencyObject parent, object tagValue) where T : FrameworkElement
        {
            if (parent == null) return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T element && Equals(element.Tag, tagValue))
                    return element;

                var result = FindElementByTag<T>(child, tagValue);
                if (result != null)
                    return result;
            }
            return null;
        }
        
        private static void ExpandAllTreeViewItems(ItemsControl parent)
        {
            if (parent == null) return;

            foreach (var item in parent.Items)
            {
                TreeViewItem container = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                if (container != null)
                {
                    container.IsExpanded = true;
                    ExpandAllTreeViewItems(container);
                }
            }
        }

        ~PageMain3()
        {
            //Messenger.Default.Unregister<FindElementByTagMessage>(this); 
            // Отмена подписки на сообщения.
            Messenger.Default.Unregister<SelectLayoutDocumentPane>(this);
            Messenger.Default.Unregister<SaveLayoutMessage>(this);
            Messenger.Default.Unregister<LoadLayoutMessage>(this);
        }
    }
}

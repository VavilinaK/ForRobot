using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections;

namespace ForRobot.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для SelectWindow.xaml
    /// </summary>
    public partial class SelectWindow : Window, IDisposable
    {
        #region Public variables

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource),
                                                                                                    typeof(IEnumerable),
                                                                                                    typeof(SelectWindow),
                                                                                                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnItemsSourceChanged));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(nameof(SelectedItems),
                                                                                                      typeof(IEnumerable),
                                                                                                      typeof(SelectWindow),
                                                                                                      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemsChanged));

        public IEnumerable SelectedItems
        {
            get => this.SelectedListBox.SelectedItems;
            set => this.SelectedListBox.SelectedItem = value;
        }

        #endregion Public variables

        #region Constructs

        public SelectWindow()
        {
            InitializeComponent();
            this.Closed += (a, b) => this.Dispose();
        }

        public SelectWindow(IEnumerable itemsSource, IEnumerable selectedItems = null, ResourceDictionary resource = null) : this()
        {
            this.ItemsSource = itemsSource;
            this.SelectedItems = selectedItems;
        }

        #endregion Construct

        #region Private functions

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SelectWindow)d;
            control.SelectedListBox.ItemsSource = e.NewValue as IEnumerable;
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SelectWindow)d;
            control.UpdateSelectedItems();
        }

        private void UpdateSelectedItems()
        {
            if (SelectedItems == null) return;

            SelectedListBox.SelectedItems.Clear();
            foreach (var item in SelectedItems)
            {
                SelectedListBox.SelectedItems.Add(item);
            }
        }

        private void BtnDialogOk_Click(object sender, RoutedEventArgs e) => this.DialogResult = true;

        #endregion Private functions

        #region IDisposable Support

        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing) { }
                _disposedValue = true;
            }
        }

        ~SelectWindow() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

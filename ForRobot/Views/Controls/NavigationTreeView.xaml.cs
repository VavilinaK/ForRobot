using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

using ForRobot.Model.Controls;

namespace ForRobot.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для NavigationTreeView.xaml
    /// </summary>
    public partial class NavigationTreeView : UserControl, INotifyPropertyChanged
    {

        #region Properties

        public string SelectedFile
        {
            get => (string)GetValue(SelectedFileProperty);
            set => SetValue(SelectedFileProperty, value);
        }

        public bool DataFileIsHidden
        {
            get => (bool)GetValue(DataFileIsHiddenProperty);
            set => SetValue(DataFileIsHiddenProperty, value);
        }

        public ObservableCollection<File> FileCollection
        {
            get => (ObservableCollection<File>)GetValue(FileCollectionProperty);
            set => SetValue(FileCollectionProperty, value);
        }
        

        #region Command

        public IAsyncCommand SelectItemCommand
        {
            get { return (IAsyncCommand)GetValue(SelectItemCommandProperty); }
            set { SetValue(SelectItemCommandProperty, value); }
        }

        public IAsyncCommand DeleteNodeCommand
        {
            get { return (IAsyncCommand)GetValue(DeleteNodeCommandProperty); }
            set { SetValue(DeleteNodeCommandProperty, value); }
        }

        public IAsyncCommand SelectFolderCommand
        {
            get { return (IAsyncCommand)GetValue(SelectFolderCommandProperty); }
            set { SetValue(SelectFolderCommandProperty, value); }
        }

        #endregion

        #region Static readonly

        public static readonly DependencyProperty SelectedFileProperty = DependencyProperty.Register(nameof(SelectedFile), typeof(string), typeof(NavigationTreeView), 
                                                                                                     new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnSelectedFileChanged)));

        public static readonly DependencyProperty DataFileIsHiddenProperty = DependencyProperty.Register(nameof(DataFileIsHidden), typeof(bool), typeof(NavigationTreeView));

        public static readonly DependencyProperty FileCollectionProperty = DependencyProperty.Register(nameof(FileCollection), typeof(ObservableCollection<File>), typeof(NavigationTreeView), new PropertyMetadata(null));

        #region Command

        public static readonly DependencyProperty SelectItemCommandProperty = DependencyProperty.Register(nameof(SelectItemCommand),
                                                                                                          typeof(IAsyncCommand),
                                                                                                          typeof(NavigationTreeView),
                                                                                                          new PropertyMetadata());

        public static readonly DependencyProperty DeleteNodeCommandProperty = DependencyProperty.Register(nameof(DeleteNodeCommand),
                                                                                                          typeof(IAsyncCommand),
                                                                                                          typeof(NavigationTreeView),
                                                                                                          new PropertyMetadata());

        public static readonly DependencyProperty SelectFolderCommandProperty = DependencyProperty.Register(nameof(SelectFolderCommand),
                                                                                                            typeof(RelayCommand),
                                                                                                            typeof(NavigationTreeView),
                                                                                                            new PropertyMetadata());

        #endregion

        #endregion

        #endregion

        #region Construct

        public NavigationTreeView()
        {
            InitializeComponent();
        }

        #endregion

        #region Private functions

        private static void OnSelectedFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NavigationTreeView treeView = (NavigationTreeView)d;
            treeView.SelectedFile = (string)e.NewValue;
            treeView.PropertyChanged?.Invoke(treeView, new PropertyChangedEventArgs(nameof(treeView.SelectedFile)));
        }

        #endregion

        #region Implementations of IDisposable

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}

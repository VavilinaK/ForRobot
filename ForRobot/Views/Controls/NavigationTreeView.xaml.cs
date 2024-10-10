using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

using ForRobot.Model.Controls;

namespace ForRobot.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для NavigationTreeView.xaml
    /// </summary>
    public partial class NavigationTreeView : UserControl
    {

        #region Properties

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

        public RelayCommand SelectFolderCommand
        {
            get { return (RelayCommand)GetValue(SelectFolderCommandProperty); }
            set { SetValue(SelectFolderCommandProperty, value); }
        }

        #endregion

        #region Static readonly

        public static readonly DependencyProperty DataFileIsHiddenProperty = DependencyProperty.Register(nameof(DataFileIsHidden), typeof(bool), typeof(NavigationTreeView));

        public static readonly DependencyProperty FileCollectionProperty = DependencyProperty.Register(nameof(FileCollection), typeof(ObservableCollection<File>), typeof(NavigationTreeView));

        #region Command

        public static readonly DependencyProperty SelectItemCommandProperty = DependencyProperty.Register(nameof(SelectItemCommand),
                                                                                                          typeof(IAsyncCommand),
                                                                                                          typeof(NavigationTreeView),
                                                                                                          new PropertyMetadata());

        public static readonly DependencyProperty SelectFolderCommandProperty = DependencyProperty.Register(nameof(SelectFolderCommand),
                                                                                                          typeof(RelayCommand),
                                                                                                          typeof(NavigationTreeView));

        #endregion

        #endregion

        #endregion

        #region Construct

        public NavigationTreeView()
        {
            InitializeComponent();
            this.SelectFolderCommand = new RelayCommand(obj => { });
        }

        #endregion

        private static RelayCommand _onSelectFolder;

        private static RelayCommand OnSelectFolder
        {
            get
            {
                return _onSelectFolder ??
                    (_onSelectFolder = new RelayCommand(obj =>
                    {  }));
            }
        }
    }
}

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

        public ObservableCollection<File> FileCollection
        {
            get => (ObservableCollection<File>)GetValue(FileCollectionProperty);
            set => SetValue(FileCollectionProperty, value);
        }

        #region Command

        public RelayCommand SelectItemCommand
        {
            get { return (RelayCommand)GetValue(SelectItemCommandProperty); }
            set { SetValue(SelectItemCommandProperty, value); }
        }

        #endregion

        #region Static readonly

        public static readonly DependencyProperty FileCollectionProperty = DependencyProperty.Register(nameof(FileCollection), typeof(ObservableCollection<File>), typeof(NavigationTreeView));

        #region Command

        public static readonly DependencyProperty SelectItemCommandProperty = DependencyProperty.Register(nameof(SelectItemCommand),
                                                                                                          typeof(RelayCommand),
                                                                                                          typeof(NavigationTreeView));

        #endregion

        #endregion

        #endregion

        #region Construct

        public NavigationTreeView()
        {
            InitializeComponent();
        }

        #endregion
    }
}

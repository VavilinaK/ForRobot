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

        public ObservableCollection<File> RootFileCollection
        {
            get => (ObservableCollection<File>)GetValue(RootFileCollectionProperty);
            set => SetValue(RootFileCollectionProperty, value);
        }

        #region Static readonly

        public static readonly DependencyProperty RootFileCollectionProperty = DependencyProperty.Register("RootFileCollection", typeof(ObservableCollection<File>), typeof(NavigationTreeView));

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

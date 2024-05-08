using System;
using System.Windows;
using System.Windows.Controls;

namespace ForRobot.Themes
{
    /// <summary>
    /// Логика взаимодействия для ToolBarTrayForRobot.xaml
    /// </summary>
    public partial class ToolBarTrayForRobot : UserControl
    {
        #region Public variables

        #region Properties

        public string NameRobot
        {
            get => (string)GetValue(NameRobotProperty);
            set => SetValue(NameRobotProperty, value);
        }

        #endregion

        #region Commands

        public RelayCommand RefreshCommand
        {
            get { return (RelayCommand)GetValue(RefreshCommandProperty); }
            set { SetValue(RefreshCommandProperty, value); }
        }

        #endregion

        #region Static readonly

        public static readonly DependencyProperty NameRobotProperty = DependencyProperty.Register("NameRobot", typeof(string), typeof(ToolBarTrayForRobot));

        #region Commands

        public static readonly DependencyProperty RefreshCommandProperty = DependencyProperty.Register("RefreshCommand", typeof(RelayCommand), typeof(ToolBarTrayForRobot), new UIPropertyMetadata(null));
        
        #endregion

        #endregion

        #endregion

        #region Construct

        public ToolBarTrayForRobot()
        {
            InitializeComponent();

            this.DataContext = new ViewModels.ToolBarViewModel();
        }

        #endregion
    }
}

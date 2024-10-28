using System;
using System.Windows;
using System.Windows.Controls;

namespace ForRobot.Views.Controls
{
    public enum WorkingRegims
    {
        Normal,
        AddFile
    }

    /// <summary>
    /// Логика взаимодействия для DragAndDropPopup.xaml
    /// </summary>
    public partial class DragAndDropPopup : UserControl
    {
        #region Private variables

        #region Commands

        private static RelayCommand _changeWorkingMode;

        #endregion

        #endregion

        #region Public variables

        #region Properties

        /// <summary>
        /// Режим работы
        /// </summary>
        public bool WorkingRegime
        {
            get => (bool)GetValue(WorkingRegimeProperty);
            set => SetValue(WorkingRegimeProperty, value);
        }

        public static readonly DependencyProperty WorkingRegimeProperty = DependencyProperty.Register(nameof(WorkingRegime),
                                                                                           typeof(bool),
                                                                                           typeof(Breadcrumb),
                                                                                           new PropertyMetadata(false));

        #endregion

        #region Commands

        #endregion

        #endregion

        #region Constructor

        public DragAndDropPopup()
        {
            InitializeComponent();
        }

        #endregion

        #region Private functions

        #endregion

        #region Public functions

        #endregion
    }
}

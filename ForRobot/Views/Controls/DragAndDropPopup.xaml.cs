using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using ForRobot.Model.Controls;

namespace ForRobot.Views.Controls
{
    public class FileForDragAndDropPopup : INotifyPropertyChanged
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public FileTypes Type { get; }

        public bool IsProgres { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// Логика взаимодействия для DragAndDropPopup.xaml
    /// </summary>
    public partial class DragAndDropPopup : UserControl
    {
        #region Private variables

        #region Commands

        private static RelayCommand _changeWorkingModeCommand;

        #endregion

        #endregion

        #region Public variables

        public ForRobot.Libr.FullyObservableCollection<FileForDragAndDropPopup> DowloadFiles { get; set; } = new Libr.FullyObservableCollection<FileForDragAndDropPopup>();

        #region Properties

        /// <summary>
        /// Добавляется ли в данный момент файл
        /// </summary>
        public bool IsAddingFile
        {
            get => (bool)GetValue(IsAddingFileProperty);
            private set => SetValue(IsAddingFileProperty, value);
        }

        public static readonly DependencyProperty IsAddingFileProperty = DependencyProperty.Register(nameof(IsAddingFile),
                                                                                           typeof(bool),
                                                                                           typeof(DragAndDropPopup),
                                                                                           new PropertyMetadata(false));

        #endregion

        #region Commands

        /// <summary>
        /// Команда смены режима работы <see cref="DragAndDropPopup"/>
        /// </summary>
        private static RelayCommand ChangeWorkingModeCommand
        {
            get
            {
                return _changeWorkingModeCommand ??
                    (_changeWorkingModeCommand = new RelayCommand(obj =>
                    {

                    }));
            }
        }

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

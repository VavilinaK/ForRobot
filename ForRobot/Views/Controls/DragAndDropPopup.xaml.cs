using System;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using ForRobot.Model.Controls;

namespace ForRobot.Views.Controls
{
    public class DragAndDropFile : INotifyPropertyChanged
    {
        /// <summary>
        /// Имя с разрешением
        /// </summary>
        public string FullName { get; private set; }

        public string OldPath { get; private set; }

        public FileTypes Type
        {
            get
            {
                switch (Path.GetExtension(this.OldPath))
                {
                    case ".dat":
                        return FileTypes.DataList;

                    case ".src":
                        return FileTypes.Program;

                    default:
                        return FileTypes.Unknow;
                }
            }
        }

        public bool IsCopy { get; set; } = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public DragAndDropFile(string sFilePath)
        {
            this.OldPath = sFilePath;
            this.FullName = Path.GetFileName(sFilePath);
        }
    }

    /// <summary>
    /// Логика взаимодействия для DragAndDropPopup.xaml
    /// </summary>
    public partial class DragAndDropPopup : UserControl, INotifyPropertyChanged
    {
        #region Private variables

        private ForRobot.Libr.FullyObservableCollection<DragAndDropFile> _copyFiles;

        //private ForRobot.Libr.FullyObservableCollection<DragAndDropFile> _copyFiles = new Libr.FullyObservableCollection<DragAndDropFile>(new System.Collections.Generic.List<DragAndDropFile>()
        //{
        //    new DragAndDropFile(@"D:\newPrograms\R2\main_gen.src"),
        //    new DragAndDropFile(@"D:\newPrograms\R2\edge_4_right_mte.dat"),
        //    new DragAndDropFile(@"D:\Пути установки.txt"),
        //    new DragAndDropFile(@"D:\newPrograms\R2\main_gen.src"),
        //    new DragAndDropFile(@"D:\newPrograms\R2\edge_4_right_mte.dat"),
        //    new DragAndDropFile(@"D:\Пути установки.txt"),
        //    new DragAndDropFile(@"D:\newPrograms\R2\main_gen.src"),
        //    new DragAndDropFile(@"D:\newPrograms\R2\edge_4_right_mte.dat"),
        //    new DragAndDropFile(@"D:\Пути установки.txt")
        //});


        #region Commands

        private static RelayCommand _changeWorkingModeCommand;

        private static RelayCommand _selectFilesCommand;

        #endregion

        #endregion

        #region Public variables

        /// <summary>
        /// Допустимые разрешения файлов
        /// </summary>
        public string[] AccopterebulExtension { get; } = new string[]
        {
            ".dat",
            ".src"
        };

        public ForRobot.Libr.FullyObservableCollection<DragAndDropFile> CopyFiles
        {
            get => this._copyFiles ?? (this._copyFiles = new Libr.FullyObservableCollection<DragAndDropFile>());
            set
            {
                this._copyFiles = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CopyFiles)));
            }
        }

        #region Properties

        /// <summary>
        /// Конечный путь для копирования файлов
        /// </summary>
        public string FinishPath
        {
            get => (string)GetValue(FinishPathProperty);
            set => SetValue(FinishPathProperty, value);
        }

        public static readonly DependencyProperty FinishPathProperty = DependencyProperty.Register(nameof(FinishPath),
                                                                                           typeof(string),
                                                                                           typeof(DragAndDropPopup));
        /// <summary>
        /// Добавляется ли в данный момент файл
        /// </summary>
        public bool IsAddingFile
        {
            get => (bool)GetValue(IsAddingFileProperty);
            set => SetValue(IsAddingFileProperty, value);
        }

        public static readonly DependencyProperty IsAddingFileProperty = DependencyProperty.RegisterAttached(nameof(IsAddingFile),
                                                                                                           typeof(bool),
                                                                                                           typeof(DragAndDropPopup),
                                                                                                           new UIPropertyMetadata(false, new PropertyChangedCallback(OnIsAddingFileChange)));

        #endregion

        #region Events


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

        /// <summary>
        /// Выбор файлов для отправки на роботы
        /// </summary>
        public RelayCommand SelectedFilesCommand
        {
            get
            {
                return _selectFilesCommand ??
                    (_selectFilesCommand = new RelayCommand(obj =>
                    {
                        System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog()
                        {
                            Filter = "Source Code or Data files (*.src, *.dat)|*.src;*.dat|Data files (*.dat)|*.dat|Source Code File (*.src)|*src",
                            Title = $"Копирование файлов в {this.FinishPath}",
                            Multiselect = true
                        };

                        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel && (string.IsNullOrEmpty(openFileDialog.FileName) || string.IsNullOrEmpty(openFileDialog.FileNames[0])))
                            return;

                        foreach(var path in openFileDialog.FileNames)
                        {
                            this.CopyFiles.Add(new DragAndDropFile(path));
                        }
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

        private async void Border_Drop(object sender, DragEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                this.IsAddingFile = false;
                if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                    return;

                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                for (int i = 0; i < files.Length; i++)
                {
                    if (this.AccopterebulExtension.Any(item => item == Path.GetExtension(files[i])))
                        this.CopyFiles.Add(new DragAndDropFile(files[i]));
                }
            });
        }

        private async void Border_DragEnter(object sender, DragEventArgs e) => await Dispatcher.InvokeAsync(() => { this.IsAddingFile = true; });

        private async void Border_DragOver(object sender, DragEventArgs e) => await Dispatcher.InvokeAsync(() => { this.IsAddingFile = true; });

        private async void Border_DragLeave(object sender, DragEventArgs e) => await Dispatcher.InvokeAsync(() => { this.IsAddingFile = false; });

        #region Static

        private static void OnIsAddingFileChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DragAndDropPopup dragAndDropPopup = (DragAndDropPopup)d;
            dragAndDropPopup.IsAddingFile = (bool)e.NewValue;
            dragAndDropPopup.RaisePropertyChanged(nameof(dragAndDropPopup.IsAddingFile));
        }

        #endregion

        #endregion

        #region Public functions

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            foreach (var prop in propertyNames)
            {
                this.RaisePropertyChanged(prop);
            }
        }

        protected void Set<T>(ref T propertyFiled, T newValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (!object.Equals(propertyFiled, newValue))
            {
                T oldValue = propertyFiled;
                propertyFiled = newValue;
                RaisePropertyChanged(propertyName);

                OnPropertyChanged(propertyName, oldValue, newValue);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName, object oldValue, object newValue) { }

        #endregion
    }
}

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using ForRobot.Model;
using ForRobot.Model.Controls;

namespace ForRobot.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для NavigationTreeView.xaml
    /// </summary>
    public partial class NavigationTreeView : UserControl, INotifyPropertyChanged
    {
        #region Properties

        public static readonly DependencyProperty RobotSourceProperty = DependencyProperty.Register(nameof(RobotSource),
                                                                                              typeof(Robot),
                                                                                              typeof(NavigationTreeView),
                                                                                              new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnRobotChanged)));

        public Robot RobotSource
        {
            get => (Robot)GetValue(RobotSourceProperty);
            set => SetValue(RobotSourceProperty, value);
        }

        public static readonly DependencyProperty AccessDataFileProperty = DependencyProperty.Register(nameof(AccessDataFile),
                                                                                                       typeof(bool), 
                                                                                                       typeof(NavigationTreeView),
                                                                                                       new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnAccessDataFileChanged)));

        /// <summary>
        /// Доступны ли файлы расширения .dat
        /// </summary>
        public bool AccessDataFile
        {
            get => (bool)GetValue(AccessDataFileProperty);
            set => SetValue(AccessDataFileProperty, value);
        }

        public static readonly DependencyProperty BlockedFolderProperty = DependencyProperty.Register(nameof(BlockedFolder),
                                                                                                      typeof(IDictionary<string, bool>),
                                                                                                      typeof(NavigationTreeView),
                                                                                                      new PropertyMetadata(null, new PropertyChangedCallback(OnBlockedFolderChanged)));

        #endregion Properties

        #region Private variables

        //private ICommand _selectFileCommand;
        //private ICommand _selectControllerFolderCommand;
        //private ICommand _deleteFileCommand;

        /// <summary>
        /// Обработчик исключений асинхронных комманд
        /// </summary>
        private static readonly Action<Exception> _exceptionCallback = new Action<Exception>(e =>
        {
            try
            {
                throw e;
            }
            catch (DivideByZeroException ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            catch (System.Threading.Tasks.TaskCanceledException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        });

        #endregion Private variables

        #region Public variables

        /// <summary>
        /// Скрытие папки
        /// </summary>
        public IDictionary<string, bool> BlockedFolder
        {
            get => (IDictionary<string, bool>)GetValue(BlockedFolderProperty);
            set => SetValue(BlockedFolderProperty, value);
        }

        /// <summary>
        /// Коллекция имен папок составляющих директорию
        /// </summary>
        public List<String> FoldersCollection
        {
            get => this.RobotSource?.PathControllerFolder.Split('\\').Where(item => item != string.Empty).Select((item, index) => index == 0 ? item + "\\" : item).ToList<string>();
        }

        //public ObservableCollection<IFile> FileCollection { get => this.DeleteFolder(); }

        #region Commands

        //public ICommand SelectFileCommandAsync { get => this._selectFileCommand ?? (this._selectFileCommand = new RelayCommand(obj => this.SelectFile(obj as string))); }

        //public ICommand SelectControllerFolderCommand { get => this._selectControllerFolderCommand ?? (this._selectControllerFolderCommand = new RelayCommand(obj => this.SelectControllerFolder(obj as string))); }

        //public ICommand DeleteFileCommandAsync { get => this._deleteFileCommand ?? (this._deleteFileCommand = new AsyncRelayCommand(async obj => await this.DeleteFileAsync(obj as string), _exceptionCallback)); }

        #endregion

        #endregion Public variables

        public NavigationTreeView()
        {
            InitializeComponent();          
        }

        #region Private function

        ///// <summary>
        ///// Удаление скрытых папок из файлов робота
        ///// </summary>
        ///// <param name="pairs"></param>
        //private ObservableCollection<IFile> DeleteFolder()
        //{
        //    if (this.Robot?.Files == null || this.BlockedFolder == null)
        //        return this.Robot?.Files.Children;

        //    File files = this.Robot.Files;

        //    var set = this.BlockedFolder.Where(x => !x.Value).Select(s => s.Key).ToList<string>();
        //    for (int i = 0; i < files.Children.Count(); i++)
        //    {
        //        var file = files.Children.ToArray()[i];
        //        if (set.Contains(file.Name))
        //        {
        //            files.Children.Remove(file);
        //            i--;
        //            continue;
        //        }

        //        var q = new Queue<IFile>();
        //        q.Enqueue(file);

        //        while (q.Count > 0)
        //        {
        //            var node = q.Dequeue();

        //            for (int y = 0; y < node.Children.Count; y++)
        //            {
        //                q.Enqueue(node.Children[y] as ForRobot.Model.Controls.File);

        //                if (set.Contains(node.Children[y].Name))
        //                {
        //                    node.Children.Remove(node.Children[y] as ForRobot.Model.Controls.File);
        //                    y--;
        //                }
        //            }
        //        }
        //    }
        //    return files.Children;
        //}

        //private void SelectControllerFolder(string path) => this.Robot.PathControllerFolder = path;

        //private void SelectFile(string filePath) => this.Robot.SelectProgramByPath(filePath);

        //private async Task DeleteFileAsync(string filePath)
        //{
        //    if (string.IsNullOrEmpty(filePath))
        //        return;

        //    this.Robot.DeleteFile(filePath);
        //    await this.Robot.GetFilesAsync();
        //}

        private static void OnRobotChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NavigationTreeView navigationTreeView = (NavigationTreeView)d;
            navigationTreeView.RobotSource = (Robot)e.NewValue;

            if (navigationTreeView.RobotSource != null)
                navigationTreeView.OnPropertyChanged(nameof(FoldersCollection));

            //    navigationTreeView.RobotSource.LoadedFilesEvent += (s, o) => navigationTreeView.OnPropertyChanged(nameof(FileCollection));
            navigationTreeView.OnPropertyChanged(nameof(Robot));
        }

        private static void OnAccessDataFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static void OnBlockedFolderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NavigationTreeView navigationTreeView = (NavigationTreeView)d;
            navigationTreeView.BlockedFolder = (IDictionary<string, bool>)e.NewValue;
            //navigationTreeView.OnPropertyChanged(nameof(navigationTreeView.BlockedFolder), nameof(navigationTreeView.FileCollection));
        }

        #endregion Private function

        #region Implementations of IDisposable

        private void OnPropertyChanged(params string[] propertyNames)
        {
            foreach (var prop in propertyNames)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}

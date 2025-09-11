using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Diagnostics;
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

        public static readonly DependencyProperty SelectedFolderProperty = DependencyProperty.Register(nameof(SelectedFolder),
                                                                                                       typeof(string),
                                                                                                       typeof(NavigationTreeView),
                                                                                                       new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnSelectedFolderChanged)));

        /// <summary>
        /// Выбранная папка
        /// </summary>
        public string SelectedFolder
        {
            get => (string)GetValue(SelectedFolderProperty);
            set => SetValue(SelectedFolderProperty, value);
        }

        /// <summary>
        /// Дочернии папки выбранного каталога
        /// </summary>
        public List<ForRobot.Model.Controls.File> ChildrenFolder
        {
            get => (this.SelectedFolder != null && this.RobotSource.Files.Children.Count > 0) ?
                    this.RobotSource.Files?.Search(this.SelectedFolder)?.Children.Where(item => item.Type == FileTypes.Folder).Cast<ForRobot.Model.Controls.File>().ToList() : null;
        }

        public static readonly DependencyProperty BlockedFolderProperty = DependencyProperty.Register(nameof(BlockedFolder),
                                                                                                      typeof(IDictionary<string, bool>),
                                                                                                      typeof(NavigationTreeView),
                                                                                                      new PropertyMetadata(null, new PropertyChangedCallback(OnBlockedFolderChanged)));

        /// <summary>
        /// Скрытие папки
        /// </summary>
        public IDictionary<string, bool> BlockedFolder
        {
            get => (IDictionary<string, bool>)GetValue(BlockedFolderProperty);
            set => SetValue(BlockedFolderProperty, value);
        }

        private ForRobot.Model.Controls.File _selectedPopupItem;
        public ForRobot.Model.Controls.File SelectedPopupItem
        {
            get => this._selectedPopupItem;
            set
            {
                if (value == null)
                    return;

                this._selectedPopupItem = value;
                this.RobotSource.PathControllerFolder = this._selectedPopupItem.Path.TrimEnd(new char[] { '\\' });
                this.OnPropertyChanged(nameof(this.RobotSource), nameof(this.FoldersCollection));
            }
        }

        #endregion Properties

        #region Private variables

        private ICommand _selectFileCommand;
        private ICommand _openCommand;
        private ICommand _downladeFileCommand;
        //private ICommand _selectControllerFolderCommand;
        private ICommand _deleteFileCommand;

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
                App.Current.Logger.Error(ex, ex.Message);
                System.Windows.MessageBox.Show(ex.Message);
            }
        });

        #endregion Private variables

        #region Public variables

        ///// <summary>
        ///// Скрытие папки
        ///// </summary>
        //public IDictionary<string, bool> BlockedFolder
        //{
        //    get => (IDictionary<string, bool>)GetValue(BlockedFolderProperty);
        //    set => SetValue(BlockedFolderProperty, value);
        //}

        /// <summary>
        /// Коллекция имен папок составляющих директорию
        /// </summary>
        public List<string> FoldersCollection
        {
            get => this.RobotSource?.PathControllerFolder.Split('\\').Where(item => item != string.Empty).Select((item, index) => index == 0 ? item + "\\" : item).ToList<string>();
        }

        //public ObservableCollection<IFile> FileCollection { get => this.DeleteFolder(); }

        #region Commands

        public ICommand SelectFileCommandAsync { get => this._selectFileCommand ?? (this._selectFileCommand = new AsyncRelayCommand(async obj => await this.SelectFileAsync(obj as string), _exceptionCallback)); }

        public ICommand OpenFileCommandAsync { get => this._openCommand ?? (this._openCommand = new AsyncRelayCommand(async obj => await this.OpenFileAsync(obj as string), _exceptionCallback)); }

        public ICommand DownladeFileCommandAsync { get => this._downladeFileCommand ?? (this._downladeFileCommand = new AsyncRelayCommand(async obj => await this.DownladeFileAsync(obj as string), _exceptionCallback)); }

        //public ICommand SelectControllerFolderCommand { get => this._selectControllerFolderCommand ?? (this._selectControllerFolderCommand = new RelayCommand(obj => this.SelectControllerFolder(obj as string))); }

        public ICommand DeleteFileCommandAsync { get => this._deleteFileCommand ?? (this._deleteFileCommand = new AsyncRelayCommand(async obj => await this.DeleteFileAsync(obj as string), _exceptionCallback)); }

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

        private async Task SelectFileAsync(string filePath) => await this.RobotSource.SelectProgramByPathAsync(filePath);

        private async Task OpenFileAsync(string path)
        {
            string finalPath = Path.GetTempPath();

            await DownladeFileAsync(path, finalPath);

            var app = App.Current.Settings.SelectedAppForOpened;
            if (app == null)
                throw new Exception("В настройках не выбранно приложение для открытия файлов.");

            string newPath = Path.Combine(finalPath, Path.GetFileName(path));
            newPath = Uri.UnescapeDataString(newPath);

            if (!System.IO.File.Exists(newPath))
                return;

            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    RedirectStandardInput = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = new FileInfo(app.Path).DirectoryName,
                    FileName = app.Path,
                    Arguments = newPath
                }
            };
            process.ErrorDataReceived += (s, e) => { throw new Exception(e.Data); };
            process.Start();
        }

        private async Task DownladeFileAsync(string filePath, string finalPath = null)
        {
            if (finalPath == null)
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    using (var fbd = new System.Windows.Forms.FolderBrowserDialog() { Description = "Сохранить файлы в:" })
                    {
                        System.Windows.Forms.DialogResult result = fbd.ShowDialog();
                        if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                        {
                            finalPath = fbd.SelectedPath;
                        }
                        else
                            return;
                    }
                });

            if (!(await this.RobotSource.DownladeFileAsync(filePath, finalPath)))
                throw new Exception($"Ошибка скачивания {filePath} в {finalPath}");

            App.Current.Logger.Info(string.Format("Скачен файл {0} в {1}", finalPath, finalPath));
        }

        private async Task DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            await this.RobotSource.DeleteFileAsync(filePath);
            await this.RobotSource.GetFilesAsync();
        }
        
        private static void OnRobotChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NavigationTreeView navigationTreeView = (NavigationTreeView)d;

            if(navigationTreeView.RobotSource != null)
            {
                navigationTreeView.RobotSource.LoadedFilesEvent -= (s, o) => navigationTreeView.OnPropertyChanged(nameof(RobotSource), nameof(FoldersCollection));
            }

            navigationTreeView.RobotSource = (Robot)e.NewValue;

            if (navigationTreeView.RobotSource != null)
            {
                navigationTreeView.RobotSource.PropertyChanged += (s, o) =>
                {
                    navigationTreeView.OnPropertyChanged(nameof(RobotSource));
                    if(o.PropertyName == nameof(navigationTreeView.RobotSource.PathControllerFolder))
                    {
                        navigationTreeView.OnPropertyChanged(nameof(FoldersCollection));
                    }
                };
                navigationTreeView.RobotSource.LoadedFilesEvent += (s, o) => navigationTreeView.OnPropertyChanged(nameof(RobotSource), nameof(FoldersCollection));
            }
        }

        private static void OnAccessDataFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private static void OnSelectedFolderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NavigationTreeView navigationTreeView = (NavigationTreeView)d;
            //var newPath = navigationTreeView.RobotSource.Files.Search((string)e.NewValue);
            //navigationTreeView.SelectedFolder = newPath?.Path;
            //navigationTreeView.SelectedFolder = newPath?.Path.TrimEnd(new char[] { '\\' });
            navigationTreeView.SelectedFolder = (string)e.NewValue;
            navigationTreeView.OnPropertyChanged(nameof(navigationTreeView.SelectedFolder), nameof(navigationTreeView.ChildrenFolder));
        }

        private static void OnBlockedFolderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //NavigationTreeView navigationTreeView = (NavigationTreeView)d;
            //navigationTreeView.BlockedFolder = (IDictionary<string, bool>)e.NewValue;
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

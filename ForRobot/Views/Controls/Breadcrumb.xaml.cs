using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.Generic;

using ForRobot.Model;
using ForRobot.Model.Controls;

namespace ForRobot.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для Breadcrumb.xaml
    /// </summary>
    public partial class Breadcrumb : ComboBox, INotifyPropertyChanged
    {
        #region Properties

        public static readonly DependencyProperty RobotProperty = DependencyProperty.Register(nameof(Robot),
                                                                                              typeof(Robot),
                                                                                              typeof(Breadcrumb),
                                                                                              new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnRobotChanged)));

        public static readonly DependencyProperty RootProperty = DependencyProperty.Register(nameof(Root),
                                                                                             typeof(string),
                                                                                             typeof(Breadcrumb),
                                                                                             new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IsProgressProperty = DependencyProperty.Register(nameof(IsProgress),
                                                                                                   typeof(bool),
                                                                                                   typeof(Breadcrumb),
                                                                                                   new FrameworkPropertyMetadata(false));

        #endregion Properties

        #region Private variables

        private ICommand _homeCommand;
        private ICommand _updateFilesCommand;

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

        #endregion

        #region Public variables

        public Robot Robot
        {
            get => (Robot)GetValue(RobotProperty);
            set => SetValue(RobotProperty, value);
        }

        /// <summary>
        /// Корневой каталог
        /// </summary>
        public string Root
        {
            get => (string)GetValue(RootProperty);
            set => SetValue(RootProperty, value);
        }

        private string _selectedFolder;
        /// <summary>
        /// Выбранная папка
        /// </summary>
        public string SelectedFolder
        {
            get => this._selectedFolder;
            set
            {
                if (value == null)
                    return;

                this._selectedFolder = value;
                this.OnPropertyChanged(nameof(this.ChildrenFolder), nameof(CanOpenMenu));
            }
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
                this.Robot.PathControllerFolder = this._selectedPopupItem.Path.TrimEnd(new char[] { '\\' });
                this.OnPropertyChanged(nameof(this.FoldersCollection));

                System.Windows.Input.FocusManager.SetFocusedElement(System.Windows.Input.FocusManager.GetFocusScope(this.Parent), null);
            }
        }

        public bool CanOpenMenu { get => this.ChildrenFolder?.Count > 0; }

        /// <summary>
        /// Идёт процесс закрузки файлов
        /// </summary>
        public bool IsProgress
        {
            get => (bool)GetValue(IsProgressProperty);
            set => SetValue(IsProgressProperty, value);
        }

        /// <summary>
        /// Коллекция имен папок составляющих директорию
        /// </summary>
        public List<String> FoldersCollection
        {
            get => this.Robot?.PathControllerFolder.Split('\\').Where(item => item != string.Empty).Select((item, index) => index == 0 ? item + "\\" : item).ToList<string>();
        }

        /// <summary>
        /// Дочернии папки выбранного каталога
        /// </summary>
        public List<ForRobot.Model.Controls.File> ChildrenFolder
        {
            get => (this.SelectedFolder != null && this.Robot.Files.Children.Count > 0) ?
                    this.Robot.Files?.Search(this.SelectedFolder)?.Children.Where(item => item.Type == FileTypes.Folder).Cast<ForRobot.Model.Controls.File>().ToList() : null;
        }

        #region Commands

        public ICommand HomeCommand { get => this._homeCommand ?? (this._homeCommand = new RelayCommand(_ => this.HomeDirection())); }

        public ICommand UpdateFilesCommandAsync { get => this._updateFilesCommand ?? (this._updateFilesCommand = new AsyncRelayCommand(async _  => await this.UpdateFilesAsync(), _exceptionCallback)); }

        public ICommand DropFilesCommandAsync { get; } = new AsyncRelayCommand(async obj => await DropFilesAsync(obj as Robot), _exceptionCallback);

        public ICommand DeleteFilesCommandAsync { get; } = new AsyncRelayCommand(async obj => await DeleteFilesAsync(obj as Robot), _exceptionCallback);

        #endregion Commands

        #endregion Public variables

        public Breadcrumb()
        {
            InitializeComponent();
        }

        #region Private function

        private void HomeDirection() => this.Robot.PathControllerFolder = this.Root;

        private async Task UpdateFilesAsync() => await this.Robot.GetFilesAsync();
        
        private static async Task DeleteFilesAsync(Robot robot)
        {
            List<string> checkedFiles = new List<string>(); // Список отмеченных файлов
            foreach (var file in robot.Files.Children)
            {
                Stack<ForRobot.Model.Controls.IFile> stack = new Stack<Model.Controls.IFile>();
                stack.Push(file);
                ForRobot.Model.Controls.IFile current;
                do
                {
                    current = stack.Pop();
                    IEnumerable<ForRobot.Model.Controls.IFile> files = current.Children;

                    if (current.IsCheck)
                        checkedFiles.Add(current.Path);

                    foreach (var f in files)
                        stack.Push(f);
                }
                while (stack.Count > 0);
            }

            var task = Task.Run(async () =>
            {
                foreach (string path in checkedFiles)
                {
                    await Task.Run(() => robot.DeleteFile(path));
                }
            });
            await task;
            await robot.GetFilesAsync();
        }

        private static async Task DropFilesAsync(Robot robot)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = "Source Code or Data files (*.src, *.dat)|*.src;*.dat|Data files (*.dat)|*.dat|Source Code File (*.src)|*src",
                Title = $"Отправка файла/ов на {robot.Name}",
                Multiselect = true
            })
            {
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel && (string.IsNullOrEmpty(openFileDialog.FileName) || string.IsNullOrEmpty(openFileDialog.FileNames[0])))
                    return;

                foreach (var path in openFileDialog.FileNames)
                {
                    string fileName = Path.GetFileName(path);

                    string tempFile = System.IO.Path.Combine(Robot.PathOfTempFolder, fileName);

                    if (!robot.CopyToPC(path, tempFile))
                        continue;

                    if (!robot.Copy(tempFile, System.IO.Path.Combine(robot.PathControllerFolder, fileName)))
                        continue;
                }

                await robot.GetFilesAsync();

                foreach (var file in robot.Files.Children)
                {
                    foreach (var path in openFileDialog.FileNames)
                    {
                        var searchFile = file.Search(Path.GetFileName(path));
                        if (searchFile == null) continue;
                        file.Search(Path.GetFileName(path)).IsCopy = true;
                    }
                }
            }
        }

        private static void OnRobotChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Breadcrumb breadcrumb = (Breadcrumb)d;
            breadcrumb.Robot = (Robot)e.NewValue;

            if(breadcrumb.Robot != null)
                breadcrumb.Robot.ChangedControllerPathEvent += (s, o) => breadcrumb.OnPropertyChanged(nameof(FoldersCollection));

            breadcrumb.OnPropertyChanged(nameof(breadcrumb.Robot), nameof(breadcrumb.FoldersCollection));
        }

        #endregion

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

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

using ForRobot.Model.Controls;

namespace ForRobot.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для Breadcrumb.xaml
    /// </summary>
    public partial class Breadcrumb : ComboBox, INotifyPropertyChanged
    {
        #region Private variables

        #region Commands

        private static RelayCommand _onSelectedMenuItem;

        private static RelayCommand _onSelectedDirectory;

        private static RelayCommand _onHome;

        #endregion

        #endregion

        #region Public variables

        public bool CanOpenMenu { get => this.ItemsSource == null || this.ItemsSource.Cast<object>().Count() == 0 || this.SelectedFolder == null ? false 
                                            : this.ItemsSource.Cast<File>().First().Search(this.SelectedFolder).IncludeFileChildren; }

        /// <summary>
        /// Коллекция имен папок составляющих директорию
        /// </summary>
        public List<String> FoldersCollection { get => this.Directory.Split('\\').Where(x => x != this.Root.Replace("\\", "")).ToList<string>(); }

        /// <summary>
        /// Дочернии папки выбранного каталога
        /// </summary>
        public List<File> ChildrenFolder
        {
            get => (this.ItemsSource !=null && this.ItemsSource.Cast<object>().Count() > 0 && this.SelectedFolder != null) ?
                this.ItemsSource.Cast<File>().ToList().First().Search(this.SelectedFolder).Children.Where(item => item.Type == FileTypes.Folder).Cast<File>().ToList() : null;
        }

        #region Properties

        /// <summary>
        /// Идёт процесс закрузки файлов
        /// </summary>
        public bool IsProgress
        {
            get => (bool)GetValue(IsProgressProperty);
            set => SetValue(IsProgressProperty, value);
        }

        /// <summary>
        /// Корневой каталог
        /// </summary>
        public string Root
        {
            get => (string)GetValue(RootProperty);
            set => SetValue(RootProperty, value);
        }

        /// <summary>
        /// Директория выбранной папки
        /// </summary>
        public string Directory
        {
            get => (string)GetValue(DirectoryProperty);
            set => SetValue(DirectoryProperty, value);
        }

        /// <summary>
        /// Выбранная папка
        /// </summary>
        public string SelectedFolder
        {
            get => (string)GetValue(SelectedFolderProperty);
            set => SetValue(SelectedFolderProperty, value);
        }

        public System.Windows.Media.Brush IconsBackground
        {
            get => (System.Windows.Media.Brush)GetValue(IconsBackgroundProperty);
            set => SetValue(IconsBackgroundProperty, value);
        }

        #endregion

        #region Event

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Commands

        public RelayCommand SelectMenuItemCommand
        {
            get { return (RelayCommand)GetValue(SelectMenuItemCommandProperty); }
            set { SetValue(SelectMenuItemCommandProperty, value); }
        }

        public RelayCommand SelectDirectoryCommand
        {
            get { return (RelayCommand)GetValue(SelectMenuItemCommandProperty); }
            set { SetValue(SelectMenuItemCommandProperty, value); }
        }

        public RelayCommand HomeCommand
        {
            get { return (RelayCommand)GetValue(HomeCommandProperty); }
            set { SetValue(HomeCommandProperty, value); }
        }

        public RelayCommand UpDateCommand
        {
            get { return (RelayCommand)GetValue(UpDateCommandProperty); }
            set { SetValue(UpDateCommandProperty, value); }
        }

        #endregion

        #region Static readonly

        public static readonly DependencyProperty IsProgressProperty = DependencyProperty.Register(nameof(IsProgress),
                                                                                                   typeof(bool),
                                                                                                   typeof(Breadcrumb),
                                                                                                   new PropertyMetadata(false));

        public static readonly DependencyProperty RootProperty = DependencyProperty.Register(nameof(Root), 
                                                                                             typeof(string), 
                                                                                             typeof(Breadcrumb), 
                                                                                             new PropertyMetadata(@"KRC:\"));

        public static readonly DependencyProperty DirectoryProperty = DependencyProperty.Register(nameof(Directory),
                                                                                                  typeof(string),
                                                                                                  typeof(Breadcrumb),
                                                                                                  new PropertyMetadata(@"KRC:\R1", new PropertyChangedCallback(OnDirectoryValueChanged)));

        public static readonly DependencyProperty SelectedFolderProperty = DependencyProperty.Register(nameof(SelectedFolder),
                                                                                                       typeof(string),
                                                                                                       typeof(Breadcrumb),
                                                                                                       new PropertyMetadata("", new PropertyChangedCallback(OnSelectedFolderChanged)));

        public static readonly DependencyProperty IconsBackgroundProperty = DependencyProperty.Register(nameof(IconsBackground),
                                                                                                        typeof(System.Windows.Media.Brush),
                                                                                                        typeof(Breadcrumb), 
                                                                                                        new PropertyMetadata(System.Windows.Media.Brushes.Black));

        #region Commands

        public static readonly DependencyProperty SelectMenuItemCommandProperty = DependencyProperty.Register(nameof(SelectMenuItemCommand),
                                                                                                              typeof(RelayCommand),
                                                                                                              typeof(Breadcrumb),
                                                                                                              new PropertyMetadata(OnSelectedMenuItem));

        public static readonly DependencyProperty SelectDirectoryCommandProperty = DependencyProperty.Register(nameof(SelectDirectoryCommand),
                                                                                                               typeof(RelayCommand),
                                                                                                               typeof(Breadcrumb),
                                                                                                               new PropertyMetadata(OnSelectedDirectory));

        public static readonly DependencyProperty HomeCommandProperty = DependencyProperty.Register(nameof(HomeCommand),
                                                                                                    typeof(RelayCommand),
                                                                                                    typeof(Breadcrumb),
                                                                                                    new PropertyMetadata(OnHome));

        public static readonly DependencyProperty UpDateCommandProperty = DependencyProperty.Register(nameof(UpDateCommand),
                                                                                                      typeof(RelayCommand),
                                                                                                      typeof(Breadcrumb),
                                                                                                      new PropertyMetadata());

        #endregion

        #endregion

        #endregion

        #region Construct

        public Breadcrumb()
        {
            InitializeComponent();
        }

        #endregion

        #region Private functions

        private static void OnDirectoryValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Breadcrumb breadcrumb = (Breadcrumb)d;
            breadcrumb.Directory = (string)e.NewValue;
            breadcrumb.PropertyChanged?.Invoke(breadcrumb, new PropertyChangedEventArgs(nameof(breadcrumb.FoldersCollection)));
        }

        private static void OnSelectedFolderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Breadcrumb breadcrumb = (Breadcrumb)d;
            breadcrumb.SelectedFolder = (string)e.NewValue;
            breadcrumb.PropertyChanged?.Invoke(breadcrumb, new PropertyChangedEventArgs(nameof(breadcrumb.CanOpenMenu)));
            breadcrumb.PropertyChanged?.Invoke(breadcrumb, new PropertyChangedEventArgs(nameof(breadcrumb.ChildrenFolder)));
        }

        #region Commands

        /// <summary>
        /// Команда выбора в всплывающем меню
        /// </summary>
        private static RelayCommand OnSelectedMenuItem
        {
            get
            {
                return _onSelectedMenuItem ??
                    (_onSelectedMenuItem = new RelayCommand(obj =>
                    {
                        if(obj is object[] && ((object[])obj)[0]!=null && ((object[])obj)[1] != null)
                        {
                            Breadcrumb breadcrumb = ((object[])obj)[0] as Breadcrumb;
                            File file = ((object[])obj)[1] as File;
                            breadcrumb.Directory = breadcrumb.Root + file.Path.TrimEnd(new char[] { '\\' });
                        }
                    }));
            }
        }

        /// <summary>
        /// Команда выбора папки в адресной строке
        /// </summary>
        private static RelayCommand OnSelectedDirectory
        {
            get
            {
                return _onSelectedDirectory ??
                    (_onSelectedDirectory = new RelayCommand(obj =>
                    {
                        if (obj is object[] && ((object[])obj)[0] != null && ((obj as object[]).First() as ObservableCollection<File>).Count > 0
                                            && ((object[])obj)[1] != null && ((object[])obj)[2] != null)
                        {
                            File root = ((obj as object[]).First() as ObservableCollection<File>)[0];
                            string sSearchFolder = ((object[])obj)[1] as string;
                            Breadcrumb breadcrumb = ((object[])obj)[2] as Breadcrumb;
                            breadcrumb.Directory = breadcrumb.Root + root.Search(sSearchFolder).Path.TrimEnd(new char[] { '\\' });
                        }
                    }));
            }
        }

        /// <summary>
        /// Команда возврата к родительской папке
        /// </summary>
        private static RelayCommand OnHome
        {
            get
            {
                return _onHome ??
                    (_onHome = new RelayCommand(obj =>
                    {
                        Breadcrumb breadcrumb = obj as Breadcrumb;
                        breadcrumb.Directory = breadcrumb.Root + breadcrumb.FoldersCollection[0];
                    }));
            }
        }

        #endregion

        #endregion
    }
}

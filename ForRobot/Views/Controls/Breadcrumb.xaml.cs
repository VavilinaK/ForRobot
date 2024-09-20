using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ForRobot.Model.Controls;

namespace ForRobot.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для Breadcrumb.xaml
    /// </summary>
    public partial class Breadcrumb : ComboBox, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        //public void RaisePropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        //public void RaisePropertyChanged(params string[] propertyNames)
        //{
        //    foreach (var prop in propertyNames)
        //    {
        //        this.RaisePropertyChanged(prop);
        //    }
        //}

        //public virtual void OnPropertyChanged(string propertyName, object oldValue, object newValue) { }

        #region Private variables

        private string _selectedListItem;

        #endregion

        #region Public variables

        /// <summary>
        /// Коллекция имен папок составляющих директорию
        /// </summary>
        public List<String> FoldersCollection { get => this.Directory.Split('\\').Where(x => x != this.Root.Replace("\\", "")).ToList<string>(); }

        /// <summary>
        /// Содержание выбранной папки
        /// </summary>
        public List<File> FolderSource { get; set; }

        #region Properties

        //ImageBrush

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

        /// <summary>
        /// Дочернии папки выбранного каталога
        /// </summary>
        public List<File> ChildrenFolder { get => (this.ItemsSource.Cast<object>().Count()>0 && this.SelectedFolder!=null) ? 
                File.Search(this.ItemsSource.Cast<File>().ToList().First(), this.SelectedFolder).Children.Where(item => item.Type == ForRobot.Model.FileTypes.Folder).Cast<File>().ToList() : null; }

        ///// <summary>
        ///// 
        ///// </summary>
        //public new List<File> ItemsSource
        //{
        //    get => GetValue(ItemsSourceProperty) as List<File>;
        //    set => SetValue(ItemsSourceProperty, value);
        //}

        //this.Directory.Split('\\').Where(x => x != this.Root.Replace("\\", "")).ToList<string>()

        public System.Windows.Media.Brush IconsBackground
        {
            get => (System.Windows.Media.Brush)GetValue(IconsBackgroundProperty);
            set => SetValue(IconsBackgroundProperty, value);
        }

        #endregion

        #region Events

        #endregion

        #region Static readonly

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

        //public new static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource),
        //                                                                                                typeof(List<File>),
        //                                                                                                typeof(Breadcrumb));

        public static readonly DependencyProperty IconsBackgroundProperty = DependencyProperty.Register(nameof(IconsBackground),
                                                                                                        typeof(System.Windows.Media.Brush),
                                                                                                        typeof(Breadcrumb), 
                                                                                                        new PropertyMetadata(System.Windows.Media.Brushes.Black));
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
            breadcrumb.PropertyChanged?.Invoke(breadcrumb, new PropertyChangedEventArgs(nameof(breadcrumb.ChildrenFolder)));
        }

        #endregion
    }
}

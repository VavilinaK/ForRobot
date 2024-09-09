using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

namespace ForRobot.Model.Controls.NavigationTree
{
    public interface INavigationTreeItem : INotifyPropertyChanged
    {
        bool IsExpanded { get; set; }

        bool IncludeFileChildren { get; }

        /// <summary>
        /// Наименование файла
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Иконка файла
        /// </summary>
        BitmapSource Icon { get; set; }

        /// <summary>
        /// Схема Диск/Папка/Файл
        /// </summary>
        string FullPathFile { get; set; }

        ObservableCollection<INavigationTreeItem> Children { get; set; }

        void DeleteChildren();
    }
}

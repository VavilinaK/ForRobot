using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

namespace ForRobot.Model.Controls.NavigationTree
{
    public interface IFile : INotifyPropertyChanged
    {
        /// <summary>
        /// Тип
        /// </summary>
        FileTypes Type { get; }

        /// <summary>
        /// Является ли объект диском
        /// </summary>
        bool IsDrive { get; }

        /// <summary>
        /// Явряется ли объект папкой
        /// </summary>
        bool IsFolder { get; }

        /// <summary>
        /// Содержит ли файлы или подпапки
        /// </summary>
        bool IncludeFileChildren { get; }

        /// <summary>
        /// Наименование файла без расширения
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Имя файла с расширением
        /// </summary>
        string FullName { get; set; }

        /// <summary>
        /// Иконка файла
        /// </summary>
        BitmapSource Icon { get; }

        /// <summary>
        /// Схема Диск/Папка/Файл
        /// </summary>
        string FullPathFile { get; set; }

        ObservableCollection<IFile> Children { get; set; }

        //void DeleteChildren();
    }
}

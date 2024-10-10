using System;
using System.Linq;
using System.Windows.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ForRobot.Libr.Converters
{
    ///// <summary>
    ///// Вставляет Binding внутрь строки
    ///// </summary>
    ///// <example>
    ///// <code>Converter={StaticResource ResourceKey=StringFormat}, ConverterParameter='Hello {0}'</code>
    ///// </example>
    //public class StringFormatConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return string.Format(parameter as string, value);
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //}

    /// <summary>
    /// Скрывает папки на роботе, запрещённые настройками
    /// </summary>
    public class VisibilityFolderOnRobot : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<ForRobot.Model.Controls.File> files = values[0] as ObservableCollection<ForRobot.Model.Controls.File>; // Коллекция файлов на роботе.
            SortedDictionary<string, bool> settings = values[1] as SortedDictionary<string, bool>; // Коллекция доступных для блокировки папок из настроек.

            if(files != null)
            {
                var set = settings.Where(x => !x.Value).Select(s => s.Key).ToList<string>();
                var v = files.Where(t2 => !set.Any(t1 => t2.Path.Contains(t1)));
            }
            //foreach (var file in files.Where(t2 => !App.Settings.AvailableFolders.Where(x => !x.Value).Select(s => s.Key).ToList<string>().Any(t1 => t2.Key.Contains(t1))))
            //var v = files.Where(t2 => !settings.Where(x => !x.Value).Select(s => s.Key).ToList<string>().Any(t1 => t2.Path.Contains(t1)));

            return files;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}

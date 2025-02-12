using System;
using System.Linq;
using System.Windows.Data;
using System.Globalization;
using System.Collections.Generic;

namespace ForRobot.Libr.Converters
{
    /// <summary>
    /// Класс преобразователь для вытаскивания значения типа <see cref="Boolean" из коллекции типа <see cref="SortedDictionary{TKey, TValue}"/>/>
    /// </summary>

    //public class CollectionsStringToBoolConverter : IMultiValueConverter
    //{
    //    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var v1 = values[0] as SortedDictionary<string, bool>;

    //        return v1.Where(item => item.Key == values[1].ToString()).First().Value;
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class CollectionsStringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var col = value as SortedDictionary<string, bool>;
            string key = parameter as string;

            return col.Where(item => item.Key == key.TrimStart().TrimEnd()).First().Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

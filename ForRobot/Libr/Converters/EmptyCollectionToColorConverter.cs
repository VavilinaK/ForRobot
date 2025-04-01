using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections;
using System.Globalization;

namespace ForRobot.Libr.Converters
{
    public class EmptyCollectionToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = value as IEnumerable;
            return collection?.Cast<object>().Any() == true ? Brushes.Transparent : Brushes.LightGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

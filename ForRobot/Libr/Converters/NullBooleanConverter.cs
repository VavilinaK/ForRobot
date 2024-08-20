using System;
using System.Windows.Data;
using System.Globalization;

namespace ForRobot.Libr.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class NullBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            return value;
        }
    }
}

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
            IComparable v1 = value as IComparable;
            return v1 == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

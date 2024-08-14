using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace ForRobot.Libr.Converters
{
    public class ThicknessConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var left = (int)values[0];
            var top = (int)values[1];
            var right = (int)values[2];
            var bottom = (int)values[3];
            return new Thickness(left, top, right, bottom);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Globalization;

namespace ForRobot.Libr.Converters
{
    /// <summary>
    /// Класс преобразования коэффициента маштабирования 3д модели
    /// </summary>
    public class ScaleFactorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IComparable v = value as IComparable;

            if (v == null || !(v is decimal))
                throw new FormatException("to use this converter, value and parameter shall inherit from Decimal");

            return (1.00M / (decimal)value).ToString();
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IComparable v = value as IComparable;

            if (v == null || !(v is string))
                throw new FormatException("to use this converter, value and parameter shall inherit from String");

            if (decimal.TryParse(v as string, NumberStyles.Any, culture, out decimal result))
            {
                return 1.00M / result;
            }

            return DependencyProperty.UnsetValue;
        }
    }
}

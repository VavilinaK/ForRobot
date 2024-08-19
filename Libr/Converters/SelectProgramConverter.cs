using System.Windows.Data;
using System;

namespace ForRobot.Libr.Converters
{
    /// <summary>
    /// Класс проверки равенства имени файла и имени выбранной прогаммы
    /// </summary>
    public class SelectProgramConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IComparable v1 = values[0] as IComparable; // Имя файла.
            IComparable v2 = values[1] as IComparable; // Имя выбранной программы

            if (v1 == null || v2 == null)
                throw new FormatException("to use this converter, SelectProgramConverter, value and parameter shall inherit from String");

            return (((string)v1).Split(new char[] { '.' })[0]).ToLower() == ((string)v2).ToLower();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException("IsEqualThanConverter can only be used OneWay.");
        }
    }
}

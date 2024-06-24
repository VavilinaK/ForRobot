using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Data;
using System.Globalization;
using System;

namespace ForRobot.Libr
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IComparable v1 = value as IComparable;

            if (value is string == false)
                throw new FormatException("to use this converter, value and parameter shall inherit from String.");

            if (!string.IsNullOrWhiteSpace((string)v1) && Regex.IsMatch((string)value, @"^Нет соединения", RegexOptions.Compiled))
                return new SolidColorBrush(Color.FromRgb(111, 82, 255));

            if (!string.IsNullOrWhiteSpace((string)v1) && Regex.IsMatch((string)value, @"^Программа не выбрана", RegexOptions.Compiled))
                return new SolidColorBrush(Color.FromRgb(10, 122, 255));

            if (!string.IsNullOrWhiteSpace((string)v1) && Regex.IsMatch((string)value, @"^Выбрана программа \w*", RegexOptions.Compiled))
                return new SolidColorBrush(Color.FromRgb(247, 153, 0));

            if (!string.IsNullOrWhiteSpace((string)v1) && Regex.IsMatch((string)value, @"^Запущена программа \w*", RegexOptions.Compiled))
                return new SolidColorBrush(Color.FromRgb(0, 183, 56));

            if (!string.IsNullOrWhiteSpace((string)v1) && Regex.IsMatch((string)value, @"\w* остановлена$", RegexOptions.Compiled))
                return new SolidColorBrush(Color.FromRgb(246, 77, 0));

            if (!string.IsNullOrWhiteSpace((string)v1) && Regex.IsMatch((string)value, @"\w* завершена$", RegexOptions.Compiled))
                return new SolidColorBrush(Color.FromRgb(221, 0, 0));
            else
                return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is SolidColorBrush))
                throw new FormatException("To use this convertBack, value and parameter shall inherit from SolidColorBrush");

            if (((SolidColorBrush)value).Color == Color.FromRgb(111, 82, 255))
                return "Нет соединения";

            else if (((SolidColorBrush)value).Color == Color.FromRgb(10, 122, 255))
                return "Программа не выбрана";

            else if (((SolidColorBrush)value).Color == Color.FromRgb(247, 153, 0))
                return "Выбрана программа";

            else if (((SolidColorBrush)value).Color == Color.FromRgb(0, 183, 56))
                return "Запущена программа";

            else if (((SolidColorBrush)value).Color == Color.FromRgb(246, 77, 0))
                return "Программа остановлена";

            else if (((SolidColorBrush)value).Color == Color.FromRgb(221, 0, 0))
                return "Программа завершена";
            else
                throw new Exception(string.Format("Cannot convert, unknown value {0}", value));
        }
    }
}

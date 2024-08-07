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
            //IComparable v1 = value as IComparable;

            //if (!(value is string))
            //    throw new FormatException("to use this converter, value and parameter shall inherit from String.");

            if (!string.IsNullOrWhiteSpace((string)value) && Regex.IsMatch((string)value, @"^Нет соединения", RegexOptions.Compiled))
                return new SolidColorBrush(Color.FromRgb(178, 34, 34));

            if (!string.IsNullOrWhiteSpace((string)value) && Regex.IsMatch((string)value, @"^Программа не выбрана", RegexOptions.Compiled))
                return new SolidColorBrush(Color.FromRgb(72, 155, 255));

            if (!string.IsNullOrWhiteSpace((string)value) && Regex.IsMatch((string)value, @"^Выбрана программа \w*", RegexOptions.Compiled))
                return new SolidColorBrush(Color.FromRgb(207, 193, 0));

            if (!string.IsNullOrWhiteSpace((string)value) && Regex.IsMatch((string)value, @"^Запущена программа \w*", RegexOptions.Compiled))
                return new SolidColorBrush(Color.FromRgb(0, 214, 118));

            if (!string.IsNullOrWhiteSpace((string)value) && Regex.IsMatch((string)value, @"\w* остановлена$", RegexOptions.Compiled))
                return new SolidColorBrush(Color.FromRgb(255, 56, 56));

            if (!string.IsNullOrWhiteSpace((string)value) && Regex.IsMatch((string)value, @"\w* завершена$", RegexOptions.Compiled))
                return new SolidColorBrush(Color.FromRgb(95, 255, 51));
            else
                return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is SolidColorBrush))
                throw new FormatException("To use this convertBack, value and parameter shall inherit from SolidColorBrush");

            if (((SolidColorBrush)value).Color == Color.FromRgb(255, 56, 56))
                return "Программа не выбрана";

            else if (((SolidColorBrush)value).Color == Color.FromRgb(255, 252, 51))
                return "Выбрана программа";

            else if (((SolidColorBrush)value).Color == Color.FromRgb(0, 214, 118))
                return "Запущена программа";

            else if (((SolidColorBrush)value).Color == Color.FromRgb(255, 56, 56))
                return "Остановлена программа";

            else if (((SolidColorBrush)value).Color == Color.FromRgb(95, 255, 51))
                return "Завершена программаа";
            else
                throw new Exception(string.Format("Cannot convert, unknown value {0}", value));
        }
    }
}

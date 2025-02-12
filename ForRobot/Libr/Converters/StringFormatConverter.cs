using System.Windows.Data;
using System;
using System.Globalization;

namespace ForRobot.Libr.Converters
{
    /// <summary>
    /// Вставляет Binding внутрь строки
    /// </summary>
    /// <example>
    /// <code>Converter={StaticResource ResourceKey=StringFormat}, ConverterParameter='Hello {0}'</code>
    /// </example>
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format(parameter as string, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// Добавление MultiBinding в строку
    /// </summary>
    public class StringFormatMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Format(parameter as string, values);
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

using System;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.Generic;

namespace ForRobot.Libr.Converters
{
    /// <summary>
    /// Класс-преобразователь словаря цветов в настройках приложения
    /// </summary>
    public class DictionaryToBindableCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Dictionary<string, Color> dictionary)
            {
                return dictionary.Keys
                                 .Select(key => new ForRobot.Model.File3D.PropertyColor(key, dictionary.Where(item => item.Key == key).First().Value))
                                 .ToList();
            }
            return Enumerable.Empty<ForRobot.Model.File3D.PropertyColor>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                return parameter;
            }
            else
                return null;
        }
    }
}

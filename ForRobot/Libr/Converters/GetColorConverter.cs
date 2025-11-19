using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Globalization;

namespace ForRobot.Libr.Converters
{
    /// <summary>
    /// Класс преобразователь для выгрузки цвета из <see cref="ForRobot.Models.File3D.Colors"/> по имени свойства
    /// </summary>
    public class GetColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string propName = parameter as string;
            return propName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

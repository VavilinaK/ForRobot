using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Globalization;

namespace ForRobot.Libr.Converters
{
    /// <summary>
    /// Класс преобразователь из <see cref="System.Windows.Media.Color"/> в <see cref="System.Windows.Media.Brush"/>
    /// </summary>
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color v = (Color)value;

            if (v == null)
                throw new FormatException("to use this converter, value and parameter shall inherit from Color");

            return new System.Windows.Media.SolidColorBrush(v);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush v = (Brush)value;

            if (v == null)
                throw new FormatException("to use this converter, value and parameter shall inherit from Color");

            return ((SolidColorBrush)v).Color;
        }
    }
}

using System;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Globalization;

namespace ForRobot.Libr.Converters
{
    public class ColorSaveConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TextBlock textBlock = (TextBlock)parameter;
            App.Current.Settings.Colors[textBlock.Text] = (Color)value;
            return (Color)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TextBlock textBlock = (TextBlock)parameter;
            App.Current.Settings.Colors[textBlock.Text] = (Color)value;
            return (Color)value;
        }
    }
}

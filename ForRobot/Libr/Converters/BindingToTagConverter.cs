using System;
using System.Windows.Data;
using System.Windows.Controls;
using System.Globalization;

namespace ForRobot.Libr.Converters
{
    public class BindingToTagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                throw new FormatException("to use this converter, value shall inherit from System.Windows.Controls");

            switch (value)
            {
                case TextBox textBox:
                    return ((TextBox)value).GetBindingExpression(TextBox.TextProperty).ParentBinding.Path.Path;

                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

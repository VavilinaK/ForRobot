using System;
using System.Windows.Data;

namespace ForRobot.Libr.Converters
{
    /// <summary>
    /// Класс-преобразователь для RadioButton
    /// </summary>
    public class RadioButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null)
                throw new FormatException("to use this converter, value and parameter shall inherit from Object");
            
            if (value is Boolean && parameter is Boolean)
                return ((bool)parameter == (bool)value);
            else if (value.GetType() == parameter.GetType())
                return value.Equals(parameter);
            //else if (value is ForRobot.Libr.Settings.ModeClosingApp && parameter is ForRobot.Libr.Settings.ModeClosingApp)
            //    return ((ForRobot.Libr.Settings.ModeClosingApp)parameter == (ForRobot.Libr.Settings.ModeClosingApp)value);
            else
                return parameter.ToString() == value.ToString();
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

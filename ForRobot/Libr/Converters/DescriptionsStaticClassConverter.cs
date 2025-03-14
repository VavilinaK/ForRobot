using System;
using System.Linq;
using System.Windows.Data;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ForRobot.Model.Detals;

namespace ForRobot.Libr.Converters
{
    public class DescriptionsStaticClassConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
                throw new InvalidOperationException("This converter class can only be used with Class Fields elements.");

            var v = typeof(ForRobot.Model.Detals.ScoseTypes).GetFields().Where(field => field.GetValue(value) == value);
            var v1 = v.Select(field => field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).SingleOrDefault() as System.ComponentModel.DescriptionAttribute).First();
            return v1.Description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ForRobot.Model.Detals.ScoseTypes.FieldByDescription(value as string);
        }
    }
}

using System;
using System.Linq;
using System.Windows.Data;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ForRobot.Models.Detals;

namespace ForRobot.Libr.Converters
{
    public class DescriptionsStaticClassConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
                throw new InvalidOperationException("This converter class can only be used with Class Fields elements.");
            
            FieldInfo[] constants = typeof(ScoseTypes).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
                                                      .Where(f => f.IsLiteral)
                                                      .ToArray();

            var v = constants.Where(field => {
                try
                {
                    object fieldValue = field.GetRawConstantValue();
                    return fieldValue != null
                           && fieldValue.GetType().IsInstanceOfType(value) // Проверка типа
                           && fieldValue.Equals(value);
                }
                catch
                {
                    return false;
                }
            });

            var v1 = v.Select(field => field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).SingleOrDefault() as System.ComponentModel.DescriptionAttribute).First();
            return v1.Description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ForRobot.Models.Detals.ScoseTypes.FieldByDescription(value as string);
        }
    }
}

using System;
using System.Windows.Data;
using System.Globalization;

using ForRobot.Model.Controls;

namespace ForRobot.Libr.Converters
{
    public class VisibiliityMenuToggleButtonConvertrer : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            File file = values[0] as File;
            string sNameFolder = values[1] as string;

            if(file == null || sNameFolder == string.Empty)
                throw new InvalidOperationException("To use this converter, value and parameter shall inherit from Object.");

            return File.Search(file, sNameFolder).IncludeFileChildren;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

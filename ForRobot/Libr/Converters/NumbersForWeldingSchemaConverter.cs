using System;
using System.Windows.Data;
using System.Windows.Controls;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ForRobot.Libr.Converters
{
    internal class NumbersForWeldingSchemaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataGrid dataGrid = value as DataGrid;
            List<string> items = new List<string>() { "-" };
            for(int i = 1; i <= dataGrid.Items.Count * 2; i++)
            {
                items.Add(i.ToString());
            }
            return new ObservableCollection<string>(items);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

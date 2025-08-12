using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace ForRobot.Themes
{
    public partial class MetroStyleResourceDictionary
    {
        /// <summary>
        /// Метод выделения содержимого TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Text_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null)
                textBox.SelectAll();
        }

        /// <summary>
        /// Метод выделения содержимого TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null && !textBox.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                textBox.Focus();
            }
        }

        /// <summary>
        /// Метод изменения ширины столбцов вслед за изменением DataGrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged) return;

            DataGrid dataGrid = sender as DataGrid;
            
            dataGrid.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            dataGrid.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            dataGrid.Columns[2].Width = new DataGridLength(1, DataGridLengthUnitType.Star);

            dataGrid.UpdateLayout();
        }
    }
}

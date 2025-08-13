using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;

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

            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                // Рассчитываем доступное пространство для пропорциональных столбцов
                double totalWidth = dataGrid.ActualWidth - SystemParameters.VerticalScrollBarWidth;
                double centerWidth = dataGrid.Columns[1].ActualWidth; // Текущая ширина центрального столбца

                // Вычитаем ширину центрального столбца
                double availableWidth = totalWidth - centerWidth;

                // Устанавливаем пропорции для левого и правого столбцов
                dataGrid.Columns[0].Width = new DataGridLength(availableWidth / 2, DataGridLengthUnitType.Pixel);
                dataGrid.Columns[2].Width = new DataGridLength(availableWidth / 2, DataGridLengthUnitType.Pixel);
            }), DispatcherPriority.Render);
        }
    }
}

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;

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
            if (dataGrid == null || dataGrid.Columns.Count == 0) return;

            System.Windows.Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                AdjustDataGridColumns(dataGrid);
                // Рассчитываем доступное пространство для пропорциональных столбцов
                //double totalWidth = dataGrid.ActualWidth - SystemParameters.VerticalScrollBarWidth;

                //double centerWidth = dataGrid.Columns[1].ActualWidth; // Текущая ширина центрального столбца

                //// Вычитаем ширину центрального столбца
                //double availableWidth = totalWidth - centerWidth;

                //// Устанавливаем пропорции для левого и правого столбцов
                //dataGrid.Columns[0].Width = new DataGridLength(availableWidth / 2, DataGridLengthUnitType.Pixel);
                //dataGrid.Columns[2].Width = new DataGridLength(availableWidth / 2, DataGridLengthUnitType.Pixel);
            }), DispatcherPriority.Render);
        }

        private void AdjustDataGridColumns(DataGrid dataGrid)
        {
            double scrollbarWidth = dataGrid.VerticalScrollBarVisibility == ScrollBarVisibility.Visible ? SystemParameters.VerticalScrollBarWidth : 0;

            double totalWidth = dataGrid.ActualWidth - scrollbarWidth - dataGrid.RowHeaderActualWidth;

            //var fixedColumns = new List<DataGridColumn>();
            //var proportionalColumns = new List<DataGridColumn>();
            var proportionalColumns = new List<int>();

            foreach (var column in dataGrid.Columns)
            {
                if (dataGrid.Tag != null)
                    break;

                if (column.Width.IsAbsolute || column.Width.IsSizeToCells || column.Width.IsSizeToHeader)
                {
                    //fixedColumns.Add(column);
                }
                else
                {
                    proportionalColumns.Add(column.DisplayIndex);
                }
            }

            foreach(int index in proportionalColumns)
            {
                double proportionalWidth = totalWidth / proportionalColumns.Count;
                //dataGrid.Columns[index].Width = new DataGridLength(proportionalWidth, DataGridLengthUnitType.Pixel);
                dataGrid.Columns[index].Width = new DataGridLength(proportionalWidth, DataGridLengthUnitType.Star);
            }

            //double fixedWidth = fixedColumns.Sum(c => c.ActualWidth); // Общая ширина фиксированных столбцов
                        
            //double availableWidth = Math.Max(0, totalWidth - fixedWidth); // Доступная ширина для пропорциональных столбцов
            
            //if (proportionalColumns.Count > 0)
            //{
            //    double proportionalWidth = availableWidth / proportionalColumns.Count;

            //    foreach (var column in proportionalColumns)
            //    {
            //        column.Width = new DataGridLength(proportionalWidth, DataGridLengthUnitType.Pixel);
            //    }
            //}
        }
    }
}

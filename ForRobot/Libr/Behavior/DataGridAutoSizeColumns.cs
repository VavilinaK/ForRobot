using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;

namespace ForRobot.Libr.Behavior
{
    public static class DataGridAutoSizeColumns
    {
        private static List<Tuple<int, DataGridLength>> _fixedColumns;
        private static List<int> _autoColumns;
        private static List<int> _proportionalColumns;

        public static readonly DependencyProperty IsDynamicWithProperty = DependencyProperty.RegisterAttached("IsDynamicWith",
                                                                                                              typeof(bool),
                                                                                                              typeof(DataGridAutoSizeColumns),
                                                                                                              new PropertyMetadata(false, OnIsDynamicWithChanged));

        public static void SetIsDynamicWith(DependencyObject element, bool value) => element.SetValue(IsDynamicWithProperty, value);
        public static bool GetIsDynamicWith(DependencyObject element) => (bool)element.GetValue(IsDynamicWithProperty);

        private static void OnIsDynamicWithChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if ((bool)e.NewValue)
                {
                    dataGrid.Loaded += DataGrid_Loaded;
                    dataGrid.SizeChanged += DataGrid_SizeChanged;
                }
                else
                {
                    dataGrid.Loaded -= DataGrid_Loaded;
                    dataGrid.SizeChanged -= DataGrid_SizeChanged;
                }
            }
        }

        private static void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                SaveDataGridColumnsWidth(dataGrid);
                UpdateDataGridColumnsWidth(dataGrid);
            }
        }

        private static void DataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid && e.WidthChanged)
                UpdateDataGridColumnsWidth(dataGrid);
        }

        private static void SaveDataGridColumnsWidth(DataGrid dataGrid)
        {
            _fixedColumns = new List<Tuple<int, DataGridLength>>();
            _autoColumns = new List<int>();
            _proportionalColumns = new List<int>();

            for(int i=0; i<dataGrid.Columns.Count; i++)
            {
                var column = dataGrid.Columns[i];

                if (column.Width.IsAbsolute)
                {
                    _fixedColumns.Add(Tuple.Create<int, DataGridLength>(i, column.Width));

                }
                else if(column.Width.IsAuto)
                {
                    _autoColumns.Add(i);
                }
                else if(column.Width.IsStar)
                {
                    _proportionalColumns.Add(i);
                }
            }
        }

        private static void UpdateDataGridColumnsWidth(DataGrid dataGrid)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() => 
            {
                double scrollbarWidth = dataGrid.VerticalScrollBarVisibility == ScrollBarVisibility.Visible ? SystemParameters.VerticalScrollBarWidth : 0;
                double totalWidth = Math.Max(0, dataGrid.ActualWidth - scrollbarWidth - dataGrid.RowHeaderActualWidth);

                double fixedWidth = _fixedColumns.Sum(x => x.Item1); // Общая ширина фиксированных столбцов

                double autoWidth = 0;
                foreach (int columnIndex in _autoColumns)
                {
                    autoWidth += dataGrid.Columns[columnIndex].ActualWidth;
                }

                double availableWidth = Math.Max(0, totalWidth - fixedWidth - autoWidth); // Ширина пропорцеональных столбцов

                foreach(int i in _proportionalColumns)
                {
                    dataGrid.Columns[i].Width = new DataGridLength(availableWidth / _proportionalColumns.Count, DataGridLengthUnitType.Pixel);
                }

            }), DispatcherPriority.Render);
        }

        //public static readonly DependencyProperty WidthRatioProperty = DependencyProperty.RegisterAttached(
        //   "WidthRatio",
        //   typeof(double),
        //   typeof(DataGridColumnWidthHelper),
        //   new PropertyMetadata(1.0, OnWidthRatioChanged));

        //public static void SetWidthRatio(DependencyObject element, double value) => element.SetValue(WidthRatioProperty, value);
        //public static double GetWidthRatio(DependencyObject element) => (double)element.GetValue(WidthRatioProperty);

        //public static readonly DependencyProperty IsEnabledProperty =
        //    DependencyProperty.RegisterAttached(
        //        "IsEnabled",
        //        typeof(bool),
        //        typeof(DataGridColumnWidthHelper),
        //        new PropertyMetadata(false, OnIsEnabledChanged));

        //public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);
        //public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

        //private static void OnWidthRatioChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (d is DataGridColumn column && GetIsEnabled(column))
        //        UpdateColumnWidths(column);
        //}

        //private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (d is DataGrid dataGrid)
        //    {
        //        if ((bool)e.NewValue)
        //        {
        //            dataGrid.Loaded += DataGrid_Loaded;
        //            dataGrid.SizeChanged += DataGrid_SizeChanged;
        //        }
        //        else
        //        {
        //            dataGrid.Loaded -= DataGrid_Loaded;
        //            dataGrid.SizeChanged -= DataGrid_SizeChanged;
        //        }
        //    }
        //}

        //private static void DataGrid_Loaded(object sender, RoutedEventArgs e)
        //{
        //    if (sender is DataGrid dataGrid)
        //        UpdateDataGridColumnsWidth(dataGrid);
        //}

        //private static void DataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    if (sender is DataGrid dataGrid && e.WidthChanged)
        //        UpdateDataGridColumnsWidth(dataGrid);
        //}

        //private static void UpdateDataGridColumnsWidth(DataGrid dataGrid)
        //{
        //    // Временно отключаем обновление для избежания рекурсии
        //    dataGrid.SizeChanged -= DataGrid_SizeChanged;

        //    double totalRatio = dataGrid.Columns
        //        .Where(c => GetIsEnabled(c))
        //        .Sum(GetWidthRatio);

        //    double availableWidth = dataGrid.ActualWidth - GetNonStarWidth(dataGrid);

        //    foreach (var column in dataGrid.Columns)
        //    {
        //        if (GetIsEnabled(column))
        //        {
        //            double newWidth = availableWidth * (GetWidthRatio(column) / totalRatio);
        //            column.Width = newWidth;
        //        }
        //    }

        //    dataGrid.SizeChanged += DataGrid_SizeChanged;
        //}

        //private static double GetNonStarWidth(DataGrid dataGrid)
        //{
        //    // Учитываем ширину не-* колонок и вертикального скроллбара
        //    double nonStarWidth = dataGrid.Columns
        //        .Where(c => !GetIsEnabled(c))
        //        .Sum(c => c.ActualWidth);

        //    if (dataGrid.VerticalScrollBarVisibility == ScrollBarVisibility.Auto ||
        //        dataGrid.VerticalScrollBarVisibility == ScrollBarVisibility.Visible)
        //    {
        //        nonStarWidth += SystemParameters.VerticalScrollBarWidth;
        //    }

        //    return nonStarWidth;
        //}

        //private static void UpdateColumnWidths(DataGridColumn column)
        //{
        //    if (column.DataGridOwner is DataGrid dataGrid)
        //        UpdateDataGridColumnsWidth(dataGrid);
        //}
    }
}
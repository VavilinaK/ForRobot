using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ForRobot.Libr.Behavior
{
    public static class DataGridAutoSizeColumns
    {
        private static readonly ConditionalWeakTable<DataGrid, DataGridColumnInfo> DataGridInfos = new ConditionalWeakTable<DataGrid, DataGridColumnInfo>();

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
                    var info = new DataGridColumnInfo(dataGrid);
                    DataGridInfos.Add(dataGrid, info);

                    dataGrid.Loaded += DataGrid_Loaded;
                    dataGrid.SizeChanged += DataGrid_SizeChanged;
                    dataGrid.Unloaded += DataGrid_Unloaded;
                }
                else
                {
                    dataGrid.Loaded -= DataGrid_Loaded;
                    dataGrid.SizeChanged -= DataGrid_SizeChanged;
                    dataGrid.Unloaded -= DataGrid_Unloaded;

                    if (DataGridInfos.TryGetValue(dataGrid, out var info))
                    {
                        info.Dispose();
                        DataGridInfos.Remove(dataGrid);
                    }
                }
            }
        }

        private static void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid && DataGridInfos.TryGetValue(dataGrid, out var info))
            {
                info.SaveDataGridColumnsWidth();
                info.UpdateDataGridColumnsWidth();
            }
        }

        private static void DataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid && e.WidthChanged && DataGridInfos.TryGetValue(dataGrid, out var info))
            {
                info.UpdateDataGridColumnsWidth();
            }
        }

        private static void DataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid && DataGridInfos.TryGetValue(dataGrid, out var info))
            {
                info.Dispose();
                DataGridInfos.Remove(dataGrid);
            }
        }
        
        private class DataGridColumnInfo : IDisposable
        {
            private readonly DataGrid _dataGrid;
            private List<Tuple<int, double>> _fixedColumns;
            private List<int> _autoColumns;
            private List<int> _proportionalColumns;
            private bool _isUpdating;
            private double _lastProcessedWidth = -1;

            public DataGridColumnInfo(DataGrid dataGrid)
            {
                _dataGrid = dataGrid;
            }

            public void SaveDataGridColumnsWidth()
            {
                _fixedColumns = new List<Tuple<int, double>>();
                _autoColumns = new List<int>();
                _proportionalColumns = new List<int>();

                for (int i = 0; i < _dataGrid.Columns.Count; i++)
                {
                    var column = _dataGrid.Columns[i];

                    if (column.Width.IsAbsolute)
                    {
                        _fixedColumns.Add(Tuple.Create(i, column.Width.Value));
                    }
                    else if (column.Width.IsAuto)
                    {
                        _autoColumns.Add(i);
                    }
                    else if (column.Width.IsStar)
                    {
                        _proportionalColumns.Add(i);
                    }
                }
            }

            public void UpdateDataGridColumnsWidth()
            {
                if (_isUpdating || _fixedColumns == null) return;

                _isUpdating = true;

                try
                {
                    if (Math.Abs(_lastProcessedWidth - _dataGrid.ActualWidth) < 1.0) return;

                    _lastProcessedWidth = _dataGrid.ActualWidth;

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        try
                        {
                            double scrollbarWidth = _dataGrid.VerticalScrollBarVisibility == ScrollBarVisibility.Visible ?
                                SystemParameters.VerticalScrollBarWidth : 0;
                            double totalWidth = Math.Max(0, _dataGrid.ActualWidth - scrollbarWidth - _dataGrid.RowHeaderActualWidth);
                            
                            double fixedWidth = _fixedColumns.Sum(x => x.Item2);

                            double autoWidth = 0;
                            foreach (int columnIndex in _autoColumns)
                            {
                                autoWidth += _dataGrid.Columns[columnIndex].ActualWidth;
                            }

                            double availableWidth = Math.Max(0, totalWidth - fixedWidth - autoWidth);

                            if (_proportionalColumns.Count > 0)
                            {
                                double columnWidth = availableWidth / _proportionalColumns.Count;
                                
                                columnWidth = Math.Max(50, columnWidth);

                                foreach (int i in _proportionalColumns)
                                {
                                    _dataGrid.Columns[i].Width = new DataGridLength(columnWidth, DataGridLengthUnitType.Pixel);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error updating DataGrid columns: {ex.Message}");
                        }
                        finally
                        {
                            _isUpdating = false;
                        }
                    }), DispatcherPriority.Render);
                }
                catch (Exception ex)
                {
                    _isUpdating = false;
                    System.Diagnostics.Debug.WriteLine($"Error in UpdateDataGridColumnsWidth: {ex.Message}");
                }
            }

            public void Dispose()
            {
                _fixedColumns = null;
                _autoColumns = null;
                _proportionalColumns = null;
            }
        }

        //private static List<Tuple<int, DataGridLength>> _fixedColumns;
        //private static List<int> _autoColumns;
        //private static List<int> _proportionalColumns;

        //public static readonly DependencyProperty IsDynamicWithProperty = DependencyProperty.RegisterAttached("IsDynamicWith",
        //                                                                                                      typeof(bool),
        //                                                                                                      typeof(DataGridAutoSizeColumns),
        //                                                                                                      new PropertyMetadata(false, OnIsDynamicWithChanged));

        //public static void SetIsDynamicWith(DependencyObject element, bool value) => element.SetValue(IsDynamicWithProperty, value);
        //public static bool GetIsDynamicWith(DependencyObject element) => (bool)element.GetValue(IsDynamicWithProperty);

        //private static void OnIsDynamicWithChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
        //    {
        //        SaveDataGridColumnsWidth(dataGrid);
        //        UpdateDataGridColumnsWidth(dataGrid);
        //    }
        //}

        //private static void DataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    if (sender is DataGrid dataGrid && e.WidthChanged)
        //        UpdateDataGridColumnsWidth(dataGrid);
        //}

        //private static void SaveDataGridColumnsWidth(DataGrid dataGrid)
        //{
        //    _fixedColumns = new List<Tuple<int, DataGridLength>>();
        //    _autoColumns = new List<int>();
        //    _proportionalColumns = new List<int>();

        //    for(int i=0; i<dataGrid.Columns.Count; i++)
        //    {
        //        var column = dataGrid.Columns[i];

        //        if (column.Width.IsAbsolute)
        //        {
        //            _fixedColumns.Add(Tuple.Create<int, DataGridLength>(i, column.Width));
        //        }
        //        else if(column.Width.IsAuto)
        //        {
        //            _autoColumns.Add(i);
        //        }
        //        else if(column.Width.IsStar)
        //        {
        //            _proportionalColumns.Add(i);
        //        }
        //    }
        //}

        //private static void UpdateDataGridColumnsWidth(DataGrid dataGrid)
        //{
        //    Application.Current.Dispatcher.BeginInvoke((Action)(() => 
        //    {
        //        double scrollbarWidth = dataGrid.VerticalScrollBarVisibility == ScrollBarVisibility.Visible ? SystemParameters.VerticalScrollBarWidth : 0;
        //        double totalWidth = Math.Max(0, dataGrid.ActualWidth - scrollbarWidth - dataGrid.RowHeaderActualWidth);

        //        double fixedWidth = _fixedColumns.Sum(x => x.Item1); // Общая ширина фиксированных столбцов

        //        double autoWidth = 0;
        //        foreach (int columnIndex in _autoColumns)
        //        {
        //            autoWidth += dataGrid.Columns[columnIndex].ActualWidth;
        //        }

        //        double availableWidth = Math.Max(0, totalWidth - fixedWidth - autoWidth); // Ширина пропорцеональных столбцов

        //        foreach(int i in _proportionalColumns)
        //        {
        //            dataGrid.Columns[i].Width = new DataGridLength(availableWidth / _proportionalColumns.Count, DataGridLengthUnitType.Pixel);
        //        }

        //    }), DispatcherPriority.Render);
        //}        
    }
}
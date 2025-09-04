using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Runtime.CompilerServices;

namespace ForRobot.Libr.Behavior
{
    public class DataGridAutoSizeColumns : DependencyObject
    {
        public static readonly DependencyProperty WidthRatioProperty =
       DependencyProperty.RegisterAttached(
           "WidthRatio",
           typeof(double),
           typeof(DataGridColumnWidthHelper),
           new PropertyMetadata(1.0, OnWidthRatioChanged));

        public static void SetWidthRatio(DependencyObject element, double value) => element.SetValue(WidthRatioProperty, value);
        public static double GetWidthRatio(DependencyObject element) => (double)element.GetValue(WidthRatioProperty);
        
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(DataGridColumnWidthHelper),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);
        public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

        private static void OnWidthRatioChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGridColumn column && GetIsEnabled(column))
                UpdateColumnWidths(column);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
                UpdateDataGridColumnsWidth(dataGrid);
        }

        private static void DataGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid && e.WidthChanged)
                UpdateDataGridColumnsWidth(dataGrid);
        }

        private static void UpdateDataGridColumnsWidth(DataGrid dataGrid)
        {
            // Временно отключаем обновление для избежания рекурсии
            dataGrid.SizeChanged -= DataGrid_SizeChanged;

            double totalRatio = dataGrid.Columns
                .Where(c => GetIsEnabled(c))
                .Sum(GetWidthRatio);

            double availableWidth = dataGrid.ActualWidth - GetNonStarWidth(dataGrid);

            foreach (var column in dataGrid.Columns)
            {
                if (GetIsEnabled(column))
                {
                    double newWidth = availableWidth * (GetWidthRatio(column) / totalRatio);
                    column.Width = newWidth;
                }
            }

            dataGrid.SizeChanged += DataGrid_SizeChanged;
        }

        private static double GetNonStarWidth(DataGrid dataGrid)
        {
            // Учитываем ширину не-* колонок и вертикального скроллбара
            double nonStarWidth = dataGrid.Columns
                .Where(c => !GetIsEnabled(c))
                .Sum(c => c.ActualWidth);

            if (dataGrid.VerticalScrollBarVisibility == ScrollBarVisibility.Auto ||
                dataGrid.VerticalScrollBarVisibility == ScrollBarVisibility.Visible)
            {
                nonStarWidth += SystemParameters.VerticalScrollBarWidth;
            }

            return nonStarWidth;
        }

        private static void UpdateColumnWidths(DataGridColumn column)
        {
            if (column.DataGridOwner is DataGrid dataGrid)
                UpdateDataGridColumnsWidth(dataGrid);
        }
    }
}
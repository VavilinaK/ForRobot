using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using System.Windows.Interactivity;

namespace ForRobot.Libr.Behavior
{
    public class DataGridStretchLastColumn : Behavior<DataGrid>
    {
        private DataGrid _dataGrid;

        //public double? ViewPortWidth
        //{
        //    get
        //    {
        //        return FindChild<DataGridColumnHeadersPresenter>(this, "PART_ColumnHeadersPresenter")?.ActualWidth;
        //    }
        //}

        //public static readonly DependencyProperty StretchLastColumnProperty = DependencyProperty.RegisterAttached("StretchLastColumn", typeof(bool), typeof(AutoScrollBehavior), new PropertyMetadata(false, StretchLastColumnPropertyChanged));

        //public static bool GetStretchLastColumn(DependencyObject obj) => (bool)obj.GetValue(StretchLastColumnProperty);

        //public static void SetStretchLastColumn(DependencyObject obj, bool value) => obj.SetValue(StretchLastColumnProperty, value);

        protected override void OnAttached()
        {
            base.OnAttached();
            this._dataGrid = base.AssociatedObject;
            this._dataGrid.Loaded += OnDataGridLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this._dataGrid.Loaded -= OnDataGridLoaded;
        }

        private void OnDataGridLoaded(object sender, RoutedEventArgs e)
        {
            if (this._dataGrid.Columns.Count == 0) return;

            var lastColumn = this._dataGrid.Columns[this._dataGrid.Columns.Count - 1];
            lastColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            lastColumn.CanUserResize = false;
        }
    }
}

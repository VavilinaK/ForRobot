using System;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Interactivity;

namespace ForRobot.Libr
{
    //public static class AutoScrollBehavior
    //{
    //    public static readonly DependencyProperty AutoScrollProperty = DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(AutoScrollBehavior), new PropertyMetadata(false, AutoScrollPropertyChanged));


    //    public static void AutoScrollPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    //    {
    //        var scrollViewer = obj as ScrollViewer;
    //        if (scrollViewer != null && (bool)args.NewValue)
    //        {
    //            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
    //            scrollViewer.ScrollToEnd();
    //        }
    //        else
    //        {
    //            scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
    //        }
    //    }

    //    private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    //    {
    //        // Only scroll to bottom when the extent changed. Otherwise you can't scroll up
    //        if (e.ExtentHeightChange != 0)
    //        {
    //            var scrollViewer = sender as ScrollViewer;
    //            scrollViewer?.ScrollToBottom();
    //        }
    //    }

    //    public static bool GetAutoScroll(DependencyObject obj)
    //    {
    //        return (bool)obj.GetValue(AutoScrollProperty);
    //    }

    //    public static void SetAutoScroll(DependencyObject obj, bool value)
    //    {
    //        obj.SetValue(AutoScrollProperty, value);
    //    }
    //}

    /// <summary>
    ///  Intent: Behavior which means a scrollviewer will always scroll down to the bottom.
    /// </summary>
    public class AutoScrollBehavior : Behavior<ScrollViewer>
    {
        private double _height = 0.0d;
        private ScrollViewer _scrollViewer = null;

        protected override void OnAttached()
        {
            base.OnAttached();

            this._scrollViewer = base.AssociatedObject;
            this._scrollViewer.LayoutUpdated += new EventHandler(_scrollViewer_LayoutUpdated);
        }

        private void _scrollViewer_LayoutUpdated(object sender, EventArgs e)
        {
            if (Math.Abs(this._scrollViewer.ExtentHeight - _height) > 1)
            {
                this._scrollViewer.ScrollToVerticalOffset(this._scrollViewer.ExtentHeight);
                this._height = this._scrollViewer.ExtentHeight;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this._scrollViewer != null)
            {
                this._scrollViewer.LayoutUpdated -= new EventHandler(_scrollViewer_LayoutUpdated);
            }
        }
    }
}

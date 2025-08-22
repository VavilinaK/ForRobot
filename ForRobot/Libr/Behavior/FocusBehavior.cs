using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Interactivity;

namespace ForRobot.Libr.Behavior
{
    public static class FocusBehavior
    {
        public static readonly DependencyProperty IsFocusedProperty =  DependencyProperty.RegisterAttached("IsFocused", 
                                                                                                           typeof(bool), typeof(FocusBehavior),
                                                                                                           new UIPropertyMetadata(false, OnIsFocusedChanged));

        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        private static void OnIsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uie = d as UIElement;
            if (uie != null && (bool)e.NewValue)
            {
                // Use Dispatcher.BeginInvoke to ensure focus is set after other UI updates
                uie.Dispatcher.BeginInvoke(new Action(() => uie.Focus()), DispatcherPriority.Input);
            }
        }
    }
}

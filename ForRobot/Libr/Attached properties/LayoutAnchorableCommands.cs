using System;
using System.Windows;
using System.Windows.Input;

using AvalonDock.Layout;

namespace ForRobot.Libr.Attached_properties
{
    public static class LayoutAnchorableCommands
    {
        public static readonly DependencyProperty IsClosedProperty = DependencyProperty.RegisterAttached("IsClosed",
                                                                                                         typeof(bool),
                                                                                                         typeof(LayoutAnchorableCommands),
                                                                                                         new PropertyMetadata(false, OnIsClosedChanged));

        public static bool GetIsClosed(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsClosedProperty);
        }

        public static void SetIsClosed(DependencyObject obj, bool value)
        {
            obj.SetValue(IsClosedProperty, value);
        }

        //public static readonly DependencyProperty OpenedCommandProperty = DependencyProperty.RegisterAttached("OpenedCommand",
        //                                                                                                      typeof(ICommand),
        //                                                                                                      typeof(LayoutAnchorableCommands),
        //                                                                                                      new PropertyMetadata(null, OnOpenedCommandChanged));

        private static void OnIsClosedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LayoutAnchorable layoutAnchorable)
            {
                if (e.NewValue == null)
                    return;                

                if((bool)e.NewValue)
                    layoutAnchorable.IsSelectedChanged += LayoutAnchorable_IsSelectedChanged;
                else
                    layoutAnchorable.IsSelectedChanged -= LayoutAnchorable_IsSelectedChanged;
            }
        }

        //public static ICommand GetOpenedCommand(DependencyObject obj)
        //{
        //    return (ICommand)obj.GetValue(OpenedCommandProperty);
        //}

        public static void SetOpenedCommand(DependencyObject obj, ICommand value)
        {
            //obj.SetValue(OpenedCommandProperty, value);
        }

        private static void OnOpenedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LayoutAnchorable layoutAnchorable)
            {
                if (e.OldValue is ICommand oldCommand)
                {
                    layoutAnchorable.IsVisibleChanged -= LayoutAnchorable_IsSelectedChanged;
                }
                if (e.NewValue is ICommand newCommand)
                {
                    layoutAnchorable.IsVisibleChanged += LayoutAnchorable_IsSelectedChanged;
                }
            }
        }

        private static void LayoutAnchorable_IsSelectedChanged(object sender, EventArgs e)
        {
            if (sender is LayoutAnchorable layoutAnchorable)
            {
                //var command = GetOpenedCommand(layoutAnchorable);
                //if (command != null && command.CanExecute(layoutAnchorable.Content))
                //{
                //    command.Execute(layoutAnchorable.Content);
                //}
            }
        }
    }
}

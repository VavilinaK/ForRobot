using System;
using System.Windows;
using System.Windows.Input;

namespace ForRobot.Views.Controls
{
    /// <summary>
    /// Расширенный класс <see cref="System.Windows.Controls.Image"/>
    /// <para>С добавлением двойного нажатия</para>
    /// </summary>
    public partial class Image2D : System.Windows.Controls.Image
    {
        public static readonly RoutedEvent MouseDoubleClick = EventManager.RegisterRoutedEvent(nameof(MouseDoubleClickEvent), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Image2D));

        public event RoutedEventHandler MouseDoubleClickEvent
        {
            add => AddHandler(MouseDoubleClick, value);
            remove => RemoveHandler(MouseDoubleClick, value);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                RaiseEvent(new MouseDoubleClickEventArgs(MouseDoubleClick, this));
            }
            base.OnMouseLeftButtonDown(e);
        }

        public class MouseDoubleClickEventArgs : RoutedEventArgs
        {
            public MouseDoubleClickEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source) { }
        }
    }
}

// Не актуально на момент версии 3.0
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using System.Windows.Interactivity;

namespace ForRobot.Libr.Behavior
{
    public class ClearFocusBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            base.OnDetaching();
        }

        private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e) => Keyboard.ClearFocus();
    }
}

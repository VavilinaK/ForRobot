using System;
using System.Windows;
using System.Windows.Interactivity;

namespace ForRobot.Libr.Behavior
{
    public static class BehaviorAttacher
    {
        public static readonly DependencyProperty BehaviorsProperty =
            DependencyProperty.RegisterAttached("Behaviors", typeof(BehaviorCollection), typeof(BehaviorAttacher), new PropertyMetadata(null, OnBehaviorsChanged));

        public static BehaviorCollection GetBehaviors(DependencyObject obj)
        {
            return (BehaviorCollection)obj.GetValue(BehaviorsProperty);
        }

        public static void SetBehaviors(DependencyObject obj, BehaviorCollection value)
        {
            obj.SetValue(BehaviorsProperty, value);
        }

        private static void OnBehaviorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element && e.NewValue is BehaviorCollection newBehaviors)
            {
                // Clear existing behaviors and add new ones
                System.Windows.Interactivity.Interaction.GetBehaviors(element).Clear();
                foreach (var behavior in newBehaviors)
                {
                    System.Windows.Interactivity.Interaction.GetBehaviors(element).Add(behavior);
                }
            }
        }
    }
}

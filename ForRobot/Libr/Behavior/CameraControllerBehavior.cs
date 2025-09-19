using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

using HelixToolkit.Wpf;

namespace ForRobot.Libr.Behavior
{
    public class CameraControllerBehavior : Behavior<CameraController>
    {
        public ICommand RightViewCommand
        {
            get { return (ICommand)GetValue(RightViewCommandProperty); }
            set { SetValue(RightViewCommandProperty, value); }
        }

        public static readonly DependencyProperty RightViewCommandProperty = DependencyProperty.Register(nameof(RightViewCommand), 
                                                                                                                typeof(ICommand),
                                                                                                                typeof(CameraControllerBehavior),
                                                                                                                new PropertyMetadata(null, OnCommandChanged));

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as CameraControllerBehavior;
            if (behavior?.AssociatedObject != null && e.NewValue is ICommand command)
            {
                behavior.AssociatedObject.RightViewCommand = command;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (this.RightViewCommand != null)
            {
                AssociatedObject.RightViewCommand = this.RightViewCommand;
            }
        }
    }
}

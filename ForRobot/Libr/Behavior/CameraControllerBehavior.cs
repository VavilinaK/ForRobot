using System;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Input;
using System.Windows.Interactivity;

using HelixToolkit.Wpf;

namespace ForRobot.Libr.Behavior
{
    public class CameraControllerBehavior : Behavior<HelixViewport3D>
    {
        private HelixViewport3D _helixViewport = null;

        public static readonly RoutedCommand BackViewCommand = new RoutedCommand(nameof(BackViewCommand), typeof(CameraControllerBehavior));
        public static readonly RoutedCommand FrontViewCommand = new RoutedCommand(nameof(FrontViewCommand), typeof(CameraControllerBehavior));
        public static readonly RoutedCommand TopViewCommand = new RoutedCommand(nameof(TopViewCommand), typeof(CameraControllerBehavior));
        public static readonly RoutedCommand BottomViewCommand = new RoutedCommand(nameof(BottomViewCommand), typeof(CameraControllerBehavior));
        public static readonly RoutedCommand LeftViewCommand = new RoutedCommand(nameof(LeftViewCommand), typeof(CameraControllerBehavior));
        public static readonly RoutedCommand RightViewCommand = new RoutedCommand(nameof(RightViewCommand), typeof(CameraControllerBehavior));

        protected override void OnAttached()
        {
            base.OnAttached();

            this._helixViewport = base.AssociatedObject;
            this._helixViewport.Focusable = true;
            this._helixViewport.Focus();

            this._helixViewport.PreviewKeyDown += HandleKeyEvent;
            this._helixViewport.KeyDown += HandleKeyEvent;

            this._helixViewport.Loaded += (s, e) => (s as HelixViewport3D).Focus();

            this._helixViewport.InputBindings.Add(new KeyBinding(BackViewCommand, new KeyGesture(Key.B, ModifierKeys.Control)));
            this._helixViewport.InputBindings.Add(new KeyBinding(FrontViewCommand, new KeyGesture(Key.F, ModifierKeys.Control)));
            this._helixViewport.InputBindings.Add(new KeyBinding(TopViewCommand, new KeyGesture(Key.U, ModifierKeys.Control)));
            this._helixViewport.InputBindings.Add(new KeyBinding(BottomViewCommand, new KeyGesture(Key.D, ModifierKeys.Control)));
            this._helixViewport.InputBindings.Add(new KeyBinding(LeftViewCommand, new KeyGesture(Key.L, ModifierKeys.Control)));
            this._helixViewport.InputBindings.Add(new KeyBinding(RightViewCommand, new KeyGesture(Key.R, ModifierKeys.Control)));

            this._helixViewport.CommandBindings.Add(new CommandBinding(BackViewCommand,
                                                                       OnBackViewCommandExecuted,
                                                                       CanExecuteCommand));

            this._helixViewport.CommandBindings.Add(new CommandBinding(FrontViewCommand,
                                                                       OnFrontViewCommandExecuted,
                                                                       CanExecuteCommand));

            this._helixViewport.CommandBindings.Add(new CommandBinding(FrontViewCommand,
                                                                       OnTopViewCommandExecuted,
                                                                       CanExecuteCommand));

            this._helixViewport.CommandBindings.Add(new CommandBinding(FrontViewCommand,
                                                                       OnBottomViewCommandExecuted,
                                                                       CanExecuteCommand));

            this._helixViewport.CommandBindings.Add(new CommandBinding(LeftViewCommand,
                                                                       OnLeftViewCommandExecuted,
                                                                       CanExecuteCommand));

            this._helixViewport.CommandBindings.Add(new CommandBinding(RightViewCommand,
                                                                       OnRightViewCommandExecuted,
                                                                       CanExecuteCommand));
        }

        protected override void OnDetaching()
        {
            this._helixViewport.PreviewKeyDown -= HandleKeyEvent;
            this._helixViewport.KeyDown -= HandleKeyEvent;
            base.OnDetaching();
        }

        private void HandleKeyEvent(object sender, KeyEventArgs e)
        {
            if (this._helixViewport?.CameraController == null)
                throw new Exception("CameraController is null");

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.B:
                        this.ExecuteCommand(BackViewCommand);
                        e.Handled = true;
                        break;

                    case Key.F:
                        this.ExecuteCommand(FrontViewCommand);
                        e.Handled = true;
                        break;

                    case Key.U:
                        this.ExecuteCommand(TopViewCommand);
                        e.Handled = true;
                        break;

                    case Key.D:
                        this.ExecuteCommand(BottomViewCommand);
                        e.Handled = true;
                        break;

                    case Key.L:
                        this.ExecuteCommand(LeftViewCommand);
                        e.Handled = true;
                        break;

                    case Key.R:
                        this.ExecuteCommand(RightViewCommand);
                        e.Handled = true;
                        break;
                }
            }
        }

        private void ExecuteCommand(RoutedCommand command)
        {
            if (command.CanExecute(null, this._helixViewport)) command.Execute(null, this._helixViewport);
        }

        private void OnBackViewCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this._helixViewport?.CameraController == null)
                throw new Exception("CameraController is null");

            this._helixViewport?.CameraController.ChangeDirection(new Vector3D(0, -1, 0), new Vector3D(0, 0, 1));
            e.Handled = true;
        }

        private void OnFrontViewCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this._helixViewport?.CameraController == null)
                throw new Exception("CameraController is null");

            this._helixViewport?.CameraController.ChangeDirection(new Vector3D(0, 1, 0), new Vector3D(0, 0, 1));
            e.Handled = true;
        }

        private void OnTopViewCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this._helixViewport?.CameraController == null)
                throw new Exception("CameraController is null");

            this._helixViewport?.CameraController.ChangeDirection(new Vector3D(0, 0, -1), new Vector3D(0, 1, 0));
            e.Handled = true;
        }

        private void OnBottomViewCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this._helixViewport?.CameraController == null)
                throw new Exception("CameraController is null");

            this._helixViewport?.CameraController.ChangeDirection(new Vector3D(0, 0, 1), new Vector3D(0, -1, 0));
            e.Handled = true;
        }

        private void OnLeftViewCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this._helixViewport?.CameraController == null)
                throw new Exception("CameraController is null");

            this._helixViewport?.CameraController.ChangeDirection(new Vector3D(1, 0, 0), new Vector3D(0, 0, 1));
            e.Handled = true;
        }

        private void OnRightViewCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this._helixViewport?.CameraController == null)
                throw new Exception("CameraController is null");

            this._helixViewport?.CameraController.ChangeDirection(new Vector3D(-1, 0, 0), new Vector3D(0, 0, 1));
            e.Handled = true;
        }

        private void CanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this._helixViewport?.Camera != null && this._helixViewport.CameraController != null;
            e.Handled = true;
        }
    }
}

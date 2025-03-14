using System;
using System.Windows.Interactivity;
using GalaSoft.MvvmLight.Messaging;
using HelixToolkit.Wpf;

namespace ForRobot.Libr.Behavior
{
    public class ZoomBehavior : Behavior<HelixViewport3D>
    {
        private HelixViewport3D _helixViewport = null;

        protected override void OnAttached()
        {
            base.OnAttached();
            Messenger.Default.Register<ZoomMessage>(this, message => Zoom(message.Step));
            this._helixViewport = base.AssociatedObject;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            Messenger.Default.Unregister<ZoomMessage>(this);
        }

        public void Zoom() => this.Zoom(null);

        public void Zoom(double? i)
        {
            if (this._helixViewport == null)
                return;

            if (i == null)
                this._helixViewport.ZoomExtents();
            else
                this._helixViewport.CameraController.Zoom((double)i);
        }
    }
}

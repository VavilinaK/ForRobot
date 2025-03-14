using System;
using System.Windows.Interactivity;

using GalaSoft.MvvmLight.Messaging;

using HelixToolkit.Wpf;

namespace ForRobot.Libr.Behavior
{
    public class ZoomMessage
    {
        public double Step { get; set; }

        public ZoomMessage() { }

        public ZoomMessage(double s)
        {
            this.Step = s;
        }
    }
}

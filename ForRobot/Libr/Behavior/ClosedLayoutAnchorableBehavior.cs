using System;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;

using System.Windows.Interactivity;

using AvalonDock.Layout;

namespace ForRobot.Libr.Behavior
{
    /// <summary>
    /// Поведение закрытия <see cref="AvalonDock.Layout.LayoutAnchorable"/> пин-кодом
    /// </summary>
    public class ClosedLayoutAnchorableBehavior : Behavior<LayoutAnchorable>
    {
        private LayoutAnchorable _layoutAnchorable = null;
        private bool _isOpening = false;
        
        protected override void OnAttached()
        {
            base.OnAttached();
            this._layoutAnchorable = base.AssociatedObject;

            if (this._layoutAnchorable != null)
                this._layoutAnchorable.Hiding += HandleHidingEvent;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this._layoutAnchorable != null)
                this._layoutAnchorable.Hiding -= HandleHidingEvent;
        }

        private static void HandleHidingEvent(object sender, CancelEventArgs e)
        {

        }

        //private static void LayoutAnchorable_IsSelectedChanged(object sender, EventArgs e)
        //{
        //    if (sender is LayoutAnchorable layoutAnchorable)
        //    {

        //    }
        //}
    }
}

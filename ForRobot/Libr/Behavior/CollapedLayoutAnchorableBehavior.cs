using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Interactivity;

using GalaSoft.MvvmLight.Messaging;
using AvalonDock;
using AvalonDock.Layout;

namespace ForRobot.Libr.Behavior
{
    public class CollapedLayoutAnchorableBehavior : Behavior<DockingManager>
    {
        private DockingManager _dockingManager = null;

        protected override void OnAttached()
        {
            base.OnAttached();
            Messenger.Default.Register<CollapedLayoutAnchorableMessage>(this, message => this.CollapedPanel(message.ContentId));
            this._dockingManager = base.AssociatedObject;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            Messenger.Default.Unregister<CollapedLayoutAnchorableMessage>(this);
        }

        public void CollapedPanel(string contentId)
        {
            var panel = _dockingManager.Layout.Descendents().OfType<LayoutAnchorable>().FirstOrDefault(p => p.ContentId == contentId);
            panel.IsVisible = true;
        }
    }
}

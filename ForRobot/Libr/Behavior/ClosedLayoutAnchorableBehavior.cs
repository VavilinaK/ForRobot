using System;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
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
        private bool _isCheckingPin = false;
        private bool _isUnlocked = false;

        protected override void OnAttached()
        {
            base.OnAttached();
            this._layoutAnchorable = base.AssociatedObject;

            if (this._layoutAnchorable != null)
            {
                this._layoutAnchorable.PropertyChanged += HandlerPropertyChangedEvent;
                this._layoutAnchorable.Hiding += HandleHidingEvent;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this._layoutAnchorable != null)
            {
                this._layoutAnchorable.PropertyChanged -= HandlerPropertyChangedEvent;
                this._layoutAnchorable.Hiding -= HandleHidingEvent;
            }
        }
    
        private void HandlerPropertyChangedEvent(object sender, PropertyChangedEventArgs e)
        {
            string propertyName = e.PropertyName;
            if ((propertyName == nameof(LayoutAnchorable.IsSelected) && _layoutAnchorable.IsSelected) ||
               (propertyName == nameof(LayoutAnchorable.IsVisible) && _layoutAnchorable.IsVisible))
            {
                if (!_isUnlocked && !_isCheckingPin)
                {
                    //Task.Run(async () => await CheckPinCodeAsync());
                }
            }
        }

        private void HandleHidingEvent(object sender, CancelEventArgs e) => _isUnlocked = false;

        private async Task CheckPinCodeAsync()
        {
            this._isCheckingPin = true;
            try
            {
                bool pinResult = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    pinResult = ForRobot.App.EqualsPinCode();
                });
                if (pinResult)
                {
                    _isUnlocked = true;
                    return;
                }
            }
            finally
            {
                _layoutAnchorable.Hide();
                _isCheckingPin = false;
                //MessageBox.Show("Неверный пин-код")
            }
        }

        //private static void LayoutAnchorable_IsSelectedChanged(object sender, EventArgs e)
        //{
        //    if (sender is LayoutAnchorable layoutAnchorable)
        //    {

        //    }
        //}
    }
}

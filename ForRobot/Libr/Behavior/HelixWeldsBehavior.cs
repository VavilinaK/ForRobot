using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Interactivity;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using HelixToolkit.Wpf;

using ForRobot.Model.Detals;

namespace ForRobot.Libr.Behavior
{
    public class HelixWeldsBehavior : Behavior<HelixViewport3D>
    {
        private HelixViewport3D _helixViewport = null;
        private List<LinesVisual3D> _weldsVisuals = new List<LinesVisual3D>();

        public static readonly DependencyProperty WeldsProperty = DependencyProperty.Register("Welds",
                                                                                              typeof(ObservableCollection<Weld>),
                                                                                              typeof(HelixWeldsBehavior),
                                                                                              new PropertyMetadata(null, OnWeldsChanged));

        public ObservableCollection<Weld> Welds
        {
            get => (ObservableCollection<Weld>)GetValue(WeldsProperty);
            set => SetValue(WeldsProperty, value);
        }

        private static void OnWeldsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (HelixWeldsBehavior)d;
            if (e.OldValue is ObservableCollection<Weld> oldCollection)
                oldCollection.CollectionChanged -= behavior.HandleWeldsChanged;
            if (e.NewValue is ObservableCollection<Weld> newCollection)
                newCollection.CollectionChanged += behavior.HandleWeldsChanged;
        }

        private void HandleWeldsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this._helixViewport == null) return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.OfType<Weld>())
                        this.AddWeldVisual(item);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.OfType<Weld>())
                        this.RemoveWeldVisual(item);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (var weldVisual in _weldsVisuals)
                        _helixViewport.Children.Remove(weldVisual);
                    _weldsVisuals.Clear();
                    break;
            }
        }

        private void AddWeldVisual(Weld weld)
        {
            var line = new LinesVisual3D
            {
                Points = new Point3DCollection { weld.StartPoint, weld.EndPoint },
                Color = weld.Color,
                Thickness = weld.Thickness
            };
            this._weldsVisuals.Add(line);
            this._helixViewport.Children.Add(line);
        }

        private void RemoveWeldVisual(Weld weld)
        {
            var line = this._helixViewport.Children.OfType<LinesVisual3D>().FirstOrDefault(l => l.Points[0] == weld.StartPoint && l.Points[1] == weld.EndPoint);
            if (line != null)
            {
                this._helixViewport.Children.Remove(line);
                this._weldsVisuals.Remove(line);
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this._helixViewport = base.AssociatedObject;

            if (this.Welds != null)
                foreach (var weld in Welds)
                    this.AddWeldVisual(weld);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.Welds != null)
                foreach (var weld in Welds)
                    this.RemoveWeldVisual(weld);
        }
    }
}

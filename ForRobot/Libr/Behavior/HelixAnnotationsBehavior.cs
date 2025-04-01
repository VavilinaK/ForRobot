using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Interactivity;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using HelixToolkit.Wpf;

using ForRobot.Model.File3D;

namespace ForRobot.Libr.Behavior
{
    public class HelixAnnotationsBehavior : Behavior<HelixViewport3D>
    {
        private HelixViewport3D _helixViewport = null;

        public static readonly DependencyProperty AnnotationsProperty = DependencyProperty.Register(nameof(Annotations), 
                                                                                                    typeof(ObservableCollection<Annotation>),
                                                                                                    typeof(HelixAnnotationsBehavior),
                                                                                                    new PropertyMetadata(null, OnAnnotationsChanged));

        public ObservableCollection<Annotation> Annotations
        {
            get => (ObservableCollection<Annotation>)GetValue(AnnotationsProperty);
            set => SetValue(AnnotationsProperty, value);
        }

        private static void OnAnnotationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (HelixAnnotationsBehavior)d;
            if (e.OldValue is ObservableCollection<Annotation> oldCollection)
                oldCollection.CollectionChanged -= behavior.HandleCollectionChanged;
            if (e.NewValue is ObservableCollection<Annotation> newCollection)
                newCollection.CollectionChanged += behavior.HandleCollectionChanged;
        }

        private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this._helixViewport == null) return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.OfType<Annotation>())
                    {
                        this._helixViewport.Children.Add(item);
                        if (item is INotifyPropertyChanged notifyItem)
                            notifyItem.PropertyChanged += OnItemPropertyChanged;
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.OfType<Annotation>())
                    {
                        this._helixViewport.Children.Remove(item);
                        if (item is INotifyPropertyChanged notifyItem)
                            notifyItem.PropertyChanged -= OnItemPropertyChanged;
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    //this._helixViewport.Children.Clear();
                    break;
            }
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var item = sender as Annotation;
            if (item != null && AssociatedObject.Children.Contains(item))
            {
                int index = AssociatedObject.Children.IndexOf(item);
                AssociatedObject.Children.RemoveAt(index);
                AssociatedObject.Children.Insert(index, item);
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this._helixViewport = base.AssociatedObject;

            if (Annotations != null)
                foreach (var item in Annotations)
                    this._helixViewport.Children.Add(item);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (Annotations != null)
                foreach (var item in Annotations)
                    this._helixViewport.Children.Remove(item);
        }
    }
}

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

namespace ForRobot.Libr.Behavior
{
    /// <summary>
    /// Класс-поведение <see cref="HelixViewport3D"/> для вывода коллекций типа <see cref="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HelixAddCollectionBehavior<T> : Behavior<HelixViewport3D> where T : Visual3D
    {
        private HelixViewport3D _helixViewport = null;

        public virtual IEnumerable<T> Items
        {
            get => (IEnumerable<T>)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(nameof(Items),
                                                                                              typeof(IEnumerable<T>),
                                                                                              typeof(HelixAddCollectionBehavior<T>),
                                                                                              new PropertyMetadata(null, OnItemsChanged));

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (HelixAddCollectionBehavior<T>)d;
            if (e.OldValue is ObservableCollection<T> oldCollection)
                oldCollection.CollectionChanged -= behavior.HandleCollectionChanged;
            if (e.NewValue is ObservableCollection<T> newCollection)
                newCollection.CollectionChanged += behavior.HandleCollectionChanged;
        }

        private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this._helixViewport == null) return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.OfType<T>())
                    {
                        this._helixViewport.Children.Add(item);
                        if (item is INotifyPropertyChanged notifyItem)
                            notifyItem.PropertyChanged += OnItemPropertyChanged;
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    List<T> itemsToRemove = _helixViewport.Children.OfType<T>().Where(item => Items.Contains(item)).ToList();

                    foreach (var item in itemsToRemove)
                    {
                        _helixViewport.Children.Remove(item);
                        if (item is INotifyPropertyChanged notifyItem)
                            notifyItem.PropertyChanged -= OnItemPropertyChanged;
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    //this._helixViewport.Children.Clear();
                    List<T> itemsToReset = _helixViewport.Children.OfType<T>().ToList();

                    foreach (var item in itemsToReset)
                    {
                        _helixViewport.Children.Remove(item);
                        if (item is INotifyPropertyChanged notifyItem)
                            notifyItem.PropertyChanged -= OnItemPropertyChanged;
                    }
                    break;
            }
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var item = sender as T;
            if (item != null && this._helixViewport.Children.Contains(item))
            {
                int index = this._helixViewport.Children.IndexOf(item);
                this._helixViewport.Children.RemoveAt(index);
                this._helixViewport.Children.Insert(index, item);
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this._helixViewport = base.AssociatedObject;

            if (Items == null)
                return;

            foreach (var item in Items)
                this._helixViewport.Children.Add(item);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (Items == null)
                return;

            foreach (var item in Items)
                this._helixViewport.Children.Remove(item);
        }
    }
}

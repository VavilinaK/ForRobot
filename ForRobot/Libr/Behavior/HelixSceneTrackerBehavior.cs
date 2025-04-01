using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Interactivity;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using GalaSoft.MvvmLight.Messaging;
using HelixToolkit.Wpf;

using ForRobot.Model.File3D;

namespace ForRobot.Libr.Behavior
{
    public class HelixSceneTrackerMessage
    {
        //public ObservableCollection<Visual3D> HelixViewport3DChildren { get; set; }

        public HelixSceneTrackerMessage() { }

        //public HelixSceneTrackerMessage(ObservableCollection<Visual3D> helixViewport3DChildren)
        //{
        //    this.HelixViewport3DChildren = helixViewport3DChildren;
        //}
    }

    public class HelixSceneTrackerBehavior : Behavior<HelixViewport3D>
    {
        private HelixViewport3D _helixViewport = null;

        public static readonly DependencyProperty SceneItemsProperty =  DependencyProperty.Register(nameof(SceneItems), 
                                                                                                    typeof(SceneItem), 
                                                                                                    typeof(HelixSceneTrackerBehavior),
                                                                                                    new PropertyMetadata(null));

        //public ObservableCollection<SceneItem> SceneItems
        //{
        //    get => (ObservableCollection<SceneItem>)GetValue(SceneItemsProperty);
        //    set => SetValue(SceneItemsProperty, value);
        //}

        public SceneItem SceneItems
        {
            get => (SceneItem)GetValue(SceneItemsProperty);
            set => SetValue(SceneItemsProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this._helixViewport = base.AssociatedObject;
            //Messenger.Default.Register<HelixSceneTrackerMessage>(this, message =>
            //{
            //    this.SceneItems = new SceneItem();
            //    ForRobot.Model.File3D.SceneItem.AddChildren(this.SceneItems, this._helixViewport);
            //    //foreach (var visual3D in this._helixViewport.Children)
            //    //{
            //    //    this.SceneItems.Add(visual3D);
            //    //}
            //});

            //this._helixViewport.DataContextChanged += ChildrenChanged;
            //((INotifyCollectionChanged)base.AssociatedObject.Children).CollectionChanged += ChildrenChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            //this._helixViewport.DataContextChanged -= ChildrenChanged;

            //((INotifyCollectionChanged)this._helixViewport.Children).CollectionChanged -= ChildrenChanged;
        }

        //private void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        //private void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    SceneItems = new ObservableCollection<Model3D>();

        //    if (e.NewValue != null)
        //        foreach (Model3D item in (ObservableCollection<Model3D>)e.NewValue)
        //            SceneItems.Add(item);

        //    if (e.OldValue != null)
        //        foreach (Model3D item in (ObservableCollection<Model3D>)e.OldValue)
        //            SceneItems.Remove(item);
        //}
    }
}

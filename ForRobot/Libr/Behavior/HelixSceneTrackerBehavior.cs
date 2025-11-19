using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Interactivity;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using GalaSoft.MvvmLight.Messaging;
using HelixToolkit.Wpf;

using ForRobot.Models.File3D;

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

    public class HelixSceneTrackerBehavior : Behavior<HelixViewport3D>, INotifyPropertyChanged
    {
        private HelixViewport3D _helixViewport = null;
        //private SceneItem _rootGroup;
        private Type[] _blockType = new Type[] 
        {
            typeof(HelixToolkit.Wpf.BoundingBoxVisual3D),
            typeof(Annotation),
            typeof(HelixToolkit.Wpf.GridLinesVisual3D)
        };

        public static readonly DependencyProperty SceneItemsProperty =  DependencyProperty.Register(nameof(SceneItems), 
                                                                                                    typeof(ObservableCollection<SceneItem>), 
                                                                                                    typeof(HelixSceneTrackerBehavior),
                                                                                                    new FrameworkPropertyMetadata(new ObservableCollection<SceneItem>(),
                                                                                                                                  FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                                                                                  new PropertyChangedCallback(OnSceneItemsChanged)
                                                                                                                                  //(o, args) => o.SetValue(SceneItemsProperty, args.NewValue)
                                                                                                                                  
                                                                                                                                  ));
        
        public ObservableCollection<SceneItem> SceneItems
        {
            get => (ObservableCollection<SceneItem>)GetValue(SceneItemsProperty);
            set => SetValue(SceneItemsProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this._helixViewport = base.AssociatedObject;
            this._helixViewport.Loaded += (s, e) => { this.SceneItemsBilding(this._helixViewport.Children); };
            this.SceneItems.CollectionChanged += SceneItemsChanged;

            //_rootGroup = new SceneItem()
            //{
            //    Children = this.SceneItems
            //};

            Messenger.Default.Register<HelixSceneTrackerMessage>(this, message =>
            {
                this.SceneItemsBilding(this._helixViewport.Children);
            });
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.SceneItems.CollectionChanged -= SceneItemsChanged;
        }

        private void SceneItemsBilding(Visual3DCollection visual3Ds)
        {
            this.SceneItems.Clear();

            if (visual3Ds == null)
                return;

            foreach (var item in visual3Ds.Where(item => !this._blockType.Contains(item.GetType())))
            {
                var obj = new SceneItem(item);
                obj.VisibleEvent += SceneItemVisible;
                obj.HiddenEvent += SceneItemHidden;
                this.SceneItems.Add(obj);
            }
        }

        private void SceneItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //switch (e.Action)
            //{
            //    case NotifyCollectionChangedAction.Add:
            //        foreach (SceneItem item in e.NewItems)
            //            _rootGroup.Children.Add(item);
            //        break;

            //    case NotifyCollectionChangedAction.Remove:
            //        foreach (SceneItem item in e.OldItems)
            //            _rootGroup.Children.Remove(item);
            //        break;

            //    case NotifyCollectionChangedAction.Reset:
            //        _rootGroup.Children.Clear();
            //        break;
            //}

            this.OnPropertyChanged(nameof(this.SceneItems));
        }

        private void SceneItemVisible(object sender, EventArgs e)
        {
            SceneItem item = sender as SceneItem;
            this._helixViewport.Children.Add((Visual3D)item.SceneObject);
        }

        private void SceneItemHidden(object sender, EventArgs e)
        {
            SceneItem item = sender as SceneItem;
            this._helixViewport.Children.Remove((Visual3D)item.SceneObject);
        }

        private static void OnSceneItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HelixSceneTrackerBehavior helixSceneTracker = (HelixSceneTrackerBehavior)d;
            helixSceneTracker.SceneItems = (ObservableCollection<SceneItem>)e.NewValue;
            helixSceneTracker.OnPropertyChanged(nameof(helixSceneTracker.SceneItems));
        }

        #region Implementations of IDisposable

        private void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Collections.Specialized;

namespace ForRobot.Libr
{
    public class TreeViewSelectedItemBehavior : Behavior<TreeView>
    {
        private bool _modelHandled;

        private readonly EventSetter _treeViewItemEventSetter;

        public delegate bool IsChildOfPredicate(object nodeA, object nodeB);

        #region SelectedItem Property

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public bool ExpandSelected
        {
            get => (bool)GetValue(ExpandSelectedProperty);
            set => SetValue(ExpandSelectedProperty, value);
        }

        public IsChildOfPredicate HierarchyPredicate
        {
            get => (IsChildOfPredicate)GetValue(HierarchyPredicateProperty);
            set => SetValue(HierarchyPredicateProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), 
                                                                                                     typeof(object),
                                                                                                     typeof(TreeViewSelectedItemBehavior),
                                                                                                     new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public static readonly DependencyProperty ExpandSelectedProperty = DependencyProperty.Register(nameof(ExpandSelected),
                                                                                                       typeof(bool),
                                                                                                       typeof(TreeViewSelectedItemBehavior),
                                                                                                       new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HierarchyPredicateProperty = DependencyProperty.Register(nameof(HierarchyPredicate),
                                                                                                           typeof(IsChildOfPredicate),
                                                                                                           typeof(TreeViewSelectedItemBehavior),
                                                                                                           new FrameworkPropertyMetadata(null));

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //var item = e.NewValue as TreeViewItem;
            //if (item != null)
            //{
            //    item.SetValue(TreeViewItem.IsSelectedProperty, true);
            //}

            var behavior = (TreeViewSelectedItemBehavior)sender;
            if (behavior._modelHandled) return;

            if (behavior.AssociatedObject == null)
                return;

            behavior._modelHandled = true;
            behavior.UpdateAllTreeViewItems();
            behavior._modelHandled = false;
        }

        //private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        //{
        //    var behavior = (TreeViewSelectedItemBehavior)sender;
        //    if (behavior._modelHandled) return;

        //    if (behavior.AssociatedObject == null)
        //        return;

        //    behavior._modelHandled = true;
        //    behavior.UpdateAllTreeViewItems();
        //    behavior._modelHandled = false;
        //}

        #endregion

        public TreeViewSelectedItemBehavior()
        {
            this._treeViewItemEventSetter = new EventSetter(FrameworkElement.LoadedEvent, new RoutedEventHandler(OnTreeViewItemLoaded));
        }

        private void UpdateTreeViewItem(TreeViewItem item, bool recurse)
        {
            if (SelectedItem == null)
                return;

            var model = item.DataContext;

            if (SelectedItem == model && !item.IsSelected)
            {
                item.IsSelected = true;
                if (ExpandSelected)
                    item.IsExpanded = true;
            }
            else
            {
                bool isParentOfModel = HierarchyPredicate?.Invoke(SelectedItem, model) ?? true;
                if (isParentOfModel)
                    item.IsExpanded = true;
            }

            if (recurse)
            {
                foreach (var subitem in item.Items)
                {
                    TreeViewItem tvi = item.ItemContainerGenerator.ContainerFromItem(subitem) as TreeViewItem;
                    if (tvi != null)
                        UpdateTreeViewItem(tvi, true);
                }
            }
        }
        
        private void UpdateAllTreeViewItems()
        {
            var treeView = AssociatedObject;
            foreach (var item in treeView.Items)
            {
                var tvi = treeView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (tvi != null)
                    UpdateTreeViewItem(tvi, true);
            }
        }

        private void UpdateTreeViewItemStyle()
        {
            //if (AssociatedObject.ItemContainerStyle == null)
            //{
            //    var style = new Style(typeof(TreeViewItem),
            //        Application.Current.TryFindResource(typeof(TreeViewItem)) as Style);

            //    AssociatedObject.ItemContainerStyle = style;
            //}

            //if (!AssociatedObject.ItemContainerStyle.Setters.Contains(_treeViewItemEventSetter))
            //    AssociatedObject.ItemContainerStyle.Setters.Add(_treeViewItemEventSetter);
        }

        private void OnTreeViewItemsChanged(object sender, NotifyCollectionChangedEventArgs args) => UpdateAllTreeViewItems();
          
        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this._modelHandled) return;

            if (AssociatedObject.Items.SourceCollection == null) return;

            this.SelectedItem = e.NewValue;
        }

        private void OnTreeViewItemLoaded(object sender, RoutedEventArgs args) => UpdateTreeViewItem((TreeViewItem)sender, false);

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
            ((INotifyCollectionChanged)AssociatedObject.Items).CollectionChanged += OnTreeViewItemsChanged;

            UpdateTreeViewItemStyle();
            _modelHandled = true;
            UpdateAllTreeViewItems();
            _modelHandled = false;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.AssociatedObject != null)
            {
                AssociatedObject.ItemContainerStyle?.Setters?.Remove(_treeViewItemEventSetter);
                AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
                ((INotifyCollectionChanged)AssociatedObject.Items).CollectionChanged -= OnTreeViewItemsChanged;
            }
        }
    }
}

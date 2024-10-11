using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ForRobot.Libr
{
    public class TreeViewSelectedItemBehavior : Behavior<TreeView>
    {
        #region SelectedItem Property

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), 
                                                                                                     typeof(object),
                                                                                                     typeof(TreeViewSelectedItemBehavior),
                                                                                                     new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var item = e.NewValue as TreeViewItem;
            if (item != null)
            {
                item.SetValue(TreeViewItem.IsSelectedProperty, true);
            }
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

        //public IsChildOfPredicate HierarchyPredicate
        //{
        //    get => (IsChildOfPredicate)GetValue(HierarchyPredicateProperty);
        //    set => SetValue(HierarchyPredicateProperty, value);
        //}

        //// Should expand selected?
        //public bool ExpandSelected
        //{
        //    get => (bool)GetValue(ExpandSelectedProperty);
        //    set => SetValue(ExpandSelectedProperty, value);
        //}






        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            }
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) => this.SelectedItem = e.NewValue;
    }
}

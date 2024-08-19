using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

using ForRobot.Libr.Controls;

namespace ForRobot.Model.Controls
{
    public abstract class NavTreeItem : BaseClass, INavTreeItem
    {
        // for display in tree
        public string FullName { get; set; }

        public string FullPathName { get; set; }
        
        public BitmapSource Icon
        {
            get;set;
            //get { return myIcon ?? (myIcon = GetMyIcon()); }
            //set { myIcon = value; }
        }

        protected ObservableCollection<INavTreeItem> _children;
        public ObservableCollection<INavTreeItem> Children
        {
            get { return _children ?? (_children = GetMyChildren()); }
            set { Set(ref _children, value, "Children"); }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { Set(ref _isExpanded, value, "IsExpanded"); }
        }

        public bool IncludeFileChildren { get; set; }

        public abstract BitmapSource GetMyIcon();
        public abstract ObservableCollection<INavTreeItem> GetMyChildren();

        public void DeleteChildren()
        {
            if (this.Children != null)
            {
                for (int i = this.Children.Count - 1; i >= 0; i--)
                {
                    this.Children[i].DeleteChildren();
                    this.Children[i] = null;
                    this.Children.RemoveAt(i);
                }
                this.Children = null;
            }
        }
    }
}

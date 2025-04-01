using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ForRobot.Model.Controls;

namespace ForRobot.ViewModels.Controls
{
    public class NavigationTreeViewVM : BaseClass
    {
        private int rootNr;
        public int RootNr
        {
            get { return rootNr; }
            set { Set(ref rootNr, value, true, "RootNr"); }
        }

        private ObservableCollection<IFile> rootChildren = new ObservableCollection<IFile> { };
        public ObservableCollection<IFile> RootChildren
        {
            get { return rootChildren; }
            set { Set(ref rootChildren, value, true, "RootChildren"); }
        }

        public NavigationTreeViewVM() : this(0) { }

        public NavigationTreeViewVM(int rootNumber) : this(rootNumber, false) { }

        public NavigationTreeViewVM(int pRootNumber = 0, bool pIncludeFileChildren = false)
        {
            //// create a new RootItem given rootNumber using convention
            //RootNr = pRootNumber;
            //NavTreeItem treeRootItem = NavTreeRootItemUtils.ReturnRootItem(pRootNumber, pIncludeFileChildren);
            //TreeName = treeRootItem.FriendlyName;

            //// Delete RootChildren and init RootChildren ussing treeRootItem.Children
            //foreach (INavTreeItem item in RootChildren) { item.DeleteChildren(); }
            //RootChildren.Clear();

            //foreach (INavTreeItem item in treeRootItem.Children) { RootChildren.Add(item); }
        }

        //public void RebuildTree(int pRootNr = -1, bool pIncludeFileChildren = false)
        //{
        //    // First take snapshot of current expanded items
        //    List<String> SnapShot = NavTreeUtils.TakeSnapshot(rootChildren);

        //    // As a matter of fact we delete and construct the tree//RoorChildren again.....
        //    // Delete all rootChildren
        //    foreach (INavTreeItem item in rootChildren) item.DeleteChildren();
        //    rootChildren.Clear();

        //    // Create treeRootItem 
        //    if (pRootNr != -1) RootNr = pRootNr;
        //    NavTreeItem treeRootItem = NavTreeRootItemUtils.ReturnRootItem(RootNr, pIncludeFileChildren);
        //    if (pRootNr != -1) TreeName = treeRootItem.FriendlyName;

        //    // Copy children treeRootItem to RootChildren, set up new tree 
        //    foreach (INavTreeItem item in treeRootItem.Children) { RootChildren.Add(item); }

        //    //Expand previous snapshot
        //    NavTreeUtils.ExpandSnapShotItems(SnapShot, treeRootItem);
        //}
    }
}

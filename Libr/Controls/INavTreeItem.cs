using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

namespace ForRobot.Libr.Controls
{
    public interface INavTreeItem : INotifyPropertyChanged
    {
        string FullName { get; set; }

        // Drive/Folder/File naming scheme to retrieve children
        string FullPathName { get; set; }

        // Image used in TreeItem
        BitmapSource Icon { get; set; }

        ObservableCollection<INavTreeItem> Children { get; }

        bool IsExpanded { get; set; }

        // Specific for this application, could be introduced later in more specific interface/classes
        bool IncludeFileChildren { get; set; }

        // For resetting the tree
        void DeleteChildren();
    }
}

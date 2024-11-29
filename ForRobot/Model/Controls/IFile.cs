using System;
using System.ComponentModel;
//using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

namespace ForRobot.Model.Controls
{
    public interface IFile : INotifyPropertyChanged
    {
        string Name { get; set; }

        string Path { get; set; }

        bool IsCheck { get; set; }

        bool IsCopy { get; set; }

        bool IsExpanded { get; set; }

        bool IncludeFileChildren { get; }

        FileTypes Type { get; }

        FileFlags Flag { get; set; }

        ObservableCollection<IFile> Children { get; }
    }
}

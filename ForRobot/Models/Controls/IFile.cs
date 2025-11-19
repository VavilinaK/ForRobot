using System;
using System.ComponentModel;
//using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

namespace ForRobot.Models.Controls
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

        IFile this[int index] { get; set; }

        ForRobot.Libr.Collections.FullyObservableCollection<IFile> Children { get; }

        IFile Search(string nameToSearchFor);
    }
}

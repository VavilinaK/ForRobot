using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace ForRobot.Model.Controls.NavigationTree
{
    public abstract class File : BaseClass
        //, IFile
    {
        private bool _isExpanded = true;

        private ObservableCollection<IFile> _children;

        public bool IsExpanded { get => this._isExpanded; set => Set(ref this._isExpanded, value); }

        public bool IncludeFileChildren { get => (this.Children.Count > 0) ? true : false; }

        public string Name { get; set; }

        public string FullPathFile { get; set;  }

        public BitmapSource Icon
        {
            get
            {
                switch (this.Type)
                {
                    case FileTypes.Folder:
                        return (BitmapImage)Application.Current.TryFindResource("ImageFolderIcon");

                    case FileTypes.DataList:
                        return (BitmapImage)Application.Current.TryFindResource("ImageDataFileIcon");

                    case FileTypes.Program:
                        return (BitmapImage)Application.Current.TryFindResource("ImageProgramFileIcon");

                    default:
                        return null;
                }
            }
        }

        public FileTypes Type
        {
            get
            {
                switch (this.Name)
                {
                    case string a when a.Contains(".dat"):
                        return FileTypes.DataList;

                    case string b when b.Contains(".src"):
                        return FileTypes.Program;

                    default:
                        return FileTypes.Folder;
                }
            }
        }

        public ObservableCollection<IFile> Children
        {
            get => this._children;
            set => Set(ref this._children, value);
        }

        //public void DeleteChildren()
        //{
        //    throw new NotImplementedException();
        //}
    }
}

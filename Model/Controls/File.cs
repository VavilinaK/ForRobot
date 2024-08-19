using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ForRobot.Model.Controls
{
    public class File : BaseClass, IFile
    {
        private bool _isExpanded = true;

        protected ObservableCollection<IFile> _children = new ObservableCollection<IFile>();
        
        public string Name { get; set; }
        public string Path { get; set; }

        public BitmapImage Icon
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

        public bool IsExpanded
        {
            get => this._isExpanded;
            set => Set(ref this._isExpanded, value);
        }
        public bool IncludeFileChildren { get; set; } = true;

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

        public FileFlags Flag { get; set; }

        public ObservableCollection<IFile> Children
        {
            get => this._children;
            set => Set(ref this._children, value);
        }

        public File() { }

        public File(string path, string inf)
        {
            this.Path = path;
            this.Name = this.Path.Split(new char[] { '\\' }).Last();

            switch (inf.Split(new char[] { ';' }).First())
            {
                case "RVO":
                    this.Flag = FileFlags.RVO;
                    break;

                case "RVP":
                    this.Flag = FileFlags.RVP;
                    break;

                case "RVEO":
                    this.Flag = FileFlags.RVEO;
                    break;

                case "RO2":
                    this.Flag = FileFlags.RO2;
                    break;

                case "RV":
                    this.Flag = FileFlags.RV;
                    break;

                case "RV2":
                    this.Flag = FileFlags.RV2;
                    break;

                default:
                    this.Flag = FileFlags.None;
                    break;
            }
        }
    }
}

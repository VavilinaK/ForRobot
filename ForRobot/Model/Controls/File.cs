using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Media.Imaging;

namespace ForRobot.Model.Controls
{
    public class File : BaseClass, IFile
    {
        private bool _isExpanded = true;

        protected ObservableCollection<IFile> _children = new ObservableCollection<IFile>();
        
        public string Name { get; set; }
        public string Path { get; set; }

        public bool IsExpanded
        {
            get => this._isExpanded;
            set => Set(ref this._isExpanded, value);
        }

        public bool IncludeFileChildren { get => this.Children.Count() != 0; }

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

        public FileFlags Flag { get; set; } = FileFlags.None;

        public ObservableCollection<IFile> Children
        {
            get => this._children;
            set => Set(ref this._children, value);
        }

        public File() { }

        public File(string path)
        {
            this.Path = path;
            this.Name = this.Path.Split(new char[] { '\\' }).Last() != string.Empty ? this.Path.Split(new char[] { '\\' }).Last()
                : this.Path.Split(new char[] { '\\' })[this.Path.Split(new char[] { '\\' }).Count() - 2];
        }

        public File(string path, string inf)
        {
            this.Path = path;
            this.Name = this.Path.Split(new char[] { '\\' }).Last() != string.Empty ? this.Path.Split(new char[] { '\\' }).Last() 
                : this.Path.Split(new char[] { '\\' })[this.Path.Split(new char[] { '\\' }).Count() - 2];

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

        public static File Search(File root, string nameToSearchFor)
        {
            Queue<File> Q = new Queue<File>();
            HashSet<File> S = new HashSet<File>();
            Q.Enqueue(root);
            S.Add(root);

            while (Q.Count > 0)
            {
                File e = Q.Dequeue();
                if (e.Name == nameToSearchFor)
                    return e;
                foreach (File friend in e.Children)
                {
                    if (!S.Contains(friend))
                    {
                        Q.Enqueue(friend);
                        S.Add(friend);
                    }
                }
            }
            return null;
        }
    }
}

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ForRobot.Model.Controls
{
    public class File : BaseClass, IFile, ICloneable
    {
        #region Private variables

        private bool _isCheck = false;
        private bool _isCopy = false;
        private bool _isExpanded = true;

        private ObservableCollection<IFile> _children = new ObservableCollection<IFile>();

        #endregion

        #region Public variables

        public string Name { get; set; }
        public string Path { get; set; }

        public bool IncludeFileChildren { get => this.Children.Count() != 0; }
        public bool IsCheck
        {
            get => this._isCheck;
            set
            {
                Set(ref this._isCheck, value);

                foreach (var file in this.Children)
                    file.IsCheck = true;
            }
        }
        public bool IsCopy { get => this._isCopy; set => Set(ref this._isCopy, value); }
        public bool IsExpanded { get => this._isExpanded; set => Set(ref this._isExpanded, value); }

        public FileTypes Type
        {
            get
            {
                switch (System.IO.Path.GetExtension(this.Name))
                {
                    case ".dat":
                        return FileTypes.DataList;

                    case ".src":
                        return FileTypes.Program;

                    case "":
                        return FileTypes.Folder;

                    default:
                        return FileTypes.Unknow;
                }
            }
        }

        public FileFlags Flag { get; set; } = FileFlags.None;

        public IFile this[int index] { get => this.Children[index]; set => this.Children[index] = value; }

        public ObservableCollection<IFile> Children
        {
            get => this._children;
            set => Set(ref this._children, value);
        }

        #endregion

        #region Constructors

        public File() { }

        public File(string path)
        {
            this.Path = path;
            this.Name = path == Libr.Client.JsonRpcConnection.DefaulRoot ? path : System.IO.Path.GetFileName(path);
        }

        public File(string path, string inf) : this(path)
        {
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

        #endregion

        #region Prublic function

        public object Clone() => (File)this.MemberwiseClone();

        public IFile Search(string nameToSearchFor)
        {
            Queue<File> Q = new Queue<File>();
            HashSet<File> S = new HashSet<File>();
            Q.Enqueue(this);
            S.Add(this);

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

            //Queue<IFile> Q = new Queue<IFile>();
            //HashSet<IFile> S = new HashSet<IFile>();
            //Q.Enqueue(this);
            //S.Add(this);

            //while (Q.Count > 0)
            //{
            //    IFile e = Q.Dequeue();
            //    if (e.Name == nameToSearchFor)
            //        return e;
            //    foreach (IFile friend in e.Children)
            //    {
            //        if (!S.Contains(friend))
            //        {
            //            Q.Enqueue(friend);
            //            S.Add(friend);
            //        }
            //    }
            //}
            //return null;
        }

        #endregion
    }
}

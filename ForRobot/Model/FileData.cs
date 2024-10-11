using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ForRobot.Model
{
    //public class

    public class FileData
    {
        private string _name;
        private string _path;
        private List<FileData> _children = new List<FileData>();

        public string Name
        {
            get => this._name;
            set
            {
                this._name = value;
                OnPropertyChanged();
            }
        }

        public string Path
        {
            get => this._path;
            set
            {
                this._path = value;
                OnPropertyChanged();
            }
        }

        //public FileTypes Type
        //{
        //    get
        //    {
        //        switch (this.Name)
        //        {
        //            case string a when a.Contains(".dat"):
        //                return FileTypes.DataList;

        //            case string b when b.Contains(".src"):
        //                return FileTypes.Program;

        //            default:
        //                return FileTypes.Folder;
        //        }
        //    }
        //}

        public FileFlags Flag { get; set; }

        //public BitmapImage Icon
        //{
        //    get
        //    {
        //        switch (this.Type)
        //        {
        //            case FileTypes.Folder:
        //                return (BitmapImage)Application.Current.TryFindResource("ImageFolderIcon");

        //            case FileTypes.DataList:
        //                return (BitmapImage)Application.Current.TryFindResource("ImageDataFileIcon");

        //            case FileTypes.Program:
        //                return (BitmapImage)Application.Current.TryFindResource("ImageProgramFileIcon");

        //            default:
        //                return null;
        //        }
        //    }
        //}

        public bool ChildrenLoaded { get; set; } = false;

        public List<FileData> Children
        {
            get => this._children;
            set
            {
                this._children = value;
                OnPropertyChanged();
            }
        }

        public FileData() { }

        public FileData(string path, string inf)
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public static class FileDataCollection
    {
        /// <summary>
        /// Поиск элемента в подмножестве
        /// </summary>
        /// <param name="root">Корневой элемент поиска</param>
        /// <param name="nameToSearchFor">Имя для поиска</param>
        /// <returns></returns>
        public static FileData Search(FileData root, string nameToSearchFor)
        {
            Queue<FileData> Q = new Queue<FileData>();
            HashSet<FileData> S = new HashSet<FileData>();
            Q.Enqueue(root);
            S.Add(root);

            while (Q.Count > 0)
            {
                FileData e = Q.Dequeue();
                if (e.Name == nameToSearchFor)
                    return e;
                foreach (FileData friend in e.Children)
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

    //public class FileData : BaseClass
    //{
    //    private bool _isExpanded;
    //    private string _name;
    //    private string _path;
    //    private List<FileData> _children = new List<FileData>();
    //    protected IFileDataRoot _root;

    //    protected interface IFileDataRoot
    //    {
    //        void EnqueueUpdate();
    //    }

    //    public int Level { get; private set; }

    //    public bool IsExpanded
    //    {
    //        get => _isExpanded;
    //        set
    //        {
    //            Set(ref _isExpanded, value);
    //            _root?.EnqueueUpdate();
    //        }
    //    }

    //    public string Name
    //    {
    //        get => this._name;
    //        set => Set(ref this._name, value);
    //    }

    //    public string Path
    //    {
    //        get => this._path;
    //        set => Set(ref this._path, value);
    //    }

    //    public FileTypes Type
    //    {
    //        get
    //        {
    //            switch (this.Name)
    //            {
    //                case string a when a.Contains(".dat"):
    //                    return FileTypes.DataList;

    //                case string b when b.Contains(".src"):
    //                    return FileTypes.Program;

    //                default:
    //                    return FileTypes.Folder;
    //            }
    //        }
    //    }

    //    public FileFlags Flag
    //    {
    //        get; set;
    //        //get
    //        //{

    //        //}
    //    }

    //    public BitmapImage Icon
    //    {
    //        get
    //        {
    //            switch (this.Type)
    //            {
    //                case FileTypes.Folder:
    //                    return (BitmapImage)Application.Current.TryFindResource("ImageFolderIcon");

    //                case FileTypes.DataList:
    //                    return (BitmapImage)Application.Current.TryFindResource("ImageDataFileIcon");

    //                case FileTypes.Program:
    //                    return (BitmapImage)Application.Current.TryFindResource("ImageProgramFileIcon");

    //                default:
    //                    return null;
    //            }
    //        }
    //    }

    //    public IList<FileData> Children
    //    {
    //        get => this._children;
    //        //set => Set(ref this._children, value);
    //    }

    //    public FileData() { }

    //    public FileData(string path, string inf)
    //    {
    //        this.Path = path;
    //        this.Name = this.Path.Split(new char[] { '\\' }).Last();

    //        switch(inf.Split(new char[] { ';' }).First())
    //        {
    //            case "RVO":
    //                this.Flag = FileFlags.RVO;
    //                break;

    //            case "RVP":
    //                this.Flag = FileFlags.RVP;
    //                break;

    //            case "RVEO":
    //                this.Flag = FileFlags.RVEO;
    //                break;

    //            case "RO2":
    //                this.Flag = FileFlags.RO2;
    //                break;

    //            case "RV":
    //                this.Flag = FileFlags.RV;
    //                break;

    //            case "RV2":
    //                this.Flag = FileFlags.RV2;
    //                break;

    //            default:
    //                this.Flag = FileFlags.None;
    //                break;
    //        }
    //    }

    //    protected void SetRoot(IFileDataRoot root, int level)
    //    {
    //        _root = root;
    //        Level = root == null ? -1 : level;

    //        foreach (var child in _children)
    //            child.SetRoot(root, level + 1);
    //    }

    //    public void Insert(int index, FileData item)
    //    {
    //        if(item._root != null)
    //            throw new InvalidOperationException();

    //        this.Children.Insert(index, item);
    //        item.SetRoot(_root, Level + 1);
    //        this._root?.EnqueueUpdate();
    //    }

    //    public void Add(FileData item) => Insert(this.Children.Count, item);

    //    public void Remove(FileData item)
    //    {
    //        int index = this.Children.IndexOf(item);
    //        if (index != -1)
    //        {
    //            var child = this.Children[index];
    //            this.Children.RemoveAt(index);
    //            child.SetRoot(null, 0);
    //            _root?.EnqueueUpdate();
    //        }
    //    }

    //    //{ get => this._files ?? (this._files = new List<FileData>()); set => Set(ref this._files, value); }
    //}

    //public class FileDataCollection : FileData
    //{
    //    public List<FileData> VisibleChildren { get; set; } = new List<FileData>();

    //    public FileDataCollection()
    //    {
    //        _root = new FileDataRoot(this);
    //    }

    //    public void ForceResync() => ((FileDataRoot)_root).Update();

    //    /// <summary>
    //    /// Поиск элемента в подмножестве
    //    /// </summary>
    //    /// <param name="root">Корневой элемент поиска</param>
    //    /// <param name="nameToSearchFor">Имя для поиска</param>
    //    /// <returns></returns>
    //    public static FileData Search(FileData root, string nameToSearchFor)
    //    {
    //        Queue<FileData> Q = new Queue<FileData>();
    //        HashSet<FileData> S = new HashSet<FileData>();
    //        Q.Enqueue(root);
    //        S.Add(root);

    //        while (Q.Count > 0)
    //        {
    //            FileData e = Q.Dequeue();
    //            if (e.Name == nameToSearchFor)
    //                return e;
    //            foreach (FileData friend in e.Children)
    //            {
    //                if (!S.Contains(friend))
    //                {
    //                    Q.Enqueue(friend);
    //                    S.Add(friend);
    //                }
    //            }
    //        }
    //        return null;
    //    }

    //    public class FileDataRoot : IFileDataRoot
    //    {
    //        private readonly FileDataCollection _root;
    //        private bool _updateEnqueued;

    //        public FileDataRoot(FileDataCollection root)
    //        {
    //            this._root = root;
    //        }

    //        public void EnqueueUpdate()
    //        {
    //            if (!_updateEnqueued)
    //            {
    //                _updateEnqueued = true;                    
    //            }
    //        }

    //        private static void AppendItems(List<FileData> list, FileData node)
    //        {
    //            list.Add(node);
    //            if (node.IsExpanded)
    //                foreach (var ch in node.Children)
    //                    AppendItems(list, ch);
    //        }

    //        public void Update()
    //        {
    //            _updateEnqueued = false;
    //            var list = new List<FileData>();
    //            AppendItems(list, _root);

    //            _root.RaisePropertyChanged(nameof(_root.VisibleChildren));
    //        }
    //    }
    //}
}

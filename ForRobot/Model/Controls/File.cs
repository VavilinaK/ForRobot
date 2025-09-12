using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using ForRobot.Libr.Collections;

namespace ForRobot.Model.Controls
{
    /// <summary>
    /// Модель представления файла на роботе
    /// </summary>
    public class File : IFile, ICloneable
    {
        #region Private variables

        private bool _isCheck = false;
        private bool _isCopy = false;
        private bool _isExpanded = true;

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
                this._isCheck = value;

                foreach (var file in this.Children)
                    file.IsCheck = true;

                this.OnChangeProperty();
            }
        }

        public bool IsCopy
        {
            get => this._isCopy;
            set
            {
                this._isCopy = value;
                this.OnChangeProperty();
            }
        }

        public bool IsExpanded
        {
            get => this._isExpanded;
            set
            {
                this._isExpanded = value;
                this.OnChangeProperty();
            }
        }

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

        public FullyObservableCollection<IFile> Children { get; set; } = new FullyObservableCollection<IFile>();

        public event PropertyChangedEventHandler PropertyChanged;

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

        /// <summary>
        /// Вызов события изменения свойства
        /// </summary>
        /// <param name="propertyName">Наименование свойства</param>
        public virtual void OnChangeProperty([CallerMemberName] string propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #region Prublic function

        public object Clone() => (File)this.MemberwiseClone();

        public IFile Search(string nameToSearch)
        {
            Queue<File> Q = new Queue<File>();
            HashSet<File> S = new HashSet<File>();
            Q.Enqueue(this);
            S.Add(this);

            while (Q.Count > 0)
            {
                File e = Q.Dequeue();
                if (e.Name == nameToSearch)
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

        #endregion
    }
}

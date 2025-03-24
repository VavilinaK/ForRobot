using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.ObjectModel;

using HelixToolkit;
using HelixToolkit.Wpf;

using CommunityToolkit.Diagnostics;
//using CommunityToolkit.
//using CommunityToolkit.Mvvm.Input;

using ForRobot.Model.Detals;

namespace ForRobot.Model.File3D
{
    public class File3D : BaseClass
    {
        #region Private variables

        //MeshElement3D _object3D;

        //private readonly IHelixViewport3D viewport;
        private bool _isSaved = false;
        private Model3DGroup _currentModel = new Model3DGroup();
        private Detal _detal;

        private readonly Dispatcher dispatcher;
        private static readonly string[] ExtensionsFilter = new string[] { ".3ds", ".obj", ".objz", ".off", ".lwo", ".stl", ".ply" };

        #endregion Private variables

        #region Public variables

        /// <summary>
        /// Файл был открыт
        /// </summary>
        public bool IsOpened { get; private set; } = false;
        /// <summary>
        /// Файл был создан
        /// </summary>
        public bool IsCreated { get; private set; } = false;

        /// <summary>
        /// Сохранены ли последнии изменения
        /// </summary>
        public bool IsSaved { get; private set; }

        /// <summary>
        /// Путь к файлу
        /// </summary>
        public string Path { get; private set; } = string.Empty;
        /// <summary>
        /// Имя с расширением
        /// </summary>
        public string Name { get => System.IO.Path.GetFileName(this.Path); }
        /// <summary>
        /// Имя без расширения
        /// </summary>
        public string NameWithoutExtension { get => System.IO.Path.GetFileNameWithoutExtension(this.Path); }

        //Model3DGroup Group3D { get; set; }
        //public MeshElement3D Object3D
        //{
        //    get => this._object3D ?? (this._object3D = new CubeVisual3D()
        //                                                   {
        //                                                        SideLength = 1,
        //                                                        Center = new System.Windows.Media.Media3D.Point3D(0, 0, 0),
        //                                                        Fill = System.Windows.Media.Brushes.Gray
        //                                                   });
        //    set
        //    {
        //        this._object3D = value;
        //    }
        //}

        public Model3DGroup CurrentModel
        {
            get => this._currentModel;
            set
            {
                Set(ref this._currentModel, value);
                this.SceneUpdate();
            }
        }

        public Detal Detal { get => this._detal; set => Set(ref this._detal, value); }

        //public ObservableCollection<SceneItem> SceneObjects
        //{
        //    get => this._sceneObjects;
        //    set => Set(ref this._sceneObjects, value);
        //}

        //private ObservableCollection<SceneItem> _sceneObjects = new ObservableCollection<SceneItem>();
        private ObservableCollection<Visual3D> _sceneObjects = new ObservableCollection<Visual3D>();


        public ObservableCollection<Visual3D> SceneObjects
        {
            get => this._sceneObjects;
            set => Set(ref this._sceneObjects, value);
        }

        public static readonly string FilterForFileDialog = "3D model files (*.3ds;*.obj;*.off;*.lwo;*.stl;*.ply;)|*.3ds;*.obj;*.objz;*.off;*.lwo;*.stl;*.ply;";

        #endregion

        #region Constructor

        public File3D() => this.dispatcher = Dispatcher.CurrentDispatcher;

        public File3D(Detal detal, string path) : this()
        {
            this.Detal = detal;

            switch (this.Detal.DetalType)
            {
                case string a when a == DetalTypes.Plita:
                    this.CurrentModel = Plita.GetModel3D((Plita)this.Detal);
                    this.Detal.Change += (s, o) =>
                    {
                        //Task.Run(() => { this.CurrentModel = Plita.GetModel3D((Plita)s); });
                        this.CurrentModel = Plita.GetModel3D((Plita)s);
                    };
                    ((Plita)this.Detal).RibsCollection.ItemPropertyChanged += (s, o) =>
                    {
                        this.CurrentModel = Plita.GetModel3D((Plita)s);
                    };
                    break;

                case string b when b == DetalTypes.Stringer:
                    break;

                case string c when c == DetalTypes.Treygolnik:
                    break;
            }

            this.Path = path;
            this.IsCreated = true;
        }

        public File3D(string sPath) : this()
        {
            if (!File.Exists(sPath))
                throw new FileNotFoundException("Заданный файл не существует", sPath);

            if (ExtensionsFilter.Count(item => System.IO.Path.GetExtension(sPath) == item) == 0)
                throw new FileFormatException("Неверный формат файла");
                
            this.Path = sPath;
            this.Load(sPath);
            //this.CurrentModel = Task.Run(async () => await this.LoadAsync(Path, true)).Result;
        }

        #endregion

        #region Private functions
        
        private async Task<Model3DGroup> LoadAsync(string model3DPath, bool freeze = false)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (!freeze)
                    return new ModelImporter().Load(model3DPath, null, true);

                return new ModelImporter().Load(model3DPath, this.dispatcher);
            });
        }

        private void SceneUpdate()
        {
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Libr.Behavior.HelixSceneTrackerMessage());
        }
        //{
        //    this.SceneObjects = new ObservableCollection<SceneItem>();
        //    this.SceneObjects.Add(new SceneItem()
        //    {
        //        Name = "Scene"
        //    });
        //    //this.CurrentModel.Children.Add(new SunLight());
        //    SceneItem.AddChildren(this.SceneObjects.First(), this.CurrentModel);
        //}

        #endregion

        #region Public functions

        public static File3D Open()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog() {
                Multiselect = false,
                AddExtension = true,
                Title = "Открытие файла",
                Filter = File3D.FilterForFileDialog})
            {
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return null;

                return new File3D(openFileDialog.FileName);
            }
        }

        public void Load(string sPath)
        {
            this.CurrentModel.Children.Add(new ModelImporter().Load(sPath));
            this.SceneUpdate();
            this.IsOpened = true;
            this.IsSaved = true;
        }

        /// <summary>
        /// Открывает <see cref="SaveFileDialog"/>
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrEmpty(this.Path))
            {

            }
        }

        //public void S(EventHandler eventHandler)
        //{
        //    this.Detal.Change = eventHandler.;
        //}

        #endregion
    }
}

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.ObjectModel;

//using HelixToolkit;
using HelixToolkit.Wpf;
//using HelixToolkit.

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
        //private bool _isSaved = false;
        private Model3DGroup _currentModel = new Model3DGroup();

        private Detal _detal;
        private Detal _detalCopy; // Копия для возврата.


        private readonly Dispatcher dispatcher;
        /// <summary>
        /// Фильтр импортируемых файлов
        /// </summary>
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

        /// <summary>
        /// Подписи на 3д моделе
        /// </summary>
        public ObservableCollection<Annotation> Annotations { get; } = new ObservableCollection<Annotation>();

        public Model3DGroup CurrentModel
        {
            get => this._currentModel;
            set
            {
                Set(ref this._currentModel, value);
                //this.SceneUpdate();
            }
        }

        private void CloneDetal()
        {
            this._detalCopy = (Detal)this.Detal.Clone();
            //switch (this._detalCopy)
            //{
            //    case Plita plita:
            //        ((Plita)this._detalCopy).SetRibsCollection(((Plita)this.Detal).RibsCollection);
            //        break;
            //}
        }

        public Detal Detal
        {
            get => this._detal;
            set
            {
                Set(ref this._detal, value, false);
                this.OnModelChanged();
                this._detal.ChangePropertyEvent += (s, o) =>
                {
                    this.OnModelChanged();
                    this.ChangePropertyAnnotations(s as Detal, o as string);

                    if (this._detal.NotSaveProperties.Contains(o as string))
                        return;

                    this.TrackUndo(this._detalCopy, (Detal)this._detal.Clone());
                    this.CloneDetal();
                };
                switch (this._detal.DetalType)
                {
                    case string a when a == DetalTypes.Plita:
                        var plita = (Plita)this._detal;
                        plita.RibsCollection.ItemPropertyChanged += (s, o) =>
                        {
                            this.OnModelChanged();
                            //this.TrackUndo(this._detalCopy, plita.Clone());
                            //this.CloneDetal();
                            //this.ChangePropertyAnnotations(s as Detal, o as string);
                        };
                        break;
                }
                this.CloneDetal();
            }
        }

        //public ObservableCollection<SceneItem> SceneObjects
        //{
        //    get => this._sceneObjects;
        //    set => Set(ref this._sceneObjects, value);
        //}
        //private ObservableCollection<SceneItem> _sceneObjects = new ObservableCollection<SceneItem>();

        //private ObservableCollection<Visual3D> _sceneObjects = new ObservableCollection<Visual3D>();
        //public ObservableCollection<Visual3D> SceneObjects
        //{
        //    get => this._sceneObjects;
        //    set => Set(ref this._sceneObjects, value);
        //}

        public static readonly string FilterForFileDialog = "3D model files (*.3ds;*.obj;*.off;*.lwo;*.stl;*.ply;)|*.3ds;*.obj;*.objz;*.off;*.lwo;*.stl;*.ply;";

        #endregion

        #region Constructor

        public File3D() => this.dispatcher = Dispatcher.CurrentDispatcher;

        public File3D(Detal detal, string path) : this()
        {
            this.Detal = detal;
            this.ModelChangedEvent += (s, o) => this.CurrentModel.Children.Clear();
            this.AddAnnotations();
            switch (this.Detal.DetalType)
            {
                case string a when a == DetalTypes.Plita:
                    this.CurrentModel.Children.Add(Plita.GetModel3D((Plita)this.Detal));
                    this.ModelChangedEvent += (s, o) =>
                    {
                        var plita = (s as File3D).Detal as Plita;
                        this.CurrentModel.Children.Add(Plita.GetModel3D(plita));
                    };
                    //this.Detal.ChangeProperty += (s, o) =>
                    //{
                    //    this.OnModelChanged();
                    //    //Task.Run(() => { this.CurrentModel = Plita.GetModel3D((Plita)s); });
                    //    //this.CurrentModel = Plita.GetModel3D((Plita)s);
                    //    //this.SceneUpdate();
                    //};
                    //((Plita)this.Detal).RibsCollection.ItemPropertyChanged += (s, o) =>
                    //{
                    //    this.CurrentModel = Plita.GetModel3D((Plita)s);
                    //    this.SceneUpdate();
                    //};
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
            //GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Libr.Behavior.HelixSceneTrackerMessage());
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

        /// <summary>
        /// Добавление подписей к модели настила с рёбрами
        /// </summary>
        private void AddAnnotationsPlita()
        {
            var plita = this.Detal as Plita;

            this.Annotations.Add(new Annotation()
            {
                Position = new Point3D(20, 10, 0),
                Text = $"Length: {plita.PlateLength} mm",
                PropertyName = nameof(plita.PlateLength)
            });

            this.Annotations.Add(new Annotation()
            {
                Position = new Point3D(15, 10, 0),
                Text = $"Width: {plita.PlateWidth} mm",
                PropertyName = nameof(plita.PlateWidth)
            });

            this.Annotations.Add(new Annotation()
            {
                Position = new Point3D(10, 10, 0),
                Text = $"Thickness: {plita.PlateThickness} mm",
                PropertyName = nameof(plita.PlateThickness)
            });

            this.Annotations.Add(new Annotation()
            {
                Position = new Point3D(0, 10, 5),
                Text = $"RibHeight: {plita.RibHeight} mm",
                PropertyName = nameof(plita.RibHeight)
            });

            this.Annotations.Add(new Annotation()
            {
                Position = new Point3D(0, 10, 10),
                Text = $"RibThickness: {plita.RibThickness} mm",
                PropertyName = nameof(plita.RibThickness)
            });

            this.Annotations.Add(new Annotation()
            {
                Position = new Point3D(0, 20, 5),
                Text = $"TechOffsetSeamStart: {plita.TechOffsetSeamStart} mm",
                PropertyName = nameof(plita.TechOffsetSeamStart)
            });

            this.Annotations.Add(new Annotation()
            {
                Position = new Point3D(0, 25, 5),
                Text = $"TechOffsetSeamEnd: {plita.TechOffsetSeamEnd} mm",
                PropertyName = nameof(plita.TechOffsetSeamEnd)
            });

            this.Annotations.Add(new Annotation()
            {
                Position = new Point3D(10, 10, 5),
                Text = $"BevelToLeft: {plita.BevelToLeft} mm",
                PropertyName = nameof(plita.BevelToLeft)
            });

            this.Annotations.Add(new Annotation()
            {
                Position = new Point3D(10, 15, 5),
                Text = $"BevelToRight: {plita.BevelToRight} mm",
                PropertyName = nameof(plita.BevelToRight)
            });
        }

        private void CreateParameterArrow(Point3D start, Point3D end, string label, Color color)
        {
            //Model3DGroup model3DGroup = new Model3DGroup();

            //// Основная линия стрелки
            //LinesVisual3D line = new LinesVisual3D
            //{
            //    Points = new Point3DCollection() { start, end },
            //    //Point1 = start,
            //    //Point2 = end,
            //    Thickness = 2,
            //    Color = color
            //};

            //model3DGroup.Children.Add(line.Model);

            //// Наконечник стрелки (треугольник)
            //var direction = (end - start).Normalized();
            //var tipLength = 5; // Длина наконечника в модельных единицах
            //var tip = new MeshBuilder();
            //tip.AddArrow(start, end, tipLength, 4);

            //var tipModel = new GeometryModel3D(
            //    tip.ToMesh(),
            //    new DiffuseMaterial(new SolidColorBrush(color))
            //);
            //group.Children.Add(tipModel);

            //// Текстовая метка
            //var textPosition = Point3D.Add(start, direction * ((end - start).Length / 2));
            //var text = new TextVisual3D
            //{
            //    Position = textPosition,
            //    Text = label,
            //    FontSize = 14,
            //    Foreground = Brushes.Black,
            //    Background = Brushes.White,
            //    UpDirection = new Vector3D(0, 0, 1) // Фиксирует ориентацию текста
            //};
            //group.Children.Add(text.Model);

        }

        /// <summary>
        /// Изменение подписи одного из параметров
        /// </summary>
        /// <param name="detal"></param>
        /// <param name="propertyName">Наименование параметра</param>
        private void ChangePropertyAnnotations(Detal detal, string propertyName)
        {
            if (propertyName == null)
                return;

            Annotation annotation = this.Annotations.Count(item => item.PropertyName == propertyName) > 0 ? this.Annotations.Where(item => item.PropertyName == propertyName).First() : null;

            if (annotation == null)
                return;

            annotation.Text = detal.GetType().GetProperty(propertyName).GetValue(detal, null).ToString();
        }

        #endregion

        #region Event

        public event EventHandler ModelChangedEvent;
            
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

        public void LoadStpFile(string sPath)
        {
            //this.CurrentModel = new HelixToolkit.Wpf.A
            this.SceneUpdate();
            this.IsOpened = true;
            this.IsSaved = true;
        }

        public void Load(string sPath)
        {
            this.CurrentModel.Children.Add(new ModelImporter().Load(sPath));
            this.SceneUpdate();
            this.IsOpened = true;
            this.IsSaved = true;
        }

        public void Export()
        {
            using (SaveFileDialog sfd = new SaveFileDialog() {  })
            {

            }
        }

        /// <summary>
        /// Открывает <see cref="SaveFileDialog"/>
        /// </summary>
        public void Save()
        {
            //if (string.IsNullOrEmpty(this.Path))
            //{

            //}
        }

        /// <summary>
        /// Добавление подписей к модели
        /// </summary>
        public void AddAnnotations()
        {
            switch (this.Detal.DetalType)
            {
                case string a when a == DetalTypes.Plita:
                    this.AddAnnotationsPlita();
                    break;
            }
        }

        public void OnModelChanged() => this.ModelChangedEvent?.Invoke(this, null);

        //public void S(EventHandler eventHandler)
        //{
        //    this.Detal.Change = eventHandler.;
        //}

        #endregion
    }
}

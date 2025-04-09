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
using Assimp;
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

        private static readonly System.Windows.Media.Brush _plateBrush = new System.Windows.Media.BrushConverter().ConvertFromString("#6cc3e6") as System.Windows.Media.Brush;
        private static readonly System.Windows.Media.Brush _plateBorderBrush = new System.Windows.Media.BrushConverter().ConvertFromString("#167cf7") as System.Windows.Media.Brush;
        private static readonly System.Windows.Media.Brush _ribBrush = new System.Windows.Media.BrushConverter().ConvertFromString("#17e64b") as System.Windows.Media.Brush;
        private static readonly System.Windows.Media.Brush _ribBorderBrush = new System.Windows.Media.BrushConverter().ConvertFromString("#1a8f11") as System.Windows.Media.Brush;
        private static readonly System.Windows.Media.Brush _arrowBrush = new System.Windows.Media.BrushConverter().ConvertFromString("#ff910a") as System.Windows.Media.Brush;

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
        public ObservableCollection<SceneItem> SceneItems { get; } = new ObservableCollection<SceneItem>();

        public Model3DGroup CurrentModel
        {
            get => this._currentModel;
            set
            {
                Set(ref this._currentModel, value);
                SceneUpdate();
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

        /// <summary>
        /// Масштабный коэффициент: 1 единица модели = 250 мм реальных размеров
        /// </summary>
        public const decimal ScaleFactor = 1.00M / 250.00M;
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
                    this.CurrentModel.Children.Add(GetModel3D((Plita)this.Detal));
                    this.ModelChangedEvent += (s, o) =>
                    {
                        var plita = (s as File3D).Detal as Plita;
                        this.CurrentModel.Children.Add(GetModel3D(plita));
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
            this.ModelChangedEvent += (s, o) => SceneUpdate();
            this.Path = path;
            this.IsCreated = true;
        }

        public File3D(string sPath) : this()
        {
            if (!File.Exists(sPath))
                throw new FileNotFoundException("Заданный файл не существует", sPath);

            //if (ExtensionsFilter.Count(item => System.IO.Path.GetExtension(sPath) == item) == 0)
            //    throw new FileFormatException("Неверный формат файла");
                
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

        private void LoadAll(string path) => this.CurrentModel.Children.Add(new ModelImporter().Load(path));

        private void LoadOther(string path)
        {
            //using (var assimpContext = new AssimpContext())
            //{
            //    Scene scene = assimpContext.ImportFile(path, PostProcessSteps.None);

            //    assimpContext.ExportFile(scene, path, "obj",
            //        PostProcessSteps.FlipUVs |
            //        PostProcessSteps.GenerateNormals);
            //}


            //// Создаем контекст Assimp
            //using (var assimpContext = new AssimpContext())
            //{
            //    assimpContext.SetConfig(new Assimp.Configs.NormalSmoothingAngleConfig(66.0f));

            //    //if (!assimpContext.IsImportFormatSupported(System.IO.Path.GetExtension(path)))
            //    //    throw new NotSupportedException($"Формат {System.IO.Path.GetExtension(path)} не поддерживается.");

            //    // Параметры для сложных CAD-моделей
            //    var postProcessFlags = PostProcessSteps.Triangulate |
            //                           PostProcessSteps.GenerateNormals |
            //                           PostProcessSteps.PreTransformVertices; // Важно для иерархий

            //    // Загружаем сцену из STEP-файла
            //    Scene scene = assimpContext.ImportFile(path, postProcessFlags);

            //    var converter = new StepConverter();

            //    if (scene == null || scene.RootNode == null)
            //        throw new InvalidOperationException("Неизвестная ошибка импорта.");

            //    // Преобразуем ноды сцены в Model3DGroup
            //    ProcessNode(scene.RootNode, scene, this.CurrentModel);
            //}
        }

        private void ProcessNode(Node node, Scene scene, Model3DGroup modelGroup)
        {
            // Обрабатываем все меши в текущем узле
            foreach (var meshIndex in node.MeshIndices)
            {
                Assimp.Mesh mesh = scene.Meshes[meshIndex];
                GeometryModel3D geometryModel = ConvertMeshToGeometryModel3D(mesh);
                modelGroup.Children.Add(geometryModel);
            }

            // Рекурсивно обрабатываем дочерние узлы
            foreach (var childNode in node.Children)
            {
                ProcessNode(childNode, scene, modelGroup);
            }
        }
        
        private GeometryModel3D ConvertMeshToGeometryModel3D(Assimp.Mesh mesh)
        {
            // Создаем MeshGeometry3D для WPF
            var meshGeometry = new MeshGeometry3D();

            // Заполняем позиции вершин (с учетом смены системы координат)
            foreach (var vertex in mesh.Vertices)
            {
                meshGeometry.Positions.Add(new Point3D(
                            //vertex.X,
                            //vertex.Z,  // Инвертируем Y и Z для WPF
                            //vertex.Y

                            vertex.X * (double)ScaleFactor,
                            vertex.Z * (double)ScaleFactor,
                            vertex.Y * (double)ScaleFactor // Инверсия осей
                ));
            }

            // Заполняем индексы треугольников
            foreach (var face in mesh.Faces)
            {
                if (face.IndexCount == 3)
                {
                    meshGeometry.TriangleIndices.Add(face.Indices[0]);
                    meshGeometry.TriangleIndices.Add(face.Indices[1]);
                    meshGeometry.TriangleIndices.Add(face.Indices[2]);
                }
            }

            // Создаем материал (базовый)
            var material = new DiffuseMaterial(Brushes.Gray);

            return new GeometryModel3D(meshGeometry, material);
        }

        //private MaterialGroup ConvertMaterial(Assimp.Material assimpMaterial)
        //{
        //    var materialGroup = new MaterialGroup();
        //    var diffuseColor = new Color(
        //        (byte)(assimpMaterial.ColorDiffuse.R * 255),
        //        (byte)(assimpMaterial.ColorDiffuse.G * 255),
        //        (byte)(assimpMaterial.ColorDiffuse.B * 255)
        //    );
        //    materialGroup.Children.Add(new DiffuseMaterial(new SolidColorBrush(diffuseColor)));
        //    return materialGroup;
        //}

        #region Create 3D Model

        /// <summary>
        /// Метод создания параллелепипеда(кубоида)
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static MeshGeometry3D CreateCuboid(decimal width, decimal height, decimal length)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            // Вычисление полуразмеров для центрирования модели
            double halfWidth = (double)width / 2;
            double halfHeight = (double)height / 2;
            double halfLength = (double)length / 2;

            // Вершины кубоида (8 точек)
            mesh.Positions = new Point3DCollection(new[]
            {
                // Передняя грань (Z = -halfLength)
                new Point3D(-halfWidth, -halfHeight, -halfLength), // 0: левый нижний угол
                new Point3D(halfWidth, -halfHeight, -halfLength),  // 1: правый нижний
                new Point3D(halfWidth, halfHeight, -halfLength),   // 2: правый верхний
                new Point3D(-halfWidth, halfHeight, -halfLength),  // 3: левый верхний

                // Задняя грань (Z = halfLength)
                new Point3D(-halfWidth, -halfHeight, halfLength),  // 4: левый нижний
                new Point3D(halfWidth, -halfHeight, halfLength),   // 5: правый нижний
                new Point3D(halfWidth, halfHeight, halfLength),    // 6: правый верхний
                new Point3D(-halfWidth, halfHeight, halfLength)    // 7: левый верхний
            });

            // Индексы треугольников для всех граней
            mesh.TriangleIndices = new System.Windows.Media.Int32Collection(new[]
            {
                // Передняя грань (Z = -halfLength)
                0, 1, 2, 2, 3, 0,

                // Задняя грань (Z = halfLength)
                4, 5, 6, 6, 7, 4,

                // Нижняя грань (Y = -halfHeight)
                0, 1, 5, 5, 4, 0,

                // Верхняя грань (Y = halfHeight)
                2, 3, 7, 7, 6, 2,

                // Левая грань (X = -halfWidth)
                0, 3, 7, 7, 4, 0,

                // Правая грань (X = halfWidth)
                1, 2, 6, 6, 5, 1
            });

            return mesh;
        }

        #region Plita

        private static Model3D GetPlitaModel(Plita plita)
        {
            Model3DGroup model3DGroup = new Model3DGroup();

            // Преобразование реальных размеров в модельные (делим на 250).
            decimal modelPlateWidth = plita.PlateWidth * ScaleFactor;
            decimal modelPlateHeight = plita.PlateThickness * ScaleFactor;
            decimal modelPlateLength = plita.PlateLength * ScaleFactor;
            decimal modelPlateBevelToLeft = plita.BevelToLeft * ScaleFactor;
            decimal modelPlateBevelToRight = plita.BevelToRight * ScaleFactor;

            decimal modelRibHeight = plita.RibHeight * ScaleFactor;
            decimal modelRibThickness = plita.RibThickness * ScaleFactor;

            // Создание плиты.
            MeshGeometry3D plateMesh = CreateCuboid(modelPlateWidth, modelPlateHeight, modelPlateLength);
            GeometryModel3D plateModel = new GeometryModel3D(plateMesh, new DiffuseMaterial(_plateBrush)
            {
                //SpecularPower = 100, // Увеличивает резкость бликов
                AmbientColor = Colors.White // Улучшает контраст
            });
            model3DGroup.Children.Add(plateModel);

            // Добавление рёбер.
            decimal currentPosition = 0; // Позиционирование рёбер
            for (int i = 0; i < plita.RibCount; i++)
            {
                var rib = plita.RibsCollection[i];

                // Преобразование реальных параметров ребра в модельные
                decimal modelRibDistanceLeft = rib.DistanceLeft * ScaleFactor;
                decimal modelRibDistanceRight = rib.DistanceRight * ScaleFactor;
                decimal modelRibIdentToLeft = rib.IdentToLeft * ScaleFactor;
                decimal modelRibIdentToRight = rib.IdentToRight * ScaleFactor;

                // Расчёт позиции ребра
                currentPosition += modelRibDistanceLeft;

                // Создание ребра
                decimal ribLength = modelPlateLength - modelRibIdentToLeft - modelRibIdentToRight;
                MeshGeometry3D ribMesh = CreateCuboid(modelRibThickness, modelRibHeight, ribLength);
                GeometryModel3D ribModel = new GeometryModel3D(ribMesh, new DiffuseMaterial(_ribBrush)
                {
                    AmbientColor = Colors.White
                });

                // Позиционирование ребра
                decimal ribX = currentPosition - modelPlateWidth / 2;

                ribModel.Transform = new TranslateTransform3D((double)ribX,
                                                              (double)modelPlateHeight / 2, // Центрирование по высоте плиты
                                                              0);

                model3DGroup.Children.Add(ribModel);

                // Перемещение позиции для следующего ребра
                if (!plita.ParalleleRibs)
                    currentPosition += modelRibThickness + modelRibDistanceRight;
            }

            //if (!plita.ParalleleRibs && currentPosition > modelPlateWidth)
            //    App.Current.Logger.Error("Суммарное расстояние между рёбрами больше, чем вся ширина плиты.");

            model3DGroup.Transform = new RotateTransform3D(new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(1, 0, 0), 90)); // Поворот модели на 90 гр.
            return model3DGroup;
        }

        #endregion

        #endregion Create 3D Model

        #endregion

        #region Event

        public event EventHandler ModelChangedEvent;

        #endregion

        #region Public functions

        public static void SceneUpdate() => GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Libr.Behavior.HelixSceneTrackerMessage());
        
        public void Load(string sPath)
        {
            if (ExtensionsFilter.Contains(System.IO.Path.GetExtension(sPath)))
                this.LoadAll(sPath);
            else
                this.LoadOther(sPath);
            
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

        #region Static

        public static File3D Open()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Multiselect = false,
                AddExtension = true,
                Title = "Открытие файла",
                Filter = File3D.FilterForFileDialog
            })
            {
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return null;

                return new File3D(openFileDialog.FileName);
            }
        }

        public static Model3D GetModel3D(Detal detal)
        {
            if (detal == null)
                throw new ArgumentNullException(nameof(detal));

            switch (detal.DetalType)
            {
                case string a when a == DetalTypes.Plita:
                    return GetPlitaModel(detal as Plita);

                default:
                    return null;
            }
        }

        #endregion

        #endregion
    }
}

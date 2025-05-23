using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
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

        private readonly ForRobot.Services.IModelingService _modelingService = new ForRobot.Services.ModelingService(ForRobot.Libr.Settings.Settings.ScaleFactor);
        private readonly ForRobot.Services.IAnnotationService _annotationService = new ForRobot.Services.AnnotationService(ForRobot.Libr.Settings.Settings.ScaleFactor);
        private readonly ForRobot.Services.IWeldService _weldService = new ForRobot.Services.WeldService(ForRobot.Libr.Settings.Settings.ScaleFactor);

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
        /// Коллекция подписей на 3д моделе
        /// </summary>
        public ObservableCollection<Annotation> Annotations { get; } = new ObservableCollection<Annotation>();
        /// <summary>
        /// Коллекция швов для отображения на модели
        /// </summary>
        public ObservableCollection<Weld> WeldsCollection { get; } = new ObservableCollection<Weld>();
        //public ObservableCollection<Weld> WeldsCollection { get => this._weldsCollection; set => Set(ref this._weldsCollection, value); }

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

        public static readonly string FilterForFileDialog = "3D model files (*.3ds;*.obj;*.off;*.lwo;*.stl;*.ply;)|*.3ds;*.obj;*.objz;*.off;*.lwo;*.stl;*.ply;";

        #endregion

        #region Constructor

        public File3D() => this.dispatcher = Dispatcher.CurrentDispatcher;

        public File3D(Detal detal, string path) : this()
        {
            this.Detal = detal;
            this.ModelChangedEvent += (s, o) => this.CurrentModel.Children.Clear();
            this.Annotations = this._annotationService.GetAnnotations(this.Detal);
            switch (this.Detal.DetalType)
            {
                case DetalTypes.Plita:
                    this.CurrentModel.Children.Add(this._modelingService.ModelBuilding((Plita)this.Detal));
                    this.FillWeldsCollection(this.Detal as Plita);
                    this.ModelChangedEvent += (s, o) =>
                    {
                        var plita = (s as File3D).Detal as Plita;
                        this.CurrentModel.Children.Add(this._modelingService.ModelBuilding((Plita)this.Detal));
                        this.FillWeldsCollection(plita);
                    };
                    break;

                case DetalTypes.Stringer:
                    break;

                case DetalTypes.Treygolnik:
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

            //annotation.Text = detal.GetType().GetProperty(propertyName).GetValue(detal, null).ToString();
            annotation.Text = string.Format("{0}: {1} mm.", propertyName, detal.GetType().GetProperty(propertyName).GetValue(detal, null));
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

        private void FillWeldsCollection(Plita plate)
        {
            this.WeldsCollection.Clear();
            foreach (var item in this._weldService.GetWelds(plate))
            {
                this.WeldsCollection.Add(item);
            }
        }

        //private void ProcessNode(Node node, Scene scene, Model3DGroup modelGroup)
        //{
        //    // Обрабатываем все меши в текущем узле
        //    foreach (var meshIndex in node.MeshIndices)
        //    {
        //        Assimp.Mesh mesh = scene.Meshes[meshIndex];
        //        GeometryModel3D geometryModel = ConvertMeshToGeometryModel3D(mesh);
        //        modelGroup.Children.Add(geometryModel);
        //    }

        //    // Рекурсивно обрабатываем дочерние узлы
        //    foreach (var childNode in node.Children)
        //    {
        //        ProcessNode(childNode, scene, modelGroup);
        //    }
        //}

        //private GeometryModel3D ConvertMeshToGeometryModel3D(Assimp.Mesh mesh)
        //{
        //    // Создаем MeshGeometry3D для WPF
        //    var meshGeometry = new MeshGeometry3D();

        //    // Заполняем позиции вершин (с учетом смены системы координат)
        //    foreach (var vertex in mesh.Vertices)
        //    {
        //        meshGeometry.Positions.Add(new Point3D(
        //                    //vertex.X,
        //                    //vertex.Z,  // Инвертируем Y и Z для WPF
        //                    //vertex.Y

        //                    vertex.X * (double)ScaleFactor,
        //                    vertex.Z * (double)ScaleFactor,
        //                    vertex.Y * (double)ScaleFactor // Инверсия осей
        //        ));
        //    }

        //    // Заполняем индексы треугольников
        //    foreach (var face in mesh.Faces)
        //    {
        //        if (face.IndexCount == 3)
        //        {
        //            meshGeometry.TriangleIndices.Add(face.Indices[0]);
        //            meshGeometry.TriangleIndices.Add(face.Indices[1]);
        //            meshGeometry.TriangleIndices.Add(face.Indices[2]);
        //        }
        //    }

        //    // Создаем материал (базовый)
        //    var material = new DiffuseMaterial(Brushes.Gray);

        //    return new GeometryModel3D(meshGeometry, material);
        //}

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

        #region Model

        private static List<MeshGeometry3D> ExtractGeometries(Model3DGroup modelGroup)
        {
            //foreach (var model in group.Children)
            //{
            //    if (model is Model3DGroup subGroup)
            //    {
            //        foreach (var mesh in ExtractMeshes(subGroup))
            //            yield return mesh;
            //    }
            //    else if (model is GeometryModel3D geomModel)
            //    {
            //        if (geomModel.Geometry is MeshGeometry3D mesh)
            //            yield return mesh;
            //    }
            //}

            var geometries = new List<MeshGeometry3D>();
            foreach (var model in modelGroup.Children)
            {
                if (model is Model3DGroup group)
                {
                    geometries.AddRange(ExtractGeometries(group));
                }
                else if (model is GeometryModel3D geometryModel)
                {
                    if (geometryModel.Geometry is MeshGeometry3D mesh)
                    {
                        geometries.Add(mesh);
                    }
                }
            }
            return geometries;
        }

        /// <summary>
        /// Получение уникальных вершин
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        private static HashSet<Point3D> GetUniquePoints(MeshGeometry3D mesh) => new HashSet<Point3D>(mesh.Positions);

        /// <summary>
        /// Поиск всех рёбер
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        private static HashSet<Tuple<int, int>> GetEdges(MeshGeometry3D mesh)
        {
            var edges = new HashSet<Tuple<int, int>>();

            for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
            {
                int[] indices = new int[3] {
                                            mesh.TriangleIndices[i],
                                            mesh.TriangleIndices[i+1],
                                            mesh.TriangleIndices[i+2]
                };

                for (int j = 0; j < 3; j++)
                {
                    int a = Math.Min(indices[j], indices[(j + 1) % 3]);
                    int b = Math.Max(indices[j], indices[(j + 1) % 3]);
                    edges.Add(Tuple.Create(a, b));
                }
            }
            return edges;
        }

        /// <summary>
        /// Расчёт площади поверхности
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        private static double CalculateSurfaceArea(MeshGeometry3D mesh)
        {
            double area = 0;
            for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
            {
                var p0 = mesh.Positions[mesh.TriangleIndices[i]];
                var p1 = mesh.Positions[mesh.TriangleIndices[i + 1]];
                var p2 = mesh.Positions[mesh.TriangleIndices[i + 2]];

                System.Windows.Media.Media3D.Vector3D v1 = p1 - p0;
                System.Windows.Media.Media3D.Vector3D v2 = p2 - p0;
                area += System.Windows.Media.Media3D.Vector3D.CrossProduct(v1, v2).Length * 0.5;
            }
            return area;
        }

        /// <summary>
        /// Построение AABB
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private static Rect3D CalculateAABB(IEnumerable<Point3D> points)
        {
            var min = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            var max = new Point3D(double.MinValue, double.MinValue, double.MinValue);

            foreach (var p in points)
            {
                min.X = Math.Min(min.X, p.X);
                min.Y = Math.Min(min.Y, p.Y);
                min.Z = Math.Min(min.Z, p.Z);

                max.X = Math.Max(max.X, p.X);
                max.Y = Math.Max(max.Y, p.Y);
                max.Z = Math.Max(max.Z, p.Z);
            }

            return new Rect3D(min.X, min.Y, min.Z, 
                              max.X - min.X, max.Y - min.Y, max.Z - min.Z);
        }

        #endregion Model

        #endregion Private functions

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

        public static Detal StandartParamertrs(Detal detal)
        {
            switch (detal.DetalType)
            {
                case DetalTypes.Plita:
                    return new Plita(DetalType.Plita)
                    {
                        ScoseType = ((Plita)detal).ScoseType,
                        DiferentDistance = ((Plita)detal).DiferentDistance,
                        ParalleleRibs = ((Plita)detal).ParalleleRibs,
                        DiferentDissolutionLeft = ((Plita)detal).DiferentDissolutionLeft,
                        DiferentDissolutionRight = ((Plita)detal).DiferentDissolutionRight
                    };

                case DetalTypes.Stringer:
                    return new PlitaStringer(DetalType.Stringer);

                case DetalTypes.Treygolnik:
                    return new PlitaTreygolnik(DetalType.Treygolnik);

                default:
                    return null;
            }
        }

        #endregion

        #endregion
    }
}

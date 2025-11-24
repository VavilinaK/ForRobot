using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//using HelixToolkit;
using HelixToolkit.Wpf;
using Assimp;
//using HelixToolkit.

using CommunityToolkit.Diagnostics;
//using CommunityToolkit.
//using CommunityToolkit.Mvvm.Input;

using ForRobot.Models.Detals;
using ForRobot.Libr.Clipboard.UndoRedo;
using ForRobot.Libr.Services;

namespace ForRobot.Models.File3D
{
    public class File3D : IDisposable
    {
        #region Private variables

        private readonly ForRobot.Libr.Factories.DetalFactory.IDetalFactory _detalFactory = new ForRobot.Libr.Factories.DetalFactory.DetalFactory(new ForRobot.Libr.Configuration.ConfigurationProvider());
        private readonly ForRobot.Libr.Clipboard.UndoRedoManager _undoRedoManager;

        private Model3DGroup _currentModel = new Model3DGroup();

        private Detal _oldDetal;
        private Detal _currentDetal;
        private WeldingProperties _currentWeldingProperties;
        private ObservableCollection<Weld> _weldsCollection = new ObservableCollection<Weld>();
        
        private readonly Dispatcher dispatcher;
        /// <summary>
        /// Массив допустимых для импорта форматов 3д файлов
        /// </summary>
        private static readonly string[] File3DExtensions = new string[] { ".3ds", ".obj", ".objz", ".off", ".lwo", ".stl", ".ply" };
        /// <summary>
        /// Массив допустимых для импрта форматов текстовых файлов
        /// </summary>
        private static readonly string[] FileTextExtensions = new string[] { ".txt", ".json" };
        
        #endregion Private variables

        #region Public variables

        /// <summary>
        /// Файл был открыт
        /// </summary>
        public bool IsOpened { get; private set; } = false;        
        /// <summary>
        /// Сохранены ли последнии изменения
        /// </summary>
        public bool IsSaved { get; private set;} = true;

        public bool CanUndo => this._undoRedoManager.CanUndo;
        public bool CanRedo => this._undoRedoManager.CanRedo;

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

        public Model3DGroup CurrentModel
        {
            get => this._currentModel;
            set
            {
                this._currentModel = value;
                this._currentModel.Changed += (s, o) => this.OnModelChanged();
                this._currentModel.Children.Changed += (s, o) => this.OnModelChanged();
                this.OnModelChanged();
            }
        }

        public Detal CurrentDetal { get => this._currentDetal; set => this.SetDetal(value); }
        public WeldingProperties CurrentWeldingProperties { get; set; }

        public static readonly string FilterForFileDialog = "3D model files (*.3ds;*.obj;*.off;*.lwo;*.stl;*.ply;)|*.3ds;*.obj;*.objz;*.off;*.lwo;*.stl;*.ply;";

        #region Events

        public event EventHandler<Libr.ValueChangedEventArgs<Detal>> DetalChangedEvent;
        public event EventHandler ModelChangedEvent;
        public event EventHandler FileChangedEvent;
        public event EventHandler SaveEvent;

        #endregion

        #endregion

        #region Constructor

        //public File3D()
        //{
        //    this.dispatcher = Dispatcher.CurrentDispatcher;
        //    this.FileChangedEvent += (s, o) => this.IsSaved = false;
        //}

        public File3D()
        {
            this.dispatcher = Dispatcher.CurrentDispatcher;
            this._undoRedoManager = new ForRobot.Libr.Clipboard.UndoRedoManager(new ForRobot.Libr.Clipboard.CacheClipboardProvider(), this.Name);
        }

        public File3D(DetalType detalType, string path = null) : this()
        {
            if (path == null)
                path = System.IO.Path.GetTempPath();


        }

        //public File3D(Detal detal, string path = null) : this()
        //{
        //    this.ModelChangedEvent += (s, o) => GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Libr.Behavior.HelixSceneTrackerMessage());
        //    this.ModelChangedEvent += new ChangeService().HandleModelChanged;
        //    this.DetalChangedEvent += new ChangeService().HandleDetalChanged_Properties;
        //    this.DetalChangedEvent += new ChangeService().HandleDetalChanged_Modeling;
        //    this.FileChangedEvent += new ChangeService().HandleFileChange;
        //    this.UndoRedoStateChanged += (s, e) =>
        //    {
        //        if (s == null)
        //            return;

        //        this._oldDetal = s as Detal;
        //    };

        //    this.CurrentDetal = detal;
        //    this.Path = path;
        //    this.IsSaved = false;
        //}

        public File3D(string path) : this()
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Файл не найден по пути", path);
            
            //if (ExtensionsFilter.Count(item => System.IO.Path.GetExtension(sPath) == item) == 0)
            //    throw new FileFormatException("Неверный формат файла");

            this.Load(path);
            //this.CurrentModel = Task.Run(async () => await this.LoadAsync(Path, true)).Result;
        }

        #endregion

        #region Private functions

        #region Load

        #endregion

        private async Task<Model3DGroup> LoadAsync(string model3DPath, bool freeze = false)
        {
            return await Task.Factory.StartNew(() =>
            {
                if (!freeze)
                    return new ModelImporter().Load(model3DPath, null, true);

                return new ModelImporter().Load(model3DPath, this.dispatcher);
            });
        }
        
        private void SetDetal(object value)
        {
            if (this._currentDetal == value)
                return;

            var detals = new HashSet<Detal>();
            detals.Add(this._currentDetal);

            if (this._currentDetal != null)
            {
                this._currentDetal.ChangePropertyEvent -= HandlePropertyChange;
                this._oldDetal = this._currentDetal.Clone() as Detal;
            }

            this._currentDetal = value as Detal;

            if (this._currentDetal == null)
                return;

            this._currentDetal.ChangePropertyEvent += HandlePropertyChange;

            this.OnDetalChanged(this._oldDetal, this._currentDetal);

            if (this._oldDetal == null)
                this._oldDetal = this._currentDetal.Clone() as Detal;

            this.OnModelChanged();

            //this.DetalChangedEvent += (s, o) => SceneUpdate();
        }

        private void SetCurrentWeldingProperties(object value)
        {

        }

        /// <summary>
        /// Выгрузка 3Д модели и файла
        /// </summary>
        /// <param name="path"></param>
        private void Load3DModel(string path) => this.CurrentModel.Children.Add(new ModelImporter().Load(path));

        /// <summary>
        /// Выгрузка параметров детали из текстового файла
        /// </summary>
        /// <param name="path"></param>
        private void LoadTextFileModel(string path)
        {
            string jsonString = File.ReadAllText(path);
            string detalType = JObject.Parse(jsonString)["DetalType"].ToString();

            switch (detalType)
            {
                case DetalTypes.Plita:
                    this.CurrentDetal = new Detals.Plita().DeserializeDetal(jsonString) as Plita;
                    break;

                case DetalTypes.Stringer:
                    break;

                case DetalTypes.Treygolnik:
                    break;
            }
        }

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

        #region Models

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

        #endregion Models

        private void HandlePropertyChange(object sender, PropertyChangedEventArgs e)
        {
            Detal detal = sender as Detal;
            this.OnDetalChanged(this._oldDetal, detal);
            //this._oldDetal = this.CurrentDetal.Clone() as Detal;
            //this.ChangePropertyAnnotations(detal, e.PropertyName);
        }

        private void SaveJsonString(string path)
        {
            //if (this.Path == System.IO.Path.Combine(System.IO.Path.GetTempPath(), this.Name))
            //    System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //    {
            //        using (var fbd = new FolderBrowserDialog())
            //        {
            //            DialogResult result = fbd.ShowDialog();
            //            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            //                this.Path = System.IO.Path.Combine(fbd.SelectedPath, this.Name);
            //            else
            //                return;
            //        }
            //    });

            if (this.CurrentDetal.JsonForSave == string.Empty) return;
            File.WriteAllText(path, this.CurrentDetal.JsonForSave);
        }
        
        /// <summary>
        /// Вызов события изменения свойства <see cref="Detal"/>
        /// </summary>
        private void OnDetalChanged(Detal oldDetal, Detal newDetal)
        {
            this.DetalChangedEvent?.Invoke(this, new Libr.ValueChangedEventArgs<Detal>(oldDetal, newDetal));
            this.OnFileChanged();
        }
        /// <summary>
        /// Вызов события изменения свойства <see cref="CurrentModel"/>
        /// </summary>
        private void OnModelChanged()
        {
            this.ModelChangedEvent?.Invoke(this, null);
            this.OnFileChanged();
        }
        /// <summary>
        /// Вызов события оповещающего, что было измененно свойство класса <see cref="File3D"/>
        /// </summary>
        private void OnFileChanged() => this.FileChangedEvent?.Invoke(this, null);
        /// <summary>
        /// Вызов события сохранения файла
        /// </summary>
        private void OnSave() => this.SaveEvent?.Invoke(this, null);

        #endregion Private functions

        #region Public functions

        public void Undo() => this._undoRedoManager.Undo();

        public void Redo() => this._undoRedoManager.Redo();

        public void Load(string path)
        {
            string extension = System.IO.Path.GetExtension(path);

            // Проверка соответстия разрешения файла на доступность загрузки
            if (File3DExtensions.Contains(extension))
                this.Load3DModel(path);
            else if (FileTextExtensions.Contains(extension))
                this.LoadTextFileModel(path);
            else
                throw new FileFormatException(string.Format("Неверный формат файла: {0}", extension));

            this.Path = path;
            
            //else
            //    this.LoadOther(sPath);
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
        /// Сохранение файла по пути <see cref="File3D.Path"/>
        /// </summary>
        public void Save() => this.Save(this.Path);

        /// <summary>
        /// Сохранение файла
        /// </summary>
        /// <param name="path">Путь для сохранения</param>
        /// <returns></returns>
        public bool Save(string path)
        {
            switch (System.IO.Path.GetExtension(path))
            {
                case ".json":
                case ".txt":
                    this.SaveJsonString(path);
                    break;

                default:
                    return false;
            }
            this.IsSaved = true;
            this.OnSave();
            return true;
        }
        
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
                    //var plita = new Plita(DetalType.Plita)
                    //{
                    //    ScoseType = ((Plita)detal).ScoseType,
                    //    DiferentDistance = ((Plita)detal).DiferentDistance,
                    //    ParalleleRibs = ((Plita)detal).ParalleleRibs,
                    //    DiferentDissolutionLeft = ((Plita)detal).DiferentDissolutionLeft,
                    //    DiferentDissolutionRight = ((Plita)detal).DiferentDissolutionRight
                    //};
                    //plita.ChangePropertyEvent = detal.ChangePropertyEvent;
                    return new Plita()
                    {
                        ScoseType = ((Plita)detal).ScoseType,
                        DiferentDistance = ((Plita)detal).DiferentDistance,
                        ParalleleRibs = ((Plita)detal).ParalleleRibs,
                        DiferentDissolutionLeft = ((Plita)detal).DiferentDissolutionLeft,
                        DiferentDissolutionRight = ((Plita)detal).DiferentDissolutionRight
                    };

                case DetalTypes.Stringer:
                    return new PlitaStringer();

                case DetalTypes.Treygolnik:
                    return new PlitaTreygolnik();

                default:
                    return null;
            }
        }

        #endregion

        #endregion

        #region Implementations of IDisposable

        private volatile bool _disposed = false;

        ~File3D() => Dispose(false);

        public void Dispose() => this.Dispose(true);

        public void Dispose(bool disposing)
        {
            if (this._disposed)
                return;

            if (disposing)
            {
                this._undoRedoManager.ClearUndoRedoHistory();
                this.ModelChangedEvent -= (s, o) => GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(new Libr.Behavior.HelixSceneTrackerMessage());
                this.ModelChangedEvent -= new ChangeService().HandleModelChanged;
                this.DetalChangedEvent -= new ChangeService().HandleDetalChanged_Properties;
                this.DetalChangedEvent -= new ChangeService().HandleDetalChanged_Modeling;
                this.FileChangedEvent -= new ChangeService().HandleFileChange;
            }
            this._disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Configuration;

using Newtonsoft.Json;

using ForRobot.Model.File3D;
using ForRobot.Libr;
using ForRobot.Libr.Json;

namespace ForRobot.Model.Detals
{
    public class Detal : BaseClass
    {
        #region Private variables

        /// <summary>
        /// Масштабный коэффициент: 1 единица модели = 250 мм реальных размеров
        /// </summary>
        private const decimal ScaleFactor = 1.00M / 250.00M;

        /// <summary>
        /// Экземпляр детали из app.config
        /// </summary>
        private ForRobot.Libr.ConfigurationProperties.PlitaConfigurationSection PlitaConfig { get; set; }

        #endregion

        #region Public variables

        #region Static

        public static System.Windows.Media.Brush PlateBrush = new System.Windows.Media.BrushConverter().ConvertFromString("#6cc3e6") as System.Windows.Media.Brush;

        public static System.Windows.Media.Brush PlateBorderBrush = new System.Windows.Media.BrushConverter().ConvertFromString("#167cf7") as System.Windows.Media.Brush;

        public static System.Windows.Media.Brush RibBrush = new System.Windows.Media.BrushConverter().ConvertFromString("#17e64b") as System.Windows.Media.Brush;

        public static System.Windows.Media.Brush RibBorderBrush = new System.Windows.Media.BrushConverter().ConvertFromString("#1a8f11") as System.Windows.Media.Brush;

        public static System.Windows.Media.Brush ArrowBrush = new System.Windows.Media.BrushConverter().ConvertFromString("#ff910a") as System.Windows.Media.Brush;

        #endregion Static

        #region Virtual

        public virtual string Json { get; }
        
        public virtual string JsonForSave { get; }
        
        /// <summary>
        /// Тип детали
        /// </summary>
        public virtual string DetalType { get; }
        
        /// <summary>
        /// Длина настила
        /// </summary>
        public virtual decimal PlateLength { get; set; }

        /// <summary>
        /// Ширина настила
        /// </summary>
        public virtual decimal PlateWidth { get; set; }

        /// <summary>
        /// Толщина настила
        /// </summary>
        public virtual decimal PlateThickness { get; set; }

        /// <summary>
        /// Расстояние до осевой линии первого ребра сверху
        /// </summary>
        public virtual decimal DistanceToFirst { get; set; }
        
        /// <summary>
        /// Расстояние между осевыми линиями рёбер
        /// </summary>
        public virtual decimal DistanceBetween { get; set; }
        
        /// <summary>
        /// Расстояние торца ребра слева
        /// </summary>
        public virtual decimal IdentToLeft { get; set; }
        
        /// <summary>
        /// Расстояние торца ребра справа
        /// </summary>
        public virtual decimal IdentToRight { get; set; }
        
        /// <summary>
        /// Роспуск слева
        /// </summary>
        public virtual decimal DissolutionLeft { get; set; }
        
        /// <summary>
        /// Роспуск справа
        /// </summary>
        public virtual decimal DissolutionRight { get; set; }

        /// <summary>
        /// Высота ребра
        /// </summary>
        public virtual decimal RibHeight { get; set; }

        /// <summary>
        /// Толщина ребра
        /// </summary>
        public virtual decimal RibThickness { get; set; }
        
        /// <summary>
        /// Отступ поиска в начале
        /// </summary>
        public virtual decimal SearchOffsetStart { get; set; }
        
        /// <summary>
        /// Отступ поиска в конце
        /// </summary>
        public virtual decimal SearchOffsetEnd { get; set; }
        
        /// <summary>
        /// Перекрытие швов
        /// </summary>
        public virtual decimal SeamsOverlap { get; set; }

        /// <summary>
        /// Технологический отступ начала шва
        /// </summary>
        public virtual decimal TechOffsetSeamStart { get; set; }

        /// <summary>
        /// Технологический отступ конца шва
        /// </summary>
        public virtual decimal TechOffsetSeamEnd { get; set; }

        /// <summary>
        /// Смещение детали от 0 точки по осям XYZ
        /// </summary>
        public virtual decimal[] XYZOffset { get; set; } = new decimal[3] { 0, 0, 0 };

        /// <summary>
        /// Обратный прогиб
        /// </summary>
        public virtual decimal ReverseDeflection { get; set; }

        /// <summary>
        /// Скорость сварки
        /// </summary>
        public virtual int WildingSpead { get; set; }
        
        /// <summary>
        /// Номер сварочной программы
        /// </summary>
        public virtual int ProgramNom { get; set; }

        /// <summary>
        /// Выбранная схема сварки рёбер
        /// </summary>
        public virtual string SelectedWeldingSchema { get; set; } = WeldingSchemas.GetDescription(WeldingSchemas.SchemasTypes.LeftEvenOdd_RightEvenOdd);

        /// <summary>
        /// Схема сварки рёбер
        /// </summary>
        public virtual FullyObservableCollection<WeldingSchemas.SchemaRib> WeldingSchema { get; set; }

        /// <summary>
        /// Количество ребер
        /// </summary>
        public virtual int RibCount { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Общее изображение детали
        /// </summary>
        public virtual BitmapImage GenericImage { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Изображение рёбер
        /// </summary>
        public virtual BitmapImage RebraImage { get; set; }

        [JsonIgnore]
        /// <summary>
        /// Продольная привязка
        /// </summary>
        public virtual Privyazka LongitudinalPrivyazka { get; set; } = Privyazka.FromLeftToRight;

        [JsonIgnore]
        /// <summary>
        /// Поперечная привязка
        /// </summary>
        public virtual Privyazka TransversePrivyazka { get; set; } = Privyazka.FromLeftToRight;

        #endregion

        #endregion

        #region Event

        /// <summary>
        /// Событие изменения параметра детали
        /// </summary>
        public event EventHandler<object> ChangePropertyEvent;

        //public virtual event Func<object, EventArgs, Task> Change;

        #endregion

        #region Constructors

        public Detal() { }

        public Detal(DetalType type)
        {
            switch (type)
            {
                case ForRobot.Model.Detals.DetalType.Plita:
                    this.PlitaConfig = ConfigurationManager.GetSection("plita") as ForRobot.Libr.ConfigurationProperties.PlitaConfigurationSection;
                    this.PlateLength = PlitaConfig.Long;
                    this.PlateWidth = PlitaConfig.Width;
                    this.RibHeight = PlitaConfig.Hight;
                    this.DistanceToFirst = PlitaConfig.DistanceToFirst;
                    this.DistanceBetween = PlitaConfig.DistanceBetween;
                    this.IdentToLeft = PlitaConfig.DistanceToStart;
                    this.IdentToRight = PlitaConfig.DistanceToEnd;
                    this.DissolutionLeft = PlitaConfig.DissolutionStart;
                    this.DissolutionRight = PlitaConfig.DissolutionEnd;
                    this.PlateThickness = PlitaConfig.ThicknessPlita;
                    this.RibThickness = PlitaConfig.ThicknessRebro;
                    this.SearchOffsetStart = PlitaConfig.SearchOffsetStart;
                    this.SearchOffsetEnd = PlitaConfig.SearchOffsetEnd;
                    this.SeamsOverlap = PlitaConfig.SeamsOverlap;
                    this.TechOffsetSeamStart = PlitaConfig.TechOffsetSeamStart;
                    this.TechOffsetSeamEnd = PlitaConfig.TechOffsetSeamEnd;
                    this.ReverseDeflection = PlitaConfig.ReverseDeflection;
                    this.WildingSpead = PlitaConfig.WildingSpead;
                    this.ProgramNom = PlitaConfig.ProgramNom;
                    this.RibCount = PlitaConfig.SumReber;
                    break;
            }

            if (this.WeldingSchema == null)
                this.WeldingSchema = ForRobot.Model.Detals.WeldingSchemas.BuildingSchema(WeldingSchemas.GetSchemaType(this.SelectedWeldingSchema), this.RibCount);
        }

        #endregion

        #region Private functions

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
            GeometryModel3D plateModel = new GeometryModel3D(plateMesh, new DiffuseMaterial(Plita.PlateBrush)
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
                GeometryModel3D ribModel = new GeometryModel3D(ribMesh, new DiffuseMaterial(Plita.RibBrush)
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
                if(!plita.ParalleleRibs)
                    currentPosition += modelRibThickness + modelRibDistanceRight;
            }

            //if (!plita.ParalleleRibs && currentPosition > modelPlateWidth)
            //    App.Current.Logger.Error("Суммарное расстояние между рёбрами больше, чем вся ширина плиты.");

            model3DGroup.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90)); // Поворот модели на 90 гр.
            return model3DGroup;
        }

        //Метод создания параллелепипеда(кубоида)
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

        #endregion

        #region Public functions

        public static Detal GetDetal(string detalType)
        {
            switch (detalType)
            {
                case string a when a == DetalTypes.Plita:
                    return new Plita(Detals.DetalType.Plita);

                case string b when b == DetalTypes.Stringer:
                    return new PlitaStringer(Detals.DetalType.Stringer);

                case string c when c == DetalTypes.Treygolnik:
                    return new PlitaTreygolnik(Detals.DetalType.Treygolnik);

                default:
                    return null;
            }
        }
        
        public virtual void OnChangeProperty(string propertyName = null) => this.ChangePropertyEvent?.Invoke(this, propertyName);

        public static Model3D GetModel3D(Detal detal)
        {
            switch (detal.DetalType)
            {
                case string a when a == DetalTypes.Plita:
                    return Detal.GetPlitaModel(detal as Plita);

                default:
                    return null;
            }
        }

        #endregion
    }
}

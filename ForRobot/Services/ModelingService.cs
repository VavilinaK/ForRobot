 using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.Generic;

using HelixToolkit.Wpf;

using ForRobot.Model.Detals;

namespace ForRobot.Services
{
    public interface IModelingService
    {
        Model3DGroup ModelBuilding(Detal detal);
    }

    /// <summary>
    /// Сервис сборки 3д модели детали
    /// </summary>
    public sealed class ModelingService : IModelingService
    {
        public const double MIN_RIB_LENGTH = 1.0;

        /// <summary>
        /// Коэффициент сужения/расширения для трапеций (от 0.1 до 0.9)
        /// </summary>
        public static double TrapezoidRatio { get; set; } = 0.2;
        /// <summary>
        /// Смещение скоса
        /// </summary>
        public static double SlopeOffset { get; set; } = 10;

        public static Material PlateMaterial { get; set; } = new DiffuseMaterial(new SolidColorBrush(ForRobot.Model.File3D.Colors.PlateColor));
        public static Material RibsMaterial { get; set; } = new DiffuseMaterial(new SolidColorBrush(ForRobot.Model.File3D.Colors.RibsColor));

        /// <summary>
        /// Масштабный коэффициент: 1 единица модели = <see cref="ScaleFactor"/> мм. реальных размеров
        /// </summary>
        public decimal ScaleFactor { get; private set; } = 1.00M / 100.00M;

        #region Contructor

        public ModelingService(decimal scaleFactor)
        {
            this.ValidateScaleFactor(scaleFactor);
            this.ScaleFactor = scaleFactor;
        }

        #endregion

        #region Private Methods

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

        #region Plate Logic
        
        /// <summary>
        /// Сборка геометрии настила формой "Параллелограмм"
        /// </summary>
        /// <param name="length">Длина настила (маштабируемая)</param>
        /// <param name="width">Ширина настила (маштабируемая)</param>
        /// <param name="height">Высота настила (маштабируемая)</param>
        /// <param name="offsetDirection">Смещение скоса (отрицательно для параллелограмма влево)</param>
        /// <returns></returns>
        private MeshGeometry3D GenerateSlopePlate(double length, double width, double height, double offsetDirection)
        {
            double halfLength = length / 2;
            double halfWidth = width / 2;
            double halfHeight = height / 2;

            Point3D A = this.CreatePoint(-halfLength - offsetDirection, -halfWidth, -halfHeight);  // левый низ
            Point3D B = this.CreatePoint(halfLength - offsetDirection, -halfWidth, -halfHeight);   // правый низ
            Point3D C = this.CreatePoint(halfLength - offsetDirection, -halfWidth, halfHeight);    // правый верх
            Point3D D = this.CreatePoint(-halfLength - offsetDirection, -halfWidth, halfHeight);   // левый верх

            Point3D E = this.CreatePoint(-halfLength + offsetDirection, halfWidth, -halfHeight);  // левый низ
            Point3D F = this.CreatePoint(halfLength + offsetDirection, halfWidth, -halfHeight);   // правый низ
            Point3D G = this.CreatePoint(halfLength + offsetDirection, halfWidth, halfHeight);    // правый верх
            Point3D H = this.CreatePoint(-halfLength + offsetDirection, halfWidth, halfHeight);   // левый верх

            MeshBuilder meshBuilder = new MeshBuilder();

            // Грани плиты
            meshBuilder.AddQuad(A, B, C, D); // Задняя грань (X = -halfLength)
            meshBuilder.AddQuad(E, F, G, H); // Передняя грань (X = halfLength)
            meshBuilder.AddQuad(A, E, H, D); // Левая грань (наклонная в плоскости XY)
            meshBuilder.AddQuad(B, F, G, C); // Правая грань (наклонная в плоскости XY)
            meshBuilder.AddQuad(A, B, F, E); // Нижняя грань (параллельна оси X)
            meshBuilder.AddQuad(D, C, G, H); // Верхняя грань (параллельна оси X)

            return meshBuilder.ToMesh();
        }

        /// <summary>
        /// Сборка геометрии настила формой "Tрапиция"
        /// </summary>
        /// <param name="length">Длина настила (маштабируемая)</param>
        /// <param name="width">Ширина настила (маштабируемая)</param>
        /// <param name="height">Высота настила (маштабируемая)</param>
        /// <param name="isTop">Направлена ли трапеция вверх</param>
        /// <returns></returns>
        private MeshGeometry3D GenerateTrapezoidPlate(double length, double width, double height, bool inBottom = false)
        {
            double halfLength = length / 2;
            double halfWidth = width / 2;
            double halfHeight = height / 2;

            // Вычисление размеров оснований с учетом ориентации
            double bottomHalfLen = inBottom ? halfLength * (1 - TrapezoidRatio) : halfLength;
            double topHalfLen = inBottom ? halfLength : halfLength * (1 - TrapezoidRatio);
            
            // Нижнее основание (Y = -halfWidth)
            Point3D A = this.CreatePoint(-bottomHalfLen, -halfWidth, -halfHeight);
            Point3D B = this.CreatePoint(bottomHalfLen, -halfWidth, -halfHeight);
            Point3D C = this.CreatePoint(bottomHalfLen, -halfWidth, halfHeight);
            Point3D D = this.CreatePoint(-bottomHalfLen, -halfWidth, halfHeight);

            // Верхнее основание (Y = halfWidth)
            Point3D E = this.CreatePoint(-topHalfLen, halfWidth, -halfHeight);
            Point3D F = this.CreatePoint(topHalfLen, halfWidth, -halfHeight);
            Point3D G = this.CreatePoint(topHalfLen, halfWidth, halfHeight);
            Point3D H = this.CreatePoint(-topHalfLen, halfWidth, halfHeight);

            MeshBuilder meshBuilder = new MeshBuilder();

            // Основания с правильным порядком вершин
            meshBuilder.AddQuad(A, D, C, B);  // Нижнее основание (нормаль вниз)
            meshBuilder.AddQuad(E, F, G, H);  // Верхнее основание (нормаль вверх)

            // Боковые грани
            meshBuilder.AddQuad(A, B, F, E);  // Передняя
            meshBuilder.AddQuad(D, C, G, H);  // Задняя
            meshBuilder.AddQuad(A, E, H, D);  // Левая
            meshBuilder.AddQuad(B, F, G, C);  // Правая

            return meshBuilder.ToMesh();
        }

        private Model3DGroup AddPlate(Plita plate)
        {
            this.ValidatePlateParameters(plate);

            Model3DGroup model3DGroup = new Model3DGroup();
            MeshBuilder meshBuilder = new MeshBuilder();

            // Преобразование реальных размеров в модельные.
            double modelPlateWidth = (double)plate.PlateWidth * (double)ScaleFactor;
            double modelPlateHeight = (double)plate.PlateThickness * (double)ScaleFactor;
            double modelPlateLength = (double)plate.PlateLength * (double)ScaleFactor;

            MeshGeometry3D geometry = new MeshGeometry3D();
            switch (plate.ScoseType)
            {
                case ScoseTypes.SlopeLeft:
                    geometry = this.GenerateSlopePlate(modelPlateLength, modelPlateWidth, modelPlateHeight, -SlopeOffset);
                    break;

                case ScoseTypes.SlopeRight:
                    geometry = this.GenerateSlopePlate(modelPlateLength, modelPlateWidth, modelPlateHeight, SlopeOffset);
                    break;

                case ScoseTypes.TrapezoidTop:
                    geometry = this.GenerateTrapezoidPlate(modelPlateLength, modelPlateWidth, modelPlateHeight);
                    break;

                case ScoseTypes.TrapezoidBottom:
                    geometry = this.GenerateTrapezoidPlate(modelPlateLength, modelPlateWidth, modelPlateHeight, true);
                    break;

                case ScoseTypes.Rect:
                    meshBuilder.AddBox(new Point3D(0, 0, 0), modelPlateLength, modelPlateWidth, modelPlateHeight);
                    geometry = meshBuilder.ToMesh();
                    break;
            }
            var plateModel = new GeometryModel3D(geometry, ForRobot.Model.File3D.Materials.Plate) { BackMaterial = ForRobot.Model.File3D.Materials.Plate };
            plateModel.SetName("Plate");
            model3DGroup.Children.Add(plateModel);
            return model3DGroup;
        }

        private Model3DGroup AddRibs(Plita plate)
        {
            Model3DGroup model3DGroup = new Model3DGroup();
            MeshBuilder meshBuilder = new MeshBuilder();

            // Преобразование реальных размеров в модельные.
            double modelPlateWidth = (double)plate.PlateWidth * (double)ScaleFactor;
            double modelPlateHeight = (double)plate.PlateThickness * (double)ScaleFactor;
            double modelPlateLength = (double)plate.PlateLength * (double)ScaleFactor;
            double modelRibHeight = (double)plate.RibHeight * (double)ScaleFactor;
            double modelRibThickness = (double)plate.RibThickness * (double)ScaleFactor;
            
            double ribLeftPositionY = -modelPlateWidth / 2; // Начальная позиция по Y.
            double ribRightPositionY = -modelPlateWidth / 2;

            for (int i = 0; i < plate.RibCount; i++)
            {
                var rib = plate.RibsCollection[i];
                double modelRibDistanceLeft = (double)rib.DistanceLeft * (double)ScaleFactor;
                double modelRibDistanceRight = (double)rib.DistanceRight * (double)ScaleFactor;
                double modelRibIdentToLeft = (double)rib.IdentToLeft * (double)ScaleFactor;
                double modelRibIdentToRight = (double)rib.IdentToRight * (double)ScaleFactor;

                ribLeftPositionY += modelRibDistanceLeft;
                ribRightPositionY += modelRibDistanceRight;

                // Базовые координаты центра ребра
                double centerY = (ribLeftPositionY + ribRightPositionY) / 2 + modelRibThickness / 2;
                double centerZ = modelPlateHeight / 2 + modelRibHeight / 2;

                // Расчет смещения для скосов
                double offsetX = 0;
                double basePlateLength = modelPlateLength;
                switch (plate.ScoseType)
                {
                    case ScoseTypes.SlopeLeft:
                    case ScoseTypes.SlopeRight:
                        double positionRatio = ((ribLeftPositionY + ribRightPositionY) / 2 + modelPlateWidth / 2) / modelPlateWidth * 2 - 1;
                        offsetX = ((plate.ScoseType == ScoseTypes.SlopeLeft) ? -SlopeOffset : SlopeOffset) * positionRatio;
                        break;

                    case ScoseTypes.TrapezoidTop:
                    case ScoseTypes.TrapezoidBottom:
                        double trapezoidPositionRatio = (centerY + modelPlateWidth / 2) / modelPlateWidth;

                        if (plate.ScoseType == ScoseTypes.TrapezoidTop)
                            basePlateLength = modelPlateLength * (1 - TrapezoidRatio * trapezoidPositionRatio);
                        else
                            basePlateLength = modelPlateLength * (1 - TrapezoidRatio * (1 - trapezoidPositionRatio));
                        break;
                }

                // Длина ребра с учетом отступов и формы плиты
                double ribLength = basePlateLength - modelRibIdentToLeft - modelRibIdentToRight;
                ribLength = Math.Max(ribLength, MIN_RIB_LENGTH);

                // Центр ребра по X с учетом смещения
                double centerX = offsetX + (modelRibIdentToLeft - modelRibIdentToRight) / 2;

                MeshBuilder ribBuilder = new MeshBuilder();

                // Вычисляем координаты вершин ребра
                double halfRibLength = ribLength / 2;
                double halfRibThickness = modelRibThickness / 2;

                //Вершины нижнего основания ребра
                Point3D A = CreatePoint(-halfRibLength + centerX, ribLeftPositionY + modelRibThickness, modelPlateHeight / 2);
                Point3D B = CreatePoint(halfRibLength + centerX, ribRightPositionY + modelRibThickness, modelPlateHeight / 2);
                Point3D C = CreatePoint(halfRibLength + centerX, ribRightPositionY, modelPlateHeight / 2);
                Point3D D = CreatePoint(-halfRibLength + centerX, ribLeftPositionY, modelPlateHeight / 2);

                // Вершины верхнего основания ребра
                Point3D E = CreatePoint(-halfRibLength + centerX, ribLeftPositionY + modelRibThickness, modelPlateHeight / 2 + modelRibHeight);
                Point3D F = CreatePoint(halfRibLength + centerX, ribRightPositionY + modelRibThickness, modelPlateHeight / 2 + modelRibHeight);
                Point3D G = CreatePoint(halfRibLength + centerX, ribRightPositionY, modelPlateHeight / 2 + modelRibHeight);
                Point3D H = CreatePoint(-halfRibLength + centerX, ribLeftPositionY, modelPlateHeight / 2 + modelRibHeight);

                ribBuilder.AddQuad(A, B, C, D);  // Нижняя грань                
                ribBuilder.AddQuad(E, F, G, H); // Верхняя грань
                //Боковые грани
                ribBuilder.AddQuad(A, D, H, E); // Левая
                ribBuilder.AddQuad(B, C, G, F); // Правая
                // Торцевые грани
                ribBuilder.AddQuad(A, B, F, E); // Передняя
                ribBuilder.AddQuad(D, C, G, H); // Задняя

                GeometryModel3D ribModel = new GeometryModel3D(ribBuilder.ToMesh(), ForRobot.Model.File3D.Materials.Rib)
                {
                    BackMaterial = ForRobot.Model.File3D.Materials.Rib
                };
                ribModel.SetName($"Rib {i + 1}");
                model3DGroup.Children.Add(ribModel);
            }

            return model3DGroup;
        }
        
        private Model3DGroup GetPlateModel(Plita plate)
        {
            Model3DGroup model3DGroup = new Model3DGroup();

            // Создание плиты.
            model3DGroup.Children.Add(this.AddPlate(plate));

            // Добавление рёбер.
            model3DGroup.Children.Add(this.AddRibs(plate));

            model3DGroup.SetName("DetalModel");
            return model3DGroup;
        }

        #endregion  Plate Logic
        
        // Создание вершины с точным округлением
        private Point3D CreatePoint(double x, double y, double z) => new Point3D(Math.Round(x, 4), Math.Round(y, 4), Math.Round(z, 4));

        private void ValidateScaleFactor(decimal scaleFactor) { if (scaleFactor <= 0) throw new ArgumentException("Масштабный коэффициент должен быть больше нуля."); }

        private void ValidatePlateParameters(Plita plate)
        {
            if (plate.ScoseType == ScoseTypes.Rect) return;

            if (plate.BevelToLeft < 0 || plate.BevelToRight < 0)
                throw new ArgumentException("Скосы не могут быть отрицательными.", nameof(plate));

            if (plate.BevelToLeft + plate.BevelToRight >= plate.PlateWidth)
                throw new ArgumentException("Сумма скосов превышает ширину плиты.", nameof(plate));

            //if (plate.RibsCollection.Sum(item => item.DistanceLeft) >= plate.PlateWidth || plate.RibsCollection.Sum(item => item.DistanceRight) >= plate.PlateWidth)
            //    throw new ArgumentException("Расстояние между плитами не может превышать ширину плиты.", nameof(plate));

            //if (plate.ScoseType == ScoseTypes.SlopeLeft && plate.BevelToLeft > plate.PlateWidth)
            //    throw new ArgumentException("Смещение параллелограмма и скосы недопустимы.", nameof(plate));

            if (TrapezoidRatio < 0.1 || TrapezoidRatio > 0.9)
                throw new InvalidOperationException("Недопустимый коэффициент трапеции.");
        }

        #endregion Private Methods

        #region Public Methods

        private const int DEFAULT_DISTANCE = 7;

        /// <summary>
        /// Построение модели детали
        /// </summary>
        /// <param name="detal"></param>
        /// <returns></returns>
        public Model3DGroup ModelBuilding(Detal detal)
        {
            Model3DGroup scene = new Model3DGroup();

            double halfModelPlateLength = (double)detal.PlateLength * (double)ScaleFactor / 2;
            double halfModelPlateWidth = (double)detal.PlateWidth * (double)ScaleFactor / 2;
            double halfModelPlateHeight = (double)detal.PlateThickness * (double)ScaleFactor / 2;

            switch (detal.DetalType)
            {
                case DetalTypes.Plita:
                    Plita plate = detal as Plita;

                    var plateModel = GetPlateModel(plate);
                    plateModel.SetName("Plate");
                    scene.Children.Add(plateModel);

                    double offsetDirection = 0;
                    switch (plate.ScoseType)
                    {
                        case ScoseTypes.SlopeLeft:
                        case ScoseTypes.SlopeRight:
                            offsetDirection = SlopeOffset;
                            break;

                        case ScoseTypes.TrapezoidBottom:
                            break;
                    }

                    scene.Children.AddRobot(halfModelPlateLength + offsetDirection, -halfModelPlateWidth - DEFAULT_DISTANCE, 0, (double)ScaleFactor * 10);
                    scene.Children.AddPC(-halfModelPlateLength - offsetDirection, -halfModelPlateWidth - DEFAULT_DISTANCE, 0, (double)ScaleFactor * 200);

                    Transform3DGroup modelTransformA = new Transform3DGroup();
                    modelTransformA.Children.Add(new ScaleTransform3D((double)ScaleFactor, (double)ScaleFactor, (double)ScaleFactor));
                    modelTransformA.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(1, 0, 0), 90), new Point3D(halfModelPlateLength + 7, 0, 0)));
                    Transform3DGroup modelTransformB = modelTransformA.Clone();
                    modelTransformA.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, 0, 1), -90)));

                    scene.Children.AddMan(halfModelPlateLength + DEFAULT_DISTANCE + offsetDirection, 0, 0, (double)ScaleFactor * 150, modelTransformA);
                    scene.Children.AddMan(0, halfModelPlateWidth + 15, 0, (double)ScaleFactor * 150, modelTransformB);
                    break;

                default:
                    throw new NotSupportedException($"Ошибка построения модели: тип детали {detal.DetalType} не поддерживается!");
            }
            return scene;
        }

        private void ApplyCustomColor(Model3DGroup modelGroup, Color color)
        {
            foreach (var model in modelGroup.Children)
            {
                if (model is GeometryModel3D geometryModel)
                {
                    geometryModel.Material = new DiffuseMaterial(new SolidColorBrush(color));
                    geometryModel.BackMaterial = geometryModel.Material;
                }
                else if (model is Model3DGroup group)
                {
                    ApplyCustomColor(group, color);
                }
            }
        }

        /// <summary>
        /// Поворот вершин вокруг оси X на заданный угол (в градусах)
        /// </summary>
        /// <param name="vertices">Точки вершин</param>
        /// <param name="angleDegrees">Угол поворота (в градусах)</param>
        public static void RotateVerticesAroundX(Point3D[] vertices, double angleDegrees)
        {
            double angleRadians = angleDegrees * Math.PI / 180;
            double cos = Math.Cos(angleRadians);
            double sin = Math.Sin(angleRadians);

            for (int i = 0; i < vertices.Length; i++)
            {
                double y = vertices[i].Y;
                double z = vertices[i].Z;

                // Матрица поворота вокруг оси X:
                // Y' = Y * cosθ - Z * sinθ
                // Z' = Y * sinθ + Z * cosθ
                double newY = y * cos - z * sin;
                double newZ = y * sin + z * cos;

                vertices[i] = new Point3D(
                    vertices[i].X, // X остаётся без изменений
                    newY,
                    newZ
                );
            }
        }

        /// <summary>
        /// Поворот вершин вокруг оси Y на заданный угол (в градусах)
        /// </summary>
        /// <param name="vertices">Точки вершин</param>
        /// <param name="angleDegrees">Угол поворота (в градусах)</param>
        public static void RotateVerticesAroundY(Point3D[] vertices, double angleDegrees)
        {
            double angleRadians = angleDegrees * Math.PI / 180;
            double cos = Math.Cos(angleRadians);
            double sin = Math.Sin(angleRadians);

            for (int i = 0; i < vertices.Length; i++)
            {
                double x = vertices[i].X;
                double z = vertices[i].Z;

                // Применение матрицы поворота вокруг оси Y:
                // X' = X * cosθ - Z * sinθ
                // Z' = X * sinθ + Z * cosθ
                double newX = x * cos - z * sin;
                double newZ = x * sin + z * cos;

                vertices[i] = new Point3D(newX, vertices[i].Y, newZ);
            }
        }

        /// <summary>
        /// Поворот вершин вокруг оси Z на заданный угол (в градусах)
        /// </summary>
        /// <param name="vertices">Точки вершин</param>
        /// <param name="angleDegrees">Угол поворота (в градусах)</param>
        public static void RotateVerticesAroundZ(Point3D[] vertices, double angleDegrees)
        {
            double angleRadians = angleDegrees * Math.PI / 180;
            double cos = Math.Cos(angleRadians);
            double sin = Math.Sin(angleRadians);

            for (int i = 0; i < vertices.Length; i++)
            {
                double x = vertices[i].X;
                double y = vertices[i].Y;

                // Матрица поворота вокруг оси Z:
                // X' = X * cosθ - Y * sinθ
                // Y' = X * sinθ + Y * cosθ
                double newX = x * cos - y * sin;
                double newY = x * sin + y * cos;

                vertices[i] = new Point3D(
                    newX,
                    newY,
                    vertices[i].Z // Z остаётся без изменений
                );
            }
        }

        #endregion Public Methods
    }

    public static partial class Model3DCollection
    {
        public const string RobotModelPath = "pack://application:,,,/InterfaceOfRobots;component/3DModels/kukaRobot.stl";
        public const string PCModelPath = "pack://application:,,,/InterfaceOfRobots;component/3DModels/computer_monitor.stl";
        public const string ManModelPath = "pack://application:,,,/InterfaceOfRobots;component/3DModels/stickman.stl";

        /// <summary>
        /// Выгрузка модели из компонентов сборки
        /// </summary>
        /// <param name="modelPath"></param>
        /// <returns></returns>
        public static Model3DGroup LoadModel(string modelPath)
        {
            Model3DGroup robotModel;
            try
            {
                robotModel = new ModelImporter().Load(modelPath);
                if (robotModel == null)
                {
                    throw new System.IO.FileNotFoundException($"Не удалось загрузить модель робота по пути: {modelPath}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка загрузки модели робота: {ex.Message}", ex);
            }
            return robotModel;
        }

        /// <summary>
        /// Добавление модели робота
        /// </summary>
        /// <param name="source"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="scaleFactor">Маштабный коэффициент</param>
        public static void AddRobot(this ICollection<Model3D> source, double x, double y, double z, double scaleFactor = 10)
        {
            Vector3D robotTranslate = new Vector3D(x, y, z);

            Model3DGroup robotModel = LoadModel(RobotModelPath);

            ApplyCustomColor(robotModel, Model.File3D.Colors.RobotColor);

            Transform3DGroup modelTransform = new Transform3DGroup();
            modelTransform.Children.Add(new ScaleTransform3D(scaleFactor, scaleFactor, scaleFactor));
            modelTransform.Children.Add(new TranslateTransform3D(robotTranslate));
            modelTransform.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, 0, 1), 90), new Point3D(robotTranslate.X, robotTranslate.Y, robotTranslate.Z)));
            robotModel.Transform = modelTransform;
            robotModel.SetName(string.Format("Robot {0}", source.Count(item => item.GetName().Contains("Robot")) + 1));
            source.Add(robotModel);
        }

        /// <summary>
        /// Добавление модели компьютера
        /// </summary>
        /// <param name="source"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="scaleFactor">Маштабный коэффициент</param>
        public static void AddPC(this ICollection<Model3D> source, double x, double y, double z, double scaleFactor = 10)
        {
            Vector3D pcTranslate = new Vector3D(x, y, z);

            Model3DGroup pcModel = LoadModel(PCModelPath);

            ApplyCustomColor(pcModel, Model.File3D.Colors.PcColor);

            Transform3DGroup modelTransform = new Transform3DGroup();
            modelTransform.Children.Add(new ScaleTransform3D(scaleFactor, scaleFactor, scaleFactor));
            modelTransform.Children.Add(new TranslateTransform3D(pcTranslate));
            modelTransform.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(1, 0, 0), 90), new Point3D(pcTranslate.X, pcTranslate.Y, pcTranslate.Z)));
            modelTransform.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(0, 0, 1), 180), new Point3D(pcTranslate.X, pcTranslate.Y, pcTranslate.Z)));
            pcModel.Transform = modelTransform;
            pcModel.SetName(string.Format("PC {0}", source.Count(item => item.GetName().Contains("PC")) + 1));
            source.Add(pcModel);
        }

        /// <summary>
        /// Добавление модели человека
        /// </summary>
        /// <param name="source"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="scaleFactor">Маштабный коэффициент</param>
        /// <param name="modelTransform"></param>
        public static void AddMan(this ICollection<Model3D> source, double x, double y, double z, double scaleFactor = 10, Transform3DGroup modelTransform = null)
        {
            Vector3D manTranslate = new Vector3D(x, y, z);
            Model3DGroup manModel = LoadModel(ManModelPath);
            ApplyCustomColor(manModel, Model.File3D.Colors.WatcherColor);
            modelTransform?.Children.Add(new ScaleTransform3D(scaleFactor, scaleFactor, scaleFactor));
            modelTransform?.Children.Add(new TranslateTransform3D(manTranslate));
            manModel.Transform = modelTransform;
            manModel.SetName(string.Format("Man {0}", source.Count(item => item.GetName().Contains("Man")) + 1));
            source.Add(manModel);
        }

        private static void ApplyCustomColor(Model3DGroup modelGroup, Color color)
        {
            foreach (var model in modelGroup.Children)
            {
                if (model is GeometryModel3D geometryModel)
                {
                    geometryModel.Material = new DiffuseMaterial(new SolidColorBrush(color));
                    geometryModel.BackMaterial = geometryModel.Material;
                }
                else if (model is Model3DGroup group)
                {
                    ApplyCustomColor(group, color);
                }
            }
        }
    }
}

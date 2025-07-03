using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;

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
        /// Построение модели детали
        /// </summary>
        /// <param name="detal"></param>
        /// <returns></returns>
        public Model3DGroup ModelBuilding(Detal detal)
        {
            switch (detal.DetalType)
            {
                case DetalTypes.Plita:
                    return GetPlateModel(detal as Plita);

                default:
                    throw new NotSupportedException($"Ошибка построения модели: тип детали {detal.DetalType} не поддерживается!");
            }
        }

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

            // Создание вершин с точным округлением
            Func<double, double, double, Point3D> createPoint = (x, y, z) => new Point3D(Math.Round(x, 4), Math.Round(y, 4), Math.Round(z, 4));

            // Вершины плиты
            Point3D A = createPoint(-halfLength, -halfWidth, -halfHeight);  // передний левый низ
            Point3D B = createPoint(-halfLength, halfWidth, -halfHeight);   // передний правый низ
            Point3D C = createPoint(-halfLength, halfWidth, halfHeight);    // передний правый верх
            Point3D D = createPoint(-halfLength, -halfWidth, halfHeight);   // передний левый верх

            Point3D E = createPoint(halfLength, -halfWidth + offsetDirection, -halfHeight);  // задний левый низ
            Point3D F = createPoint(halfLength, halfWidth + offsetDirection, -halfHeight);   // задний правый низ
            Point3D G = createPoint(halfLength, halfWidth + offsetDirection, halfHeight);    // задний правый верх
            Point3D H = createPoint(halfLength, -halfWidth + offsetDirection, halfHeight);   // задний левый верх

            MeshBuilder meshBuilder = new MeshBuilder();

            // Грани плиты
            meshBuilder.AddQuad(A, B, C, D); // Передняя
            meshBuilder.AddQuad(F, E, H, G); // Задняя (обратный порядок для нормали наружу)
            meshBuilder.AddQuad(A, E, F, B); // Нижняя
            meshBuilder.AddQuad(D, C, G, H); // Верхняя
            meshBuilder.AddQuad(A, D, H, E); // Левая
            meshBuilder.AddQuad(B, F, G, C); // Правая

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
        private MeshGeometry3D GenerateTrapezoidPlate(double length, double width, double height, bool isTop = true)
        {
            double halfLength = length / 2;
            double halfWidth = width / 2;
            double halfHeight = height / 2;

            // Вычисление размеров оснований с учетом ориентации
            double bottomHalfLen = isTop ? halfLength : halfLength * (1 - TrapezoidRatio);
            double topHalfLen = isTop ? halfLength * (1 - TrapezoidRatio) : halfLength;

            // Создание вершин с точным округлением
            Func<double, double, double, Point3D> createPoint = (x, y, z) => new Point3D(Math.Round(x, 4), Math.Round(y, 4), Math.Round(z, 4));

            // Нижнее основание (Y = -halfWidth)
            Point3D A = createPoint(-bottomHalfLen, -halfWidth, -halfHeight);
            Point3D B = createPoint(bottomHalfLen, -halfWidth, -halfHeight);
            Point3D C = createPoint(bottomHalfLen, -halfWidth, halfHeight);
            Point3D D = createPoint(-bottomHalfLen, -halfWidth, halfHeight);

            // Верхнее основание (Y = halfWidth)
            Point3D E = createPoint(-topHalfLen, halfWidth, -halfHeight);
            Point3D F = createPoint(topHalfLen, halfWidth, -halfHeight);
            Point3D G = createPoint(topHalfLen, halfWidth, halfHeight);
            Point3D H = createPoint(-topHalfLen, halfWidth, halfHeight);

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

            //// Сужение верхней грани по коэффициенту трапеции
            //double topHalfWidth = width - (width * TrapezoidRatio) / 2;

            //Point3D[] vertices = new Point3D[8]
            //{
            //    // Нижнее основание (Y = -halfWidth)
            //    new Point3D(-halfLength, -halfWidth, -halfHeight), // 0: левый задний нижний
            //    new Point3D(halfLength, -halfWidth, -halfHeight),  // 1: левый передний нижний
            //    new Point3D(-halfLength, -halfWidth, halfHeight),  // 2: левый задний верхний
            //    new Point3D(halfLength, -halfWidth, halfHeight),   // 3: левый передний верхний

            //    // Верхнее основание (Y = halfWidth)
            //    new Point3D(-topHalfWidth, halfWidth, -halfHeight), // 4: правый задний нижний
            //    new Point3D(topHalfWidth, halfWidth, -halfHeight),  // 5: правый передний нижний
            //    new Point3D(-topHalfWidth, halfWidth, halfHeight),  // 6: правый задний верхний
            //    new Point3D(topHalfWidth, halfWidth, halfHeight)     // 7: правый передний верхний
            //};

            //if (!isTop)
            //{
            //    // Поворот на 180° вокруг оси X (Y → -Y, Z → -Z)
            //    for (int i = 0; i < vertices.Length; i++)
            //    {
            //        vertices[i] = new Point3D(vertices[i].X, -vertices[i].Y, -vertices[i].Z);
            //    }
            //}

            //int[] indices = new int[]
            //{
            //    // Основания (X = halfWidth и X = -halfWidth)
            //    0, 1, 3, 3, 2, 0, // Большое основание
            //    4, 5, 7, 7, 6, 4, // Малое основание

            //    // Передняя грань (Z = halfLength)
            //    0, 2, 6, 6, 4, 0,
            //    // Задняя грань (Z = -halfLength)
            //    1, 5, 7, 7, 3, 1,

            //    // Нижняя грань (Y = -halfHeight)
            //    0, 1, 5, 5, 4, 0,
            //    1, 5, 4, 4, 0, 1, // Обратные треугольники для двусторонней видимости

            //    // Верхняя грань (Y = halfHeight)
            //    2, 3, 7, 7, 6, 2,

            //    // Боковые грани
            //    0, 1, 3, 3, 2, 0,
            //    4, 5, 7, 7, 6, 4
            //};

            //return new MeshGeometry3D()
            //{
            //    Positions = new Point3DCollection(vertices),
            //    TriangleIndices = new Int32Collection(indices)
            //};
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
                    geometry = this.GenerateTrapezoidPlate(modelPlateLength, modelPlateWidth, modelPlateHeight, false);
                    break;

                case ScoseTypes.Rect:
                    meshBuilder.AddBox(new Point3D(0, 0, 0), modelPlateLength, modelPlateWidth, modelPlateHeight);
                    geometry = meshBuilder.ToMesh();
                    break;
            }
            
            model3DGroup.Children.Add(new GeometryModel3D(geometry, ForRobot.Model.File3D.Materials.Plate) { BackMaterial = ForRobot.Model.File3D.Materials.Plate });
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
            
            double ribPositionY = -modelPlateWidth / 2; // Начальная позиция по Y
            
            for (int i = 0; i < plate.RibCount; i++)
            {
                var rib = plate.RibsCollection[i];
                double modelRibDistanceLeft = (double)rib.DistanceLeft * (double)ScaleFactor;
                double modelRibDistanceRight = (double)rib.DistanceRight * (double)ScaleFactor;
                double modelRibIdentToLeft = (double)rib.IdentToLeft * (double)ScaleFactor;
                double modelRibIdentToRight = (double)rib.IdentToRight * (double)ScaleFactor;

                // Перемещение к позиции ребра по Y - отступ от торца слева.
                ribPositionY += modelRibDistanceLeft;

                // Длина ребра (с учётом отступов)
                double ribLength = modelPlateLength - modelRibIdentToLeft - modelRibIdentToRight;
                
                // Центр ребра
                double centerX = (modelRibIdentToLeft - modelRibIdentToRight) / 2;
                double centerY = ribPositionY + modelRibThickness / 2;
                double centerZ = modelPlateHeight / 2 + modelRibHeight / 2;

                switch (plate.ScoseType)
                {
                    case ScoseTypes.SlopeLeft:
                    case ScoseTypes.SlopeRight:
                        double slopeFactor = (centerY + modelPlateWidth / 2) / modelPlateWidth;
                        double plateSurfaceZ = modelPlateHeight / 2;

                        if (plate.ScoseType == ScoseTypes.SlopeLeft)
                            plateSurfaceZ += SlopeOffset * (double)ScaleFactor * (centerY / (modelPlateWidth / 2));
                        else
                            plateSurfaceZ -= SlopeOffset * (double)ScaleFactor * (centerY / (modelPlateWidth / 2));

                        // Смещение задней части ребра для скоса
                        double ribYOffset = (plate.ScoseType == ScoseTypes.SlopeLeft) ? -SlopeOffset : SlopeOffset;

                        MeshBuilder ribBuilder = new MeshBuilder();

                        // Передняя грань (X = -ribLength/2 + centerX)
                        ribBuilder.AddQuad(new Point3D(-ribLength / 2 + centerX, centerY, plateSurfaceZ),
                                           new Point3D(-ribLength / 2 + centerX, centerY + modelRibThickness, plateSurfaceZ),
                                           new Point3D(-ribLength / 2 + centerX, centerY + modelRibThickness, plateSurfaceZ + modelRibHeight),
                                           new Point3D(-ribLength / 2 + centerX, centerY, plateSurfaceZ + modelRibHeight));

                        // Задняя грань (X = ribLength/2 + centerX) со смещением по Y
                        ribBuilder.AddQuad(new Point3D(ribLength / 2 + centerX, centerY + ribYOffset, plateSurfaceZ),
                                           new Point3D(ribLength / 2 + centerX, centerY + modelRibThickness + ribYOffset, plateSurfaceZ),
                                           new Point3D(ribLength / 2 + centerX, centerY + modelRibThickness + ribYOffset, plateSurfaceZ + modelRibHeight),
                                           new Point3D(ribLength / 2 + centerX, centerY + ribYOffset, plateSurfaceZ + modelRibHeight));

                        // Боковые грани
                        ribBuilder.AddQuad(new Point3D(-ribLength / 2 + centerX, centerY, plateSurfaceZ),
                                           new Point3D(-ribLength / 2 + centerX, centerY, plateSurfaceZ + modelRibHeight),
                                           new Point3D(ribLength / 2 + centerX, centerY + ribYOffset, plateSurfaceZ + modelRibHeight),
                                           new Point3D(ribLength / 2 + centerX, centerY + ribYOffset, plateSurfaceZ)); // Левая

                        ribBuilder.AddQuad(new Point3D(-ribLength / 2 + centerX, centerY + modelRibThickness, plateSurfaceZ),
                                           new Point3D(-ribLength / 2 + centerX, centerY + modelRibThickness, plateSurfaceZ + modelRibHeight),
                                           new Point3D(ribLength / 2 + centerX, centerY + modelRibThickness + ribYOffset, plateSurfaceZ + modelRibHeight),
                                           new Point3D(ribLength / 2 + centerX, centerY + modelRibThickness + ribYOffset, plateSurfaceZ)); // Правая

                        // Верхняя и нижняя грани
                        ribBuilder.AddQuad(new Point3D(-ribLength / 2 + centerX, centerY, plateSurfaceZ + modelRibHeight),
                                           new Point3D(-ribLength / 2 + centerX, centerY + modelRibThickness, plateSurfaceZ + modelRibHeight),
                                           new Point3D(ribLength / 2 + centerX, centerY + modelRibThickness + ribYOffset, plateSurfaceZ + modelRibHeight),
                                           new Point3D(ribLength / 2 + centerX, centerY + ribYOffset, plateSurfaceZ + modelRibHeight)); // Верхняя

                        ribBuilder.AddQuad(new Point3D(-ribLength / 2 + centerX, centerY, plateSurfaceZ),
                                           new Point3D(-ribLength / 2 + centerX, centerY + modelRibThickness, plateSurfaceZ),
                                           new Point3D(ribLength / 2 + centerX, centerY + modelRibThickness + ribYOffset, plateSurfaceZ),
                                           new Point3D(ribLength / 2 + centerX, centerY + ribYOffset, plateSurfaceZ)); // Нижняя

                        GeometryModel3D ribModel = new GeometryModel3D(ribBuilder.ToMesh(), ForRobot.Model.File3D.Materials.Rib) { BackMaterial = ForRobot.Model.File3D.Materials.Rib };
                        model3DGroup.Children.Add(ribModel);
                        continue;

                    case ScoseTypes.TrapezoidTop:
                    case ScoseTypes.TrapezoidBottom:
                        // Положение ребра по ширине плиты
                        double positionRatio = (ribPositionY + modelPlateWidth / 2) / modelPlateWidth;

                        // Определяем длину основания в позиции ребра
                        double baseLength;
                        if (plate.ScoseType == ScoseTypes.TrapezoidTop)
                        {
                            // Для верхней трапеции: уменьшение длины к верхнему краю
                            baseLength = modelPlateLength * (1 - TrapezoidRatio * positionRatio);
                        }
                        else
                        {
                            // Для нижней трапеции: уменьшение длины к нижнему краю
                            baseLength = modelPlateLength * (1 - TrapezoidRatio * (1 - positionRatio));
                        }

                        // Длина ребра с учетом отступов
                        ribLength = baseLength - modelRibIdentToLeft - modelRibIdentToRight;
                        ribLength = Math.Max(ribLength, MIN_RIB_LENGTH);
                        break;
                }

                // Строим ребро:
                // - Длина вдоль X (ribLengthX)
                // - Толщина вдоль Y (currentRibThickness)
                // - Высота вдоль Z (modelRibHeight)
                meshBuilder.AddBox(new Point3D(centerX, centerY, centerZ),
                                   ribLength,
                                   modelRibThickness,
                                   modelRibHeight);

                // Перемещаем позицию для следующего ребра
                if (!plate.ParalleleRibs)
                    ribPositionY += modelRibThickness + modelRibDistanceRight;
            }

            model3DGroup.Children.Add(new GeometryModel3D(meshBuilder.ToMesh(), ForRobot.Model.File3D.Materials.Rib)
            {
                BackMaterial = ForRobot.Model.File3D.Materials.Rib
            });
            return model3DGroup;
        }

        private Model3DGroup GetPlateModel(Plita plate)
        {
            Model3DGroup model3DGroup = new Model3DGroup();

            // Создание плиты.
            model3DGroup.Children.Add(this.AddPlate(plate));

            // Добавление рёбер.
            model3DGroup.Children.Add(this.AddRibs(plate));

            //model3DGroup.Transform = new RotateTransform3D(new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(1, 0, 0), 90)); // Поворот модели на 90 гр.
            model3DGroup.SetName("DetalModel");
            return model3DGroup;
        }

        #endregion  Plate Logic

        private void ValidateScaleFactor(decimal scaleFactor) { if (scaleFactor <= 0) throw new ArgumentException("Масштабный коэффициент должен быть больше нуля."); }

        private void ValidatePlateParameters(Plita plate)
        {
            if (plate.ScoseType == ScoseTypes.Rect) return;

            if (plate.BevelToLeft < 0 || plate.BevelToRight < 0)
                throw new ArgumentException("Скосы не могут быть отрицательными.", nameof(plate));

            if (plate.BevelToLeft + plate.BevelToRight >= plate.PlateWidth)
                throw new ArgumentException("Сумма скосов превышает ширину плиты.", nameof(plate));

            //if (plate.ScoseType == ScoseTypes.SlopeLeft && plate.BevelToLeft > plate.PlateWidth)
            //    throw new ArgumentException("Смещение параллелограмма и скосы недопустимы.", nameof(plate));

            if (TrapezoidRatio < 0.1 || TrapezoidRatio > 0.9)
                throw new InvalidOperationException("Недопустимый коэффициент трапеции.");
        }

        #endregion Private Methods

        #region Public Methods

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
}

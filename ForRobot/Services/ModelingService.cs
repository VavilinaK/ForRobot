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
        Model3D ModelBuilding(Detal detal);
    }

    public sealed class ModelingService : IModelingService
    {        
        private static readonly System.Windows.Media.Brush _plateBrush = new System.Windows.Media.BrushConverter().ConvertFromString("#6cc3e6") as System.Windows.Media.Brush;
        private static readonly System.Windows.Media.Brush _plateBorderBrush = new System.Windows.Media.BrushConverter().ConvertFromString("#167cf7") as System.Windows.Media.Brush;
        private static readonly System.Windows.Media.Brush _ribBrush = new System.Windows.Media.BrushConverter().ConvertFromString("#17e64b") as System.Windows.Media.Brush;
        private static readonly System.Windows.Media.Brush _ribBorderBrush = new System.Windows.Media.BrushConverter().ConvertFromString("#1a8f11") as System.Windows.Media.Brush;
        private static readonly System.Windows.Media.Brush _arrowBrush = new System.Windows.Media.BrushConverter().ConvertFromString("#ff910a") as System.Windows.Media.Brush;

        /// <summary>
        /// Масштабный коэффициент: 1 единица модели = 200 мм реальных размеров
        /// </summary>
        public const decimal ScaleFactor = 1.00M / 150.00M;

        /// <summary>
        /// Коэффициент сужения/расширения для трапеций
        /// </summary>
        public static double TrapezoidRatio { get; set; } = 0.1;
        /// <summary>
        /// Смещение скоса
        /// </summary>
        public static double SlopeOffset { get; set; } = 12;

        public Model3D ModelBuilding(Detal detal)
        {
            switch (detal.DetalType)
            {
                case DetalTypes.Plita:
                    return GetPlateModel(detal as Plita);

                default:
                    return null;
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

        #region Plate

        //private MeshGeometry3D GenerateRectPlate(double plateLength, double plateWidth, double plateHeight)
        //{  

        //}

        /// <summary>
        /// Сборка геометрии настила формой "Параллелограмм вправо"
        /// </summary>
        /// <param name="length">Длина настила (маштабируемая)</param>
        /// <param name="width">Ширина настила (маштабируемая)</param>
        /// <param name="height">Высота настила (маштабируемая)</param>
        /// <returns></returns>
        private MeshGeometry3D GenerateSlopeLeftPlate(double length, double width, double height)
        {
            double halfLength = length / 2;
            double halfWidth = width / 2;
            double halfHeight = height / 2;
            
            Point3D[] vertices = new Point3D[8]
            {
                // Нижняя грань (Z = -halfLength)
                new Point3D(-halfWidth, -halfHeight, -halfLength),  // 0: левый нижний
                new Point3D(halfWidth, -halfHeight, -halfLength),   // 1: правый нижний
                new Point3D(halfWidth, halfHeight, -halfLength),    // 2: правый верхний
                new Point3D(-halfWidth, halfHeight, -halfLength),   // 3: левый верхний

                // Верхняя грань (Z = halfLength) со смещением вправо
                new Point3D(-halfWidth - SlopeOffset, -halfHeight, halfLength),  // 4: левый нижний смещённый
                new Point3D(halfWidth - SlopeOffset, -halfHeight, halfLength),   // 5: правый нижний смещённый
                new Point3D(halfWidth - SlopeOffset, halfHeight, halfLength),    // 6: правый верхний смещённый
                new Point3D(-halfWidth - SlopeOffset, halfHeight, halfLength)     // 7: левый верхний смещённый
            };

            int[] indices = new int[]
            {
                // Передняя грань (Z = -halfLength)
                0, 1, 2, 2, 3, 0,

                // Задняя грань (Z = halfLength)
                4, 5, 6, 6, 7, 4,

                // Нижняя грань (Y = -halfHeight)
                0, 1, 5, 5, 4, 0,

                // Верхняя грань (Y = halfHeight)
                2, 3, 7, 7, 6, 2,

                // Левая грань (X = -halfWidth + SlopeOffset)
                0, 3, 7, 7, 4, 0,

                // Правая грань (X = halfWidth + SlopeOffset)
                1, 2, 6, 6, 5, 1
            };

            return new MeshGeometry3D()
            {
                Positions = new Point3DCollection(vertices),
                TriangleIndices = new Int32Collection(indices)
            };
        }       

        /// <summary>
        /// Сборка геометрии настила формой "Параллелограмм влево"
        /// </summary>
        /// <param name="length">Длина настила (маштабируемая)</param>
        /// <param name="width">Ширина настила (маштабируемая)</param>
        /// <param name="height">Высота настила (маштабируемая)</param>
        /// <returns></returns>
        private MeshGeometry3D GenerateSlopeRightPlate(double length, double width, double height)
        {
            double halfLength = length / 2;
            double halfWidth = width / 2;
            double halfHeight = height / 2;

            double SlopeOffset = 10; // Смещение скоса.

            Point3D[] vertices = new Point3D[8]
            {
                // Нижняя грань (Z = -halfLength)
                new Point3D(-halfWidth, -halfHeight, -halfLength),  // 0: левый нижний
                new Point3D(halfWidth, -halfHeight, -halfLength),   // 1: правый нижний
                new Point3D(halfWidth, halfHeight, -halfLength),    // 2: правый верхний
                new Point3D(-halfWidth, halfHeight, -halfLength),   // 3: левый верхний

                // Верхняя грань (Z = halfLength) со смещением вправо
                new Point3D(-halfWidth + SlopeOffset, -halfHeight, halfLength),  // 4: левый нижний смещённый
                new Point3D(halfWidth + SlopeOffset, -halfHeight, halfLength),   // 5: правый нижний смещённый
                new Point3D(halfWidth + SlopeOffset, halfHeight, halfLength),    // 6: правый верхний смещённый
                new Point3D(-halfWidth + SlopeOffset, halfHeight, halfLength)     // 7: левый верхний смещённый
            };

            int[] indices = new int[]
            {
                // Передняя грань (Z = -halfLength)
                0, 1, 2, 2, 3, 0,

                // Задняя грань (Z = halfLength)
                4, 5, 6, 6, 7, 4,

                // Нижняя грань (Y = -halfHeight)
                0, 1, 5, 5, 4, 0,

                // Верхняя грань (Y = halfHeight)
                2, 3, 7, 7, 6, 2,

                // Левая грань (X = -halfWidth + SlopeOffset)
                0, 3, 7, 7, 4, 0,

                // Правая грань (X = halfWidth + SlopeOffset)
                1, 2, 6, 6, 5, 1
            };

            return new MeshGeometry3D()
            {
                Positions = new Point3DCollection(vertices),
                TriangleIndices = new Int32Collection(indices)
            };
        }

        /// <summary>
        /// Сборка геометрии настила формой "Tрапиция"
        /// </summary>
        /// <param name="length">Длина настила (маштабируемая)</param>
        /// <param name="width">Ширина настила (маштабируемая)</param>
        /// <param name="height">Высота настила (маштабируемая)</param>
        /// <returns></returns>
        private MeshGeometry3D GenerateTrapezoidTopPlate(double length, double width, double height)
        {
            double halfLength = length / 2;
            double halfWidth = width / 2;
            double halfHeight = height / 2;

            // Сужение верхней грани по коэффициенту трапеции
            double topWidthReduction = width * TrapezoidRatio;
            //double topHalfWidth = (width - topWidthReduction) / 2;
            double topHalfWidth = width - (width * TrapezoidRatio) / 2;

            Point3D[] vertices = new Point3D[8]
            {
                // Нижнее основание (X = -halfWidth)
                new Point3D(-halfWidth, -halfHeight, -halfLength), // 0: левый задний нижний
                new Point3D(-halfWidth, -halfHeight, halfLength),  // 1: левый передний нижний
                new Point3D(-halfWidth, halfHeight, -halfLength),  // 2: левый задний верхний
                new Point3D(-halfWidth, halfHeight, halfLength),   // 3: левый передний верхний

                // Верхнее основание (X = halfWidth)
                new Point3D(halfWidth, -halfHeight, -topHalfWidth), // 4: правый задний нижний
                new Point3D(halfWidth, -halfHeight, topHalfWidth),  // 5: правый передний нижний
                new Point3D(halfWidth, halfHeight, -topHalfWidth),  // 6: правый задний верхний
                new Point3D(halfWidth, halfHeight, topHalfWidth)     // 7: правый передний верхний
            };

            // Поворот на 180° вокруг оси Y (X → -X, Z → -Z)
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Point3D(-vertices[i].X, vertices[i].Y, -vertices[i].Z);
            }

            int[] indices = new int[]
            {
                // Основания (X = halfWidth и X = -halfWidth)
                0, 1, 3, 3, 2, 0, // Большое основание
                4, 5, 7, 7, 6, 4, // Малое основание

                // Передняя грань (Z = halfLength)
                0, 2, 6, 6, 4, 0,
                // Задняя грань (Z = -halfLength)
                1, 3, 7, 7, 5, 1,

                // Нижняя грань (Y = -halfHeight)
                0, 4, 5, 5, 1, 0, // По часовой стрелке
                1, 5, 4, 4, 0, 1, // Обратные треугольники для двусторонней видимости

                // Верхняя грань (Y = halfHeight)
                2, 3, 7, 7, 6, 2,

                // Боковые грани
                0, 1, 3, 3, 2, 0,
                4, 5, 7, 7, 6, 4

                //// Нижняя грань (Y = -halfHeight) — по часовой стрелке
                //0, 1, 5, 5, 4, 0,

                //// Верхняя грань (Y = halfHeight) — против часовой (нормаль наружу)
                //3, 2, 6, 6, 7, 3,

                //// Передняя грань (Z = -halfLength) — по часовой
                //0, 1, 2, 2, 3, 0,

                //// Задняя грань (Z = halfLength) — по часовой
                //4, 5, 6, 6, 7, 4,

                //// Левая боковая грань (X = -halfWidth → X = -topHalfWidth)
                //0, 3, 7, 7, 4, 0,

                //// Правая боковая грань (X = halfWidth → X = topHalfWidth)
                //1, 2, 6, 6, 5, 1,

                //// Левый скос (дополнительные треугольники для двусторонней видимости)
                //0, 4, 7, 7, 3, 0,
                //3, 7, 4, 4, 0, 3, // Обратный порядок для обратной стороны

                //// Правый скос
                //1, 5, 6, 6, 2, 1,
                //2, 6, 5, 5, 1, 2  // Обратный порядок
            };

            return new MeshGeometry3D()
            {
                Positions = new Point3DCollection(vertices),
                TriangleIndices = new Int32Collection(indices)
            };
        }

        /// <summary>
        /// Сборка геометрии настила формой "Перевёрнутая трапиция"
        /// </summary>
        /// <param name="length">Длина настила (маштабируемая)</param>
        /// <param name="width">Ширина настила (маштабируемая)</param>
        /// <param name="height">Высота настила (маштабируемая)</param>
        /// <returns></returns>
        private MeshGeometry3D GenerateTrapezoidBottomPlate(double length, double width, double height)
        {
            double halfLength = length / 2;
            double halfWidth = width / 2;
            double halfHeight = height / 2;

            // Сужение верхней грани по коэффициенту трапеции
            double topWidthReduction = width * TrapezoidRatio;
            //double topHalfWidth = (width - topWidthReduction) / 2;
            double topHalfWidth = width - (width * TrapezoidRatio) / 2;

            Point3D[] vertices = new Point3D[8]
            {
                // Нижнее основание (X = -halfWidth)
                new Point3D(-halfWidth, -halfHeight, -halfLength), // 0: левый задний нижний
                new Point3D(-halfWidth, -halfHeight, halfLength),  // 1: левый передний нижний
                new Point3D(-halfWidth, halfHeight, -halfLength),  // 2: левый задний верхний
                new Point3D(-halfWidth, halfHeight, halfLength),   // 3: левый передний верхний

                // Верхнее основание (X = halfWidth)
                new Point3D(halfWidth, -halfHeight, -topHalfWidth), // 4: правый задний нижний
                new Point3D(halfWidth, -halfHeight, topHalfWidth),  // 5: правый передний нижний
                new Point3D(halfWidth, halfHeight, -topHalfWidth),  // 6: правый задний верхний
                new Point3D(halfWidth, halfHeight, topHalfWidth)     // 7: правый передний верхний
            };

            int[] indices = new int[]
            {
                // Основания (X = halfWidth и X = -halfWidth)
                0, 1, 3, 3, 2, 0, // Большое основание
                4, 5, 7, 7, 6, 4, // Малое основание

                // Передняя грань (Z = halfLength)
                0, 2, 6, 6, 4, 0,
                // Задняя грань (Z = -halfLength)
                1, 3, 7, 7, 5, 1,

                // Нижняя грань (Y = -halfHeight)
                0, 4, 5, 5, 1, 0, // По часовой стрелке
                1, 5, 4, 4, 0, 1, // Обратные треугольники для двусторонней видимости

                // Верхняя грань (Y = halfHeight)
                2, 3, 7, 7, 6, 2,

                // Боковые грани
                0, 1, 3, 3, 2, 0,
                4, 5, 7, 7, 6, 4

                //// Нижняя грань (Y = -halfHeight) — по часовой стрелке
                //0, 1, 5, 5, 4, 0,

                //// Верхняя грань (Y = halfHeight) — против часовой (нормаль наружу)
                //3, 2, 6, 6, 7, 3,

                //// Передняя грань (Z = -halfLength) — по часовой
                //0, 1, 2, 2, 3, 0,

                //// Задняя грань (Z = halfLength) — по часовой
                //4, 5, 6, 6, 7, 4,

                //// Левая боковая грань (X = -halfWidth → X = -topHalfWidth)
                //0, 3, 7, 7, 4, 0,

                //// Правая боковая грань (X = halfWidth → X = topHalfWidth)
                //1, 2, 6, 6, 5, 1,

                //// Левый скос (дополнительные треугольники для двусторонней видимости)
                //0, 4, 7, 7, 3, 0,
                //3, 7, 4, 4, 0, 3, // Обратный порядок для обратной стороны

                //// Правый скос
                //1, 5, 6, 6, 2, 1,
                //2, 6, 5, 5, 1, 2  // Обратный порядок
            };

            return new MeshGeometry3D()
            {
                Positions = new Point3DCollection(vertices),
                TriangleIndices = new Int32Collection(indices)
            };
        }

        private Model3DGroup AddPlate(Plita plate)
        {
            Model3DGroup model3DGroup = new Model3DGroup();
            MeshBuilder meshBuilder = new MeshBuilder();

            // Преобразование реальных размеров в модельные.
            double modelPlateWidth = (double)plate.PlateWidth * (double)ScaleFactor;
            double modelPlateHeight = (double)plate.PlateThickness * (double)ScaleFactor;
            double modelPlateLength = (double)plate.PlateLength * (double)ScaleFactor;

            // Валидация.
            if(plate.ScoseType != ScoseTypes.Rect)
            {
                if (plate.BevelToLeft < 0 || plate.BevelToRight < 0)
                    throw new ArgumentException("Скосы не могут быть отрицательными.");

                if (plate.BevelToLeft + plate.BevelToRight >= plate.PlateWidth)
                    throw new ArgumentException("Сумма скосов превышает ширину плиты.");

                if (plate.ScoseType == ScoseTypes.SlopeLeft && plate.BevelToLeft > plate.PlateWidth)
                    throw new ArgumentException("Смещение параллелограмма и скосы недопустимы.");
            }

            MeshGeometry3D geometry = new MeshGeometry3D();
            switch (plate.ScoseType)
            {
                case ScoseTypes.SlopeLeft:
                    geometry = this.GenerateSlopeLeftPlate(modelPlateLength, modelPlateWidth, modelPlateHeight);
                    break;

                case ScoseTypes.SlopeRight:
                    geometry = this.GenerateSlopeRightPlate(modelPlateLength, modelPlateWidth, modelPlateHeight);
                    break;

                case ScoseTypes.TrapezoidTop:
                    geometry = this.GenerateTrapezoidTopPlate(modelPlateLength, modelPlateWidth, modelPlateHeight);
                    break;

                case ScoseTypes.TrapezoidBottom:
                    geometry = this.GenerateTrapezoidBottomPlate(modelPlateLength, modelPlateWidth, modelPlateHeight);
                    break;

                case ScoseTypes.Rect:
                    meshBuilder.AddBox(new Point3D(0, 0, 0),
                                      modelPlateWidth,
                                      modelPlateHeight,
                                      modelPlateLength);
                    geometry = meshBuilder.ToMesh();
                    break;
            }

            model3DGroup.Children.Add(new GeometryModel3D(geometry, Materials.Plate));
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

            //decimal modelBevelToLeft = plate.BevelToLeft * ScaleFactor;
            //decimal modelBevelToRight = plate.BevelToRight * ScaleFactor;

            double ribPositionXAxes = 0; // Позиционирование рёбер
            for (int i = 0; i < plate.RibCount; i++)
            {
                var rib = plate.RibsCollection[i];

                double modelRibDistanceLeft = (double)rib.DistanceLeft * (double)ScaleFactor;
                double modelRibDistanceRight = (double)rib.DistanceRight * (double)ScaleFactor;
                double modelRibIdentToLeft = (double)rib.IdentToLeft * (double)ScaleFactor;
                double modelRibIdentToRight = (double)rib.IdentToRight * (double)ScaleFactor;

                switch (plate.ScoseType)
                {
                    case ScoseTypes.SlopeLeft:
                        break;
                    case ScoseTypes.SlopeRight:
                        break;
                    case ScoseTypes.TrapezoidTop:
                        break;
                    case ScoseTypes.TrapezoidBottom:
                        break;

                    case ScoseTypes.Rect:
                        break;
                }

                ribPositionXAxes += (double)modelRibDistanceLeft; // Расчёт позиции ребра.
                double ribX = ribPositionXAxes - (double)modelPlateWidth / 2;

                double ribLength = modelPlateLength - modelRibIdentToLeft - modelRibIdentToRight; // Длина ребра.
                double ribZCenter = (modelRibIdentToRight - modelRibIdentToLeft) / 2; // Позиция по оси Z.

                meshBuilder.AddBox(
                    new Point3D(ribX, (modelRibHeight + modelPlateHeight) / 2, ribZCenter), // Позиция у верхней грани плиты
                    modelRibThickness,
                    modelRibHeight,
                    ribLength
                );

                // Перемещение позиции для следующего ребра
                if (!plate.ParalleleRibs)
                    ribPositionXAxes += (double)modelRibThickness + (double)modelRibDistanceRight;
            }
            model3DGroup.Children.Add(new GeometryModel3D(meshBuilder.ToMesh(), Materials.Rib));
            return model3DGroup;
        }

        private Model3D GetPlateModel(Plita plate)
        {
            Model3DGroup model3DGroup = new Model3DGroup();

            // Создание плиты.
            model3DGroup.Children.Add(this.AddPlate(plate));

            // Добавление рёбер.
            model3DGroup.Children.Add(this.AddRibs(plate));

            model3DGroup.Transform = new RotateTransform3D(new AxisAngleRotation3D(new System.Windows.Media.Media3D.Vector3D(1, 0, 0), 90)); // Поворот модели на 90 гр.
            return model3DGroup;
        }

        #endregion
    }

    public static class Materials
    {
        //public static Material Plate => new DiffuseMaterial(new System.Windows.Media.BrushConverter().ConvertFromString("#6cc3e6") as System.Windows.Media.Brush) { AmbientColor = Colors.White };
        public static Material Plate => new DiffuseMaterial(new System.Windows.Media.BrushConverter().ConvertFromString("#17e64b") as System.Windows.Media.Brush) { AmbientColor = Colors.White };
        public static Material Rib => new DiffuseMaterial(new System.Windows.Media.BrushConverter().ConvertFromString("#17e64b") as System.Windows.Media.Brush) { AmbientColor = Colors.White };
        public static Material Weld => new DiffuseMaterial(new System.Windows.Media.BrushConverter().ConvertFromString("#ff8e14") as System.Windows.Media.Brush) { AmbientColor = Colors.White };
        public static Material Arrow => new DiffuseMaterial(new System.Windows.Media.BrushConverter().ConvertFromString("#6cc3e6") as System.Windows.Media.Brush);
        public static Material Steel => new DiffuseMaterial(Brushes.Silver);
        public static Material Hole => new DiffuseMaterial(Brushes.DarkGray);
    }
}

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
        /// <summary>
        /// Коэффициент сужения/расширения для трапеций (от 0.1 до 0.9)
        /// </summary>
        public static double TrapezoidRatio { get; set; } = 0.2;
        /// <summary>
        /// Смещение скоса
        /// </summary>
        public static double SlopeOffset { get; set; } = 10;

        /// <summary>
        /// Масштабный коэффициент: 1 единица модели = <see cref="ScaleFactor"/> мм. реальных размеров
        /// </summary>
        public decimal ScaleFactor { get; set; } = 1.00M / 100.00M;

        public ModelingService(decimal _scaleFactor)
        {
            this.ScaleFactor = _scaleFactor;
        }
        
        /// <summary>
        /// Построение модели детали
        /// </summary>
        /// <param name="detal"></param>
        /// <returns></returns>
        public Model3D ModelBuilding(Detal detal)
        {
            switch (detal.DetalType)
            {
                case DetalTypes.Plita:
                    return GetPlateModel(detal as Plita);

                default:
                    throw new NotSupportedException($"Тип детали {detal.DetalType} не поддерживается.");
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

            Point3D[] vertices = new Point3D[8]
            {
                // Нижняя грань (Z = -halfLength)
                new Point3D(-halfWidth, -halfHeight, -halfLength),  // 0: левый нижний
                new Point3D(halfWidth, -halfHeight, -halfLength),   // 1: правый нижний
                new Point3D(halfWidth, halfHeight, -halfLength),    // 2: правый верхний
                new Point3D(-halfWidth, halfHeight, -halfLength),   // 3: левый верхний

                // Верхняя грань (Z = halfLength) со смещением вправо
                new Point3D(-halfWidth + offsetDirection, -halfHeight, halfLength),  // 4: левый нижний смещённый
                new Point3D(halfWidth + offsetDirection, -halfHeight, halfLength),   // 5: правый нижний смещённый
                new Point3D(halfWidth + offsetDirection, halfHeight, halfLength),    // 6: правый верхний смещённый
                new Point3D(-halfWidth + offsetDirection, halfHeight, halfLength)     // 7: левый верхний смещённый
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
        /// <param name="isTop">Направлена ли трапеция вверх</param>
        /// <returns></returns>
        private MeshGeometry3D GenerateTrapezoidPlate(double length, double width, double height, bool isTop = true)
        {
            double halfLength = length / 2;
            double halfWidth = width / 2;
            double halfHeight = height / 2;

            // Сужение верхней грани по коэффициенту трапеции
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

            if (!isTop)
            {
                // Поворот на 180° вокруг оси Y (X → -X, Z → -Z)
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new Point3D(-vertices[i].X, vertices[i].Y, -vertices[i].Z);
                }
            }

            int[] indices = new int[]
            {
                // Основания (X = halfWidth и X = -halfWidth)
                0, 1, 3, 3, 2, 0, // Большое основание
                4, 5, 7, 7, 6, 4, // Малое основание

                // Передняя грань (Z = halfLength)
                0, 2, 6, 6, 4, 0,
                // Задняя грань (Z = -halfLength)
                1, 5, 7, 7, 3, 1,

                // Нижняя грань (Y = -halfHeight)
                0, 1, 5, 5, 4, 0,
                1, 5, 4, 4, 0, 1, // Обратные треугольники для двусторонней видимости

                // Верхняя грань (Y = halfHeight)
                2, 3, 7, 7, 6, 2,

                // Боковые грани
                0, 1, 3, 3, 2, 0,
                4, 5, 7, 7, 6, 4
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
                    meshBuilder.AddBox(new Point3D(0, 0, 0),
                                      modelPlateWidth,
                                      modelPlateHeight,
                                      modelPlateLength);
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

            double ribPositionXAxes = 0;

            double startWidth = modelPlateLength - (double)plate.RibsCollection.First().IdentToRight * (double)ScaleFactor 
                                                 - (double)plate.RibsCollection.First().IdentToLeft * (double)ScaleFactor; // Начальная длина (для первого ребра)
            double endWidth = modelPlateLength * (1 - TrapezoidRatio)
                                                 - (double)plate.RibsCollection.Last().IdentToRight * (double)ScaleFactor
                                                 - (double)plate.RibsCollection.Last().IdentToLeft * (double)ScaleFactor; // Конечная длина (для последнего ребра)
            double commonDifference = (endWidth - startWidth) / (plate.RibCount - 1); // Разность прогрессии

            for (int i = 0; i < plate.RibCount; i++)
            {
                var rib = plate.RibsCollection[i];

                double modelRibDistanceLeft = (double)rib.DistanceLeft * (double)ScaleFactor;
                double modelRibDistanceRight = (double)rib.DistanceRight * (double)ScaleFactor;
                double modelRibIdentToLeft = (double)rib.IdentToLeft * (double)ScaleFactor;
                double modelRibIdentToRight = (double)rib.IdentToRight * (double)ScaleFactor;

                double ribLength = modelPlateLength - modelRibIdentToLeft - modelRibIdentToRight; // Длина ребра (с учётом отступов).

                ribPositionXAxes += (double)modelRibDistanceLeft; // Расчёт позиции ребра по оси X - отступ от торца слева.

                double ribX = ribPositionXAxes - (double)modelPlateWidth / 2; // Позиционирование рёбер по оси X (с учётом разного растояния).                
                double ribY = (modelRibHeight + modelPlateHeight) / 2; // Позиция по оси Y.
                double ribZCenter = (modelRibIdentToLeft - modelRibIdentToRight) / 2; // Позиция по оси Z. Середина плиты в точки - (0, 0, 0).

                switch (plate.ScoseType)
                {
                    case ScoseTypes.SlopeLeft:
                    case ScoseTypes.SlopeRight:

                        double slopeFactor = (ribX + modelPlateWidth / 2) / modelPlateWidth;
                        double plateSurfaceY = modelPlateHeight / 2;

                        if(plate.ScoseType == ScoseTypes.SlopeLeft)
                            plateSurfaceY += SlopeOffset * (double)ScaleFactor * (ribX / (modelPlateWidth / 2));
                        else
                            plateSurfaceY -= SlopeOffset * (double)ScaleFactor * (ribX / (modelPlateWidth / 2));
                            
                        double ribZ = (plate.ScoseType == ScoseTypes.SlopeLeft) ? -SlopeOffset : SlopeOffset;

                        // Вершины ребра (параллелограмм)
                        Point3D[] vertices = new Point3D[8]
                        {
                            // Передняя грань (Y = -modelRibHeight/2)
                            new Point3D(ribX, plateSurfaceY, -ribLength/2 + ribZCenter),
                            new Point3D(ribX + modelRibThickness, plateSurfaceY, -ribLength/2 + ribZCenter),
                            new Point3D(ribX + modelRibThickness, plateSurfaceY + modelRibHeight, -ribLength/2 + ribZCenter),
                            new Point3D(ribX, plateSurfaceY + modelRibHeight, -ribLength/2 + ribZCenter),

                            // Задняя грань со смещением
                            new Point3D(ribX + ribZ, plateSurfaceY, ribLength/2 + ribZCenter),
                            new Point3D(ribX + modelRibThickness + ribZ, plateSurfaceY, ribLength/2 + ribZCenter),
                            new Point3D(ribX + modelRibThickness + ribZ, plateSurfaceY + modelRibHeight, ribLength/2 + ribZCenter),
                            new Point3D(ribX + ribZ, plateSurfaceY + modelRibHeight, ribLength/2 + ribZCenter)
                        };

                        // Индексы треугольников
                        int[] indices = new int[]
                        {
                            // Передняя грань
                            0, 1, 2, 2, 3, 0,
                            // Задняя грань
                            4, 5, 6, 6, 7, 4,
                            // Левая грань
                            0, 3, 7, 7, 4, 0,
                            // Правая грань
                            1, 2, 6, 6, 5, 1,
                            // Нижняя грань (Y = -modelRibHeight/2)
                            0, 1, 5, 5, 4, 0,
                            // Верхняя грань (Y = modelRibHeight/2)
                            2, 3, 7, 7, 6, 2
                        };
                        
                        // Добавление кастомного меша
                        MeshGeometry3D ribMesh = new MeshGeometry3D()
                        {
                            Positions = new Point3DCollection(vertices),
                            TriangleIndices = new Int32Collection(indices)
                        };

                        if (!plate.ParalleleRibs)
                            ribPositionXAxes += (double)modelRibThickness + (double)modelRibDistanceRight;

                        model3DGroup.Children.Add(new GeometryModel3D(ribMesh, ForRobot.Model.File3D.Materials.Rib) { BackMaterial = ForRobot.Model.File3D.Materials.Rib });
                        continue;

                    case ScoseTypes.TrapezoidTop:
                    case ScoseTypes.TrapezoidBottom:
                        startWidth -= (modelRibIdentToLeft + modelRibIdentToRight);
                        double currentWidth = startWidth + commonDifference * i;

                        // Длина ребра с учётом отступов
                        ribLength = currentWidth - modelRibIdentToLeft - modelRibIdentToRight;
                        ribLength = Math.Max(ribLength, 1.0); // Минимальная длина
                        break;
                }

                meshBuilder.AddBox(
                    new Point3D(ribX, ribY, ribZCenter), // Позиция у верхней грани плиты
                    modelRibThickness,
                    modelRibHeight,
                    ribLength
                );

                // Перемещение позиции для следующего ребра
                if (!plate.ParalleleRibs)
                    ribPositionXAxes += (double)modelRibThickness + (double)modelRibDistanceRight;
            }
            model3DGroup.Children.Add(new GeometryModel3D(meshBuilder.ToMesh(), ForRobot.Model.File3D.Materials.Rib) { BackMaterial = ForRobot.Model.File3D.Materials.Rib });
            return model3DGroup;
        }        

        private Model3DGroup AddRibs_old(Plita plate)
        {
            Model3DGroup model3DGroup = new Model3DGroup();
            MeshBuilder meshBuilder = new MeshBuilder();

            // Преобразование реальных размеров в модельные.
            double modelPlateWidth = (double)plate.PlateWidth * (double)ScaleFactor;
            double modelPlateHeight = (double)plate.PlateThickness * (double)ScaleFactor;
            double modelPlateLength = (double)plate.PlateLength * (double)ScaleFactor;
            double modelRibHeight = (double)plate.RibHeight * (double)ScaleFactor;
            double modelRibThickness = (double)plate.RibThickness * (double)ScaleFactor;

            double widthReduction = modelPlateWidth * TrapezoidRatio;
            double ribPositionXAxes = 0; // Позиционирование рёбер
            for (int i = 0; i < plate.RibCount; i++)
            {
                var rib = plate.RibsCollection[i];

                double modelRibDistanceLeft = (double)rib.DistanceLeft * (double)ScaleFactor;
                double modelRibDistanceRight = (double)rib.DistanceRight * (double)ScaleFactor;
                double modelRibIdentToLeft = (double)rib.IdentToLeft * (double)ScaleFactor;
                double modelRibIdentToRight = (double)rib.IdentToRight * (double)ScaleFactor;

                double ribLength = modelPlateLength - modelRibIdentToLeft - modelRibIdentToRight; // Длина ребра.

                ribPositionXAxes += (double)modelRibDistanceLeft; // Расчёт позиции ребра.
                double ribX = ribPositionXAxes - (double)modelPlateWidth / 2;
                double ribZCenter = (modelRibIdentToLeft - modelRibIdentToRight) / 2; // Позиция по оси Z. Середина плиты в точки - (0, 0, 0).

                switch (plate.ScoseType)
                {
                    case ScoseTypes.SlopeLeft:
                        break;
                    case ScoseTypes.SlopeRight:
                        break;
                    case ScoseTypes.TrapezoidTop: 
                    case ScoseTypes.TrapezoidBottom:
                        // Расчёт текущей ширины плиты для трапеций
                        double positionRatio = (ribPositionXAxes + modelPlateWidth / 2) / modelPlateWidth;
                        double currentWidth = plate.ScoseType == ScoseTypes.TrapezoidTop ? modelPlateWidth - widthReduction * (1 - positionRatio)
                                                                                         : modelPlateWidth - widthReduction * positionRatio;
                        // Максимальная длина ребра с учётом отступов
                        double maxRibLength = currentWidth - modelRibIdentToLeft - modelRibIdentToRight;
                        ribLength = Math.Min(maxRibLength, ribLength);
                        ribLength = Math.Max(ribLength, 1.0);

                        //double normalizedX = (ribPositionXAxes + modelPlateWidth / 2) / modelPlateWidth;
                        //// Расчёт сужения для трапеций
                        //double lengthModifier = 1 - TrapezoidRatio * normalizedX;
                        //double xOffset = modelPlateWidth * TrapezoidRatio * normalizedX * 0.5;
                        //ribLength *= lengthModifier;
                        ////// Корректировка X для компенсации сужения
                        ////ribX = ribPositionXAxes - xOffset;

                        ////double normalizedX = (ribPositionXAxes + modelPlateWidth / 2) / modelPlateWidth;
                        ////// Расчёт сужения для трапеций
                        ////double lengthModifier = 1.0;
                        ////lengthModifier = 1 - TrapezoidRatio * Math.Abs(normalizedX - 0.5) * 2; // Учёт симметричного сужения с обеих сторон
                        ////lengthModifier = Math.Max(lengthModifier, 0.1); // Минимальное сужение 10%
                        ////ribLength *= lengthModifier;
                        ////// Корректировка X для компенсации сужения
                        ////ribX += (modelPlateWidth * TrapezoidRatio / 2) * (normalizedX - 0.5);



                        ////double normalizedX = (ribX + modelPlateWidth / 2) / modelPlateWidth;
                        ////double currentLength = plate.ScoseType == ScoseTypes.TrapezoidTop
                        ////              ? modelPlateLength - widthReduction * normalizedX
                        ////              : modelPlateLength - widthReduction * (1 - normalizedX);
                        ////ribLength = currentLength - modelRibIdentToLeft - modelRibIdentToRight;


                        ////double positionRatio = Math.Abs(ribX + modelPlateWidth / 2) / modelPlateWidth;
                        ////double widthReduction = modelPlateWidth * TrapezoidRatio * positionRatio;

                        ////// Корректировка длины ребра
                        ////ribLength -= widthReduction * 2; // Учёт сужения с двух сторон
                        //ribLength = Math.Max(ribLength, 0); // Защита от отрицательных значений
                        break;

                    case ScoseTypes.Rect:
                        break;
                }

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
            model3DGroup.Children.Add(new GeometryModel3D(meshBuilder.ToMesh(), ForRobot.Model.File3D.Materials.Rib) { BackMaterial = ForRobot.Model.File3D.Materials.Rib });
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
}

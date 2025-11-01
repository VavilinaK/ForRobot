using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;

using HelixToolkit.Wpf;

using ForRobot.Model.Detals;
using ForRobot.Model.File3D;

namespace ForRobot.Strategies.ModelingStrategies
{
    /// <summary>
    /// Реализация стратегии <see cref="IDetalModelingStrategy"/> класса <see cref="ForRobot.Model.Detals.Plita"/>
    /// </summary>
    public class PlateModelingStrategy : IDetalModelingStrategy
    {
        /// <summary>
        /// Масштабный коэффициент
        /// </summary>
        private readonly double _scaleFactor;
        /// <summary>
        /// Коэффициент сужения/расширения для трапеций (от 0.1 до 0.9)
        /// </summary>
        private readonly double _trapezoidRatio;
        /// <summary>
        /// Смещение скоса
        /// </summary>
        private readonly double _slopeOffset;

        public PlateModelingStrategy(double scaleFactor, double slopeOffset = 10, double trapezoidRatio = 0.2)
        {
            if (slopeOffset <= 0)
                throw new ArgumentException("Масштабный коэффициент должен быть больше нуля.");

            this._scaleFactor = scaleFactor;
            this._slopeOffset = slopeOffset;
            this._trapezoidRatio = trapezoidRatio;
        }

        #region Private functions

        /// <summary>
        /// Создание вершины с точным округлением до 4 знаков после запятой
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private Point3D CreatePoint(double x, double y, double z) => new Point3D(Math.Round(x, 4), Math.Round(y, 4), Math.Round(z, 4));

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
            double bottomHalfLen = inBottom ? halfLength * (1 - this._trapezoidRatio) : halfLength;
            double topHalfLen = inBottom ? halfLength : halfLength * (1 - this._trapezoidRatio
);

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

        /// <summary>
        /// Добавление плиты
        /// </summary>
        /// <param name="plate"></param>
        /// <returns></returns>
        private Model3DGroup AddPlate(Plita plate)
        {
            this.ValidatePlateParameters(plate);

            Model3DGroup model3DGroup = new Model3DGroup();
            MeshBuilder meshBuilder = new MeshBuilder();

            // Преобразование реальных размеров в модельные.
            double modelPlateWidth = (double)plate.PlateWidth * this._scaleFactor;
            double modelPlateHeight = (double)plate.PlateThickness * this._scaleFactor;
            double modelPlateLength = (double)plate.PlateLength * this._scaleFactor;

            MeshGeometry3D geometry = new MeshGeometry3D();
            switch (plate.ScoseType)
            {
                case ScoseTypes.SlopeLeft:
                    geometry = this.GenerateSlopePlate(modelPlateLength, modelPlateWidth, modelPlateHeight, -this._slopeOffset);
                    break;

                case ScoseTypes.SlopeRight:
                    geometry = this.GenerateSlopePlate(modelPlateLength, modelPlateWidth, modelPlateHeight, this._slopeOffset);
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

        /// <summary>
        /// Добавление рёбер
        /// </summary>
        /// <param name="plate"></param>
        /// <returns></returns>
        private Model3DGroup AddRibs(Plita plate)
        {
            Model3DGroup model3DGroup = new Model3DGroup();
            MeshBuilder meshBuilder = new MeshBuilder();

            // Преобразование реальных размеров в модельные.
            double modelPlateWidth = (double)plate.PlateWidth * this._scaleFactor;
            double modelPlateHeight = (double)plate.PlateThickness * this._scaleFactor;
            double modelPlateLength = (double)plate.PlateLength * this._scaleFactor;
            double modelRibHeight = (double)plate.RibHeight * this._scaleFactor;
            double modelRibThickness = (double)plate.RibThickness * this._scaleFactor;

            double ribLeftPositionY = -modelPlateWidth / 2; // Начальная позиция по Y.
            double ribRightPositionY = -modelPlateWidth / 2;

            for (int i = 0; i < plate.RibCount; i++)
            {
                var rib = plate.RibsCollection[i];
                double modelRibDistanceLeft = (double)rib.DistanceLeft * this._scaleFactor;
                double modelRibDistanceRight = (double)rib.DistanceRight * this._scaleFactor;
                double modelRibIdentToLeft = (double)rib.IdentToLeft * this._scaleFactor;
                double modelRibIdentToRight = (double)rib.IdentToRight * this._scaleFactor;

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
                        offsetX = ((plate.ScoseType == ScoseTypes.SlopeLeft) ? -this._slopeOffset : this._slopeOffset) * positionRatio;
                        break;

                    case ScoseTypes.TrapezoidTop:
                    case ScoseTypes.TrapezoidBottom:
                        double trapezoidPositionRatio = (centerY + modelPlateWidth / 2) / modelPlateWidth;

                        if (plate.ScoseType == ScoseTypes.TrapezoidTop)
                            basePlateLength = modelPlateLength * (1 - this._trapezoidRatio * trapezoidPositionRatio);
                        else
                            basePlateLength = modelPlateLength * (1 - this._trapezoidRatio * (1 - trapezoidPositionRatio));
                        break;
                }

                // Длина ребра с учетом отступов и формы плиты
                double ribLength = basePlateLength - modelRibIdentToLeft - modelRibIdentToRight;
                ribLength = Math.Max(ribLength, ForRobot.Services.ModelingService.MIN_RIB_LENGTH);

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

        /// <summary>
        /// Валидация параметров плиты
        /// </summary>
        /// <param name="plate"></param>
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

            if (this._trapezoidRatio < 0.1 || this._trapezoidRatio > 0.9)
                throw new InvalidOperationException("Недопустимый коэффициент трапеции, вне (0.1; 0.9).");
        }

        #endregion Private functions

        public bool CanHandle(DetalType detalType) => detalType == DetalType.Plita;

        public Model3DGroup CreateModel3D(Detal detal)
        {
            Model3DGroup model3DGroup = new Model3DGroup();
            Plita plate = (Plita)detal;            
            model3DGroup.Children.Add(this.AddPlate(plate)); // Создание плиты.            
            model3DGroup.Children.Add(this.AddRibs(plate)); // Добавление рёбер.
            model3DGroup.SetName("DetalModel");
            return model3DGroup;
        }
    }
}

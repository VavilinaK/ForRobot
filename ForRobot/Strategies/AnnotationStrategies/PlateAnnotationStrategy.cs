using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;

using ForRobot.Model.Detals;
using ForRobot.Model.File3D;
using ForRobot.Services;

namespace ForRobot.Strategies.AnnotationStrategies
{
    /// <summary>
    /// Реализация стратегии <see cref="IDetalAnnotationStrategy"/> класса <see cref="ForRobot.Model.Detals.Plita"/>
    /// </summary>
    public class PlateAnnotationStrategy : IDetalAnnotationStrategy
    {
        /// <summary>
        /// Масштабный коэффициент
        /// </summary>
        private readonly double _scaleFactor;

        public PlateAnnotationStrategy(double scaleFactor)
        {
            this._scaleFactor = scaleFactor;
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

        private Annotation GetPlateLengthAnnotation(Plita plate)
        {
            double modelPlateWidth = (double)plate.PlateWidth * (double)this._scaleFactor;

            double halfLength = (double)plate.PlateLength * (double)this._scaleFactor / 2;
            double halfWidth = modelPlateWidth / 2;

            double offsetX = 0;
            switch (plate.ScoseType)
            {
                case ScoseTypes.SlopeLeft:
                case ScoseTypes.SlopeRight:
                    offsetX = (plate.ScoseType == ScoseTypes.SlopeLeft) ? -ModelingService.SLOPE_OFF_SET : ModelingService.SLOPE_OFF_SET;
                    break;
            }

            Point3D A = this.CreatePoint(-halfLength + offsetX, halfWidth, 0);
            Point3D B = this.CreatePoint(-halfLength + offsetX, halfWidth + Annotation.DefaultAnnotationWidth, 0);
            Point3D C = this.CreatePoint(halfLength + offsetX, halfWidth + Annotation.DefaultAnnotationWidth, 0);
            Point3D D = this.CreatePoint(halfLength + offsetX, halfWidth, 0);

            Point3DCollection points = new Point3DCollection() { A, B, C, D };

            return new Annotation(points)
            {
                Text = ToString(plate.PlateLength),
                PropertyName = nameof(plate.PlateLength),
            };
        }

        private Annotation GetPlateWidthAnnotation(Plita plate)
        {
            double halfLength = (double)plate.PlateLength * (double)this._scaleFactor / 2;
            double halfWidth = (double)plate.PlateWidth * (double)this._scaleFactor / 2;

            //double bottomHalfLen = halfLength, topHalfLen = halfLength;
            double offsetX = 0;
            switch (plate.ScoseType)
            {
                case ScoseTypes.SlopeLeft:
                case ScoseTypes.SlopeRight:
                    offsetX = -ModelingService.SLOPE_OFF_SET;
                    break;

                    //case ScoseTypes.TrapezoidTop:
                    //    bottomHalfLen = halfLength;
                    //    topHalfLen = halfLength * (1 - ModelingService.TrapezoidRatio);
                    //    break;

                    //case ScoseTypes.TrapezoidBottom:
                    //    bottomHalfLen = halfLength * (1 - ModelingService.TrapezoidRatio);
                    //    topHalfLen = halfLength;
                    //    break;
            }

            Point3D A = this.CreatePoint(-halfLength + offsetX, -halfWidth, 0);
            Point3D B = this.CreatePoint(-halfLength + offsetX - Annotation.DefaultAnnotationWidth, -halfWidth, 0);
            Point3D C = this.CreatePoint(-halfLength + offsetX - Annotation.DefaultAnnotationWidth, halfWidth, 0);
            Point3D D = this.CreatePoint(-halfLength + offsetX, halfWidth, 0);

            Point3DCollection points = new Point3DCollection() { A, B, C, D };

            return new Annotation(points)
            {
                Text = ToString(plate.PlateWidth),
                PropertyName = nameof(plate.PlateWidth)
            };
        }

        private Annotation GetBevelToLeftAnnotation(Plita plate)
        {
            double halfLength = (double)plate.PlateLength * (double)this._scaleFactor / 2;
            double halfWidth = (double)plate.PlateWidth * (double)this._scaleFactor / 2;

            Point3D A = new Point3D();
            Point3D B = new Point3D();
            Point3D C = new Point3D();
            Point3D D = new Point3D();

            Annotation.ArrowSide arrowSide = Annotation.ArrowSide.BC;

            switch (plate.ScoseType)
            {
                case ScoseTypes.Rect:
                    return null;

                case ScoseTypes.SlopeLeft:
                    double offsetX = -ModelingService.SLOPE_OFF_SET;
                    A = this.CreatePoint(-halfLength - offsetX, -halfWidth, 0);
                    B = this.CreatePoint(-halfLength + offsetX, -halfWidth, 0);
                    C = this.CreatePoint(-halfLength + offsetX, halfWidth, 0);
                    D = this.CreatePoint(-halfLength + offsetX, halfWidth, 0);
                    arrowSide = Annotation.ArrowSide.AB;
                    break;

                case ScoseTypes.SlopeRight:
                    offsetX = ModelingService.SLOPE_OFF_SET;
                    A = this.CreatePoint(-halfLength - offsetX, -halfWidth, 0);
                    B = this.CreatePoint(-halfLength - offsetX, -halfWidth, 0);
                    C = this.CreatePoint(-halfLength - offsetX, halfWidth, 0);
                    D = this.CreatePoint(-halfLength + offsetX, halfWidth, 0);
                    arrowSide = Annotation.ArrowSide.CD;
                    break;

                case ScoseTypes.TrapezoidTop:
                case ScoseTypes.TrapezoidBottom:
                    double maxXPoint = halfLength * (1 - ModelingService.TRAPEZOID_RATIO);
                    double isTop = (plate.ScoseType == ScoseTypes.TrapezoidTop) ? 1 : -1;

                    A = this.CreatePoint(-halfLength, -halfWidth * isTop, 0);
                    B = this.CreatePoint(-halfLength, -halfWidth * isTop, 0);
                    C = this.CreatePoint(-halfLength, halfWidth * isTop, 0);
                    D = this.CreatePoint(-maxXPoint, halfWidth * isTop, 0);
                    arrowSide = Annotation.ArrowSide.CD;
                    break;
            }

            Point3DCollection points = new Point3DCollection() { A, B, C, D };

            return new Annotation(points, arrowSide)
            {
                Text = this.ToString(plate.BevelToLeft),
                PropertyName = nameof(plate.BevelToLeft)
            };
        }

        private Annotation GetBevelToRightAnnotation(Plita plate)
        {
            double halfLength = (double)plate.PlateLength * (double)this._scaleFactor / 2;
            double halfWidth = (double)plate.PlateWidth * (double)this._scaleFactor / 2;

            Point3D A = new Point3D();
            Point3D B = new Point3D();
            Point3D C = new Point3D();
            Point3D D = new Point3D();

            Annotation.ArrowSide arrowSide = Annotation.ArrowSide.BC;

            switch (plate.ScoseType)
            {
                case ScoseTypes.Rect:
                    return null;

                case ScoseTypes.SlopeLeft:
                    double offsetX = -ModelingService.SLOPE_OFF_SET;
                    A = this.CreatePoint(halfLength - offsetX, -halfWidth, 0);
                    B = this.CreatePoint(halfLength - offsetX, -halfWidth, 0);
                    C = this.CreatePoint(halfLength - offsetX, halfWidth, 0);
                    D = this.CreatePoint(halfLength + offsetX, halfWidth, 0);
                    arrowSide = Annotation.ArrowSide.CD;
                    break;

                case ScoseTypes.SlopeRight:
                    offsetX = ModelingService.SLOPE_OFF_SET;
                    A = this.CreatePoint(halfLength - offsetX, -halfWidth, 0);
                    B = this.CreatePoint(halfLength + offsetX, -halfWidth, 0);
                    C = this.CreatePoint(halfLength + offsetX, halfWidth, 0);
                    D = this.CreatePoint(halfLength + offsetX, halfWidth, 0);
                    arrowSide = Annotation.ArrowSide.AB;
                    break;

                case ScoseTypes.TrapezoidTop:
                case ScoseTypes.TrapezoidBottom:
                    double maxXPoint = halfLength * (1 - ModelingService.TRAPEZOID_RATIO);
                    double isTop = (plate.ScoseType == ScoseTypes.TrapezoidTop) ? 1 : -1;

                    A = this.CreatePoint(halfLength, -halfWidth * isTop, 0);
                    B = this.CreatePoint(halfLength, -halfWidth * isTop, 0);
                    C = this.CreatePoint(halfLength, halfWidth * isTop, 0);
                    D = this.CreatePoint(maxXPoint, halfWidth * isTop, 0);
                    arrowSide = Annotation.ArrowSide.CD;
                    break;
            }

            Point3DCollection points = new Point3DCollection() { A, B, C, D };

            return new Annotation(points, arrowSide)
            {
                Text = this.ToString(plate.BevelToRight),
                PropertyName = nameof(plate.BevelToRight)
            };
        }

        private List<Annotation> GetRibsDistancseAnnotation(Plita plate)
        {
            List<Annotation> annotations = new List<Annotation>();
            double modelPlateLength = (double)plate.PlateLength * (double)this._scaleFactor;
            double modelPlateWidth = (double)plate.PlateWidth * (double)this._scaleFactor;
            double modelPlateHeight = (double)plate.PlateThickness * (double)this._scaleFactor;
            double modelRibThickness = (double)plate.RibThickness * (double)this._scaleFactor;

            double ribLeftPositionY = -modelPlateWidth / 2; // Начальная позиция по Y
            double ribRightPositionY = -modelPlateWidth / 2;
            for (int i = 0; i < plate.RibCount; i++)
            {
                var rib = plate.RibsCollection[i];
                double modelRibDistanceLeft = (double)rib.DistanceLeft * (double)this._scaleFactor;
                double modelRibDistanceRight = (double)rib.DistanceRight * (double)this._scaleFactor;
                double modelRibIdentToLeft = (double)rib.IdentToLeft * (double)this._scaleFactor;
                double modelRibIdentToRight = (double)rib.IdentToRight * (double)this._scaleFactor;

                //ribLeftPositionY += modelRibDistanceLeft;
                //ribRightPositionY += modelRibDistanceLeft;

                double offsetX = 0;
                double centerY = (ribLeftPositionY + ribRightPositionY) / 2 + modelRibThickness / 2;
                double basePlateLength = modelPlateLength;
                switch (plate.ScoseType)
                {
                    case ScoseTypes.SlopeLeft:
                    case ScoseTypes.SlopeRight:
                        double positionRatio = ((ribLeftPositionY + ribRightPositionY) / 2 + modelPlateWidth / 2) / modelPlateWidth * 2 - 1;
                        offsetX = ((plate.ScoseType == ScoseTypes.SlopeLeft) ? -ModelingService.SLOPE_OFF_SET : ModelingService.SLOPE_OFF_SET) * positionRatio;
                        break;

                    case ScoseTypes.TrapezoidTop:
                    case ScoseTypes.TrapezoidBottom:
                        double trapezoidPositionRatio = (centerY + modelPlateWidth / 2) / modelPlateWidth;

                        if (plate.ScoseType == ScoseTypes.TrapezoidTop)
                            basePlateLength = modelPlateLength * (1 - ModelingService.TRAPEZOID_RATIO * trapezoidPositionRatio);
                        else
                            basePlateLength = modelPlateLength * (1 - ModelingService.TRAPEZOID_RATIO * (1 - trapezoidPositionRatio));
                        break;
                }
                // Длина ребра с учетом отступов и формы плиты
                double ribLength = basePlateLength - modelRibIdentToLeft - modelRibIdentToRight;
                ribLength = Math.Max(ribLength, ModelingService.MIN_RIB_LENGTH);

                // Центр ребра по X с учетом смещения
                double centerX = offsetX + (modelRibIdentToLeft - modelRibIdentToRight) / 2;

                double halfRibLength = ribLength / 2;

                Point3D A = new Point3D(-halfRibLength + centerX, ribLeftPositionY, modelPlateHeight);
                Point3D B = new Point3D(-halfRibLength + centerX, ribLeftPositionY, modelPlateHeight);
                ribLeftPositionY += modelRibDistanceLeft;
                if (i > 0)
                    ribLeftPositionY -= modelRibThickness;
                Point3D C = new Point3D(-halfRibLength + centerX, ribLeftPositionY, modelPlateHeight);
                Point3D D = new Point3D(-halfRibLength + centerX, ribLeftPositionY, modelPlateHeight);

                Annotation annotation1 = new Annotation(new Point3DCollection() { A, B, C, D }, Annotation.ArrowSide.DA)
                {
                    Text = this.ToString(rib.DistanceLeft),
                    PropertyName = string.Format("{0} {1}", nameof(rib.DistanceLeft), i),
                    ArrowSize = 0.5
                };
                annotations.Add(annotation1);

                A = new Point3D(halfRibLength + centerX, ribRightPositionY, modelPlateHeight);
                B = new Point3D(halfRibLength + centerX, ribRightPositionY, modelPlateHeight);
                ribRightPositionY += modelRibDistanceRight;
                if (i > 0)
                    ribRightPositionY -= modelRibThickness;
                C = new Point3D(halfRibLength + centerX, ribRightPositionY, modelPlateHeight);
                D = new Point3D(halfRibLength + centerX, ribRightPositionY, modelPlateHeight);

                Annotation annotation2 = new Annotation(new Point3DCollection() { A, B, C, D }, Annotation.ArrowSide.DA)
                {
                    Text = this.ToString(rib.DistanceRight),
                    PropertyName = string.Format("{0} {1}", nameof(rib.DistanceRight), i),
                    ArrowSize = 0.5
                };
                annotations.Add(annotation2);

                ribLeftPositionY += modelRibThickness;
                ribRightPositionY += modelRibThickness;
            }
            return annotations;
        }

        #endregion Private functions

        public bool CanHandle(DetalType detalType) => detalType == DetalType.Plita;

        public ObservableCollection<Annotation> CreateAnnotations(Detal detal)
        {
            Plita plate = (Plita)detal;
            List<Annotation> annotationsList = new List<Annotation>()
            {
                this.GetPlateLengthAnnotation(plate),
                this.GetPlateWidthAnnotation(plate),
                //this.GetPlateThicknessAnnotation(plate),
                this.GetBevelToLeftAnnotation(plate),
                this.GetBevelToRightAnnotation(plate)

                //this.GetRibThicknessAnnotation(plate),
                //this.GetTechOffsetSeamStartAnnotation(plate),
                //this.GetTechOffsetSeamEndAnnotation(plate),
            };

            foreach (var item in this.GetRibsDistancseAnnotation(plate))
                annotationsList.Add(item);

            return new ObservableCollection<Annotation>(annotationsList);
        }

        public string ToString(decimal param) => string.Format("{0} mm", param);
        //public string ToString(object param) => param.ToString();
    }
}

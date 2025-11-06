using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;

using ForRobot.Libr.Attributes;
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
        /// <summary>
        /// Коэффициент сужения/расширения для трапеций (от 0.1 до 0.9)
        /// </summary>
        private readonly double _trapezoidRatio;
        /// <summary>
        /// Смещение скоса
        /// </summary>
        private readonly double _slopeOffset;

        public PlateAnnotationStrategy(double scaleFactor, double slopeOffset = ModelingService.SLOPE_OFF_SET, double trapezoidRatio = ModelingService.TRAPEZOID_RATIO)
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

        [PropertyName(nameof(Plita.PlateLength))]
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

        [PropertyName(nameof(Plita.PlateWidth))]
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

        [PropertyName(nameof(Plita.BevelToLeft))]
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

        [PropertyName(nameof(Plita.BevelToRight))]
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

        [PropertyName(nameof(Rib.DistanceLeft))]
        private List<Annotation> GetRibsDistancsesAnnotation(Plita plate)
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
                        double trapezoidPositionRatio = (centerY + modelRibDistanceLeft / 2 + modelRibDistanceRight / 2 + modelPlateWidth / 2) / modelPlateWidth;

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

                Point3D A = this.CreatePoint(-halfRibLength + centerX, ribLeftPositionY, modelPlateHeight);
                Point3D B = this.CreatePoint(-halfRibLength + centerX, ribLeftPositionY, modelPlateHeight);
                ribLeftPositionY += modelRibDistanceLeft;
                if (i > 0)
                    ribLeftPositionY -= modelRibThickness;
                Point3D C = this.CreatePoint(-halfRibLength + centerX, ribLeftPositionY, modelPlateHeight);
                Point3D D = this.CreatePoint(-halfRibLength + centerX, ribLeftPositionY, modelPlateHeight);

                Annotation annotation1 = new Annotation(new Point3DCollection() { A, B, C, D }, Annotation.ArrowSide.DA)
                {
                    Text = this.ToString(rib.DistanceLeft),
                    PropertyName = string.Format("{0} {1}", nameof(rib.DistanceLeft), i),
                    ArrowSize = 0.5
                };
                annotations.Add(annotation1);

                A = this.CreatePoint(halfRibLength + centerX, ribRightPositionY, modelPlateHeight);
                B = this.CreatePoint(halfRibLength + centerX, ribRightPositionY, modelPlateHeight);
                ribRightPositionY += modelRibDistanceRight;
                if (i > 0)
                    ribRightPositionY -= modelRibThickness;
                C = this.CreatePoint(halfRibLength + centerX, ribRightPositionY, modelPlateHeight);
                D = this.CreatePoint(halfRibLength + centerX, ribRightPositionY, modelPlateHeight);

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

        [PropertyName(nameof(Rib.IdentToLeft))]
        private List<Annotation> GetRibsIdentsAnnotation(Plita plate)
        {
            List<Annotation> annotations = new List<Annotation>();
            double halfModelPlateLength = (double)plate.PlateLength * (double)this._scaleFactor / 2;
            double modelPlateWidth = (double)plate.PlateWidth * (double)this._scaleFactor;
            double modelPlateHeight = (double)plate.PlateThickness * (double)this._scaleFactor;
            double modelRibThickness = (double)plate.RibThickness * (double)this._scaleFactor;

            double ribLeftPositionY = -modelPlateWidth / 2;
            double ribRightPositionY = -modelPlateWidth / 2;
            for (int i = 0; i < plate.RibCount; i++)
            {
                var rib = plate.RibsCollection[i];
                double modelRibDistanceLeft = (double)rib.DistanceLeft * (double)this._scaleFactor;
                double modelRibDistanceRight = (double)rib.DistanceRight * (double)this._scaleFactor;
                double modelRibIdentToLeft = (double)rib.IdentToLeft * (double)this._scaleFactor;
                double modelRibIdentToRight = (double)rib.IdentToRight * (double)this._scaleFactor;

                ribLeftPositionY += modelRibDistanceLeft;
                ribRightPositionY += modelRibDistanceRight;

                double offsetX = 0;
                double basePlateLength = halfModelPlateLength;
                switch (plate.ScoseType)
                {
                    case ScoseTypes.SlopeLeft:
                    case ScoseTypes.SlopeRight:
                        double positionRatio = ((ribLeftPositionY + ribRightPositionY) / 2 + modelPlateWidth / 2) / modelPlateWidth * 2 - 1;
                        offsetX = ((plate.ScoseType == ScoseTypes.SlopeLeft) ? -this._slopeOffset : this._slopeOffset) * positionRatio;
                        break;

                    case ScoseTypes.TrapezoidTop:
                    case ScoseTypes.TrapezoidBottom:
                        double centerY = (ribLeftPositionY + ribRightPositionY) / 2 + modelRibThickness / 2;
                        double trapezoidPositionRatio = (centerY + modelPlateWidth / 2) / modelPlateWidth;

                        if (plate.ScoseType == ScoseTypes.TrapezoidTop)
                            basePlateLength *= (1 - this._trapezoidRatio * trapezoidPositionRatio);
                        else
                            basePlateLength *= (1 - this._trapezoidRatio * (1 - trapezoidPositionRatio));
                        break;
                }

                double centerX = offsetX + (modelRibIdentToLeft - modelRibIdentToRight) / 2;

                Point3D A = this.CreatePoint(-basePlateLength + centerX, ribLeftPositionY, modelPlateHeight);
                Point3D B = this.CreatePoint(-basePlateLength + centerX, ribLeftPositionY, modelPlateHeight);
                Point3D C = this.CreatePoint(-basePlateLength + modelRibIdentToLeft + centerX, ribLeftPositionY, modelPlateHeight);
                Point3D D = this.CreatePoint(-basePlateLength + modelRibIdentToLeft + centerX, ribLeftPositionY, modelPlateHeight);

                Annotation annotation1 = new Annotation(new Point3DCollection() { A, B, C, D })
                {
                    Text = this.ToString(rib.IdentToLeft),
                    PropertyName = string.Format("{0} {1}", nameof(rib.IdentToLeft), i),
                    ArrowSize = 0.5
                };
                annotations.Add(annotation1);

                A = this.CreatePoint(basePlateLength + centerX, ribRightPositionY, modelPlateHeight);
                B = this.CreatePoint(basePlateLength + centerX, ribRightPositionY, modelPlateHeight);
                C = this.CreatePoint(basePlateLength - modelRibIdentToRight + centerX, ribRightPositionY, modelPlateHeight);
                D = this.CreatePoint(basePlateLength - modelRibIdentToRight + centerX, ribRightPositionY, modelPlateHeight);

                Annotation annotation2 = new Annotation(new Point3DCollection() { A, B, C, D })
                {
                    Text = this.ToString(rib.IdentToRight),
                    PropertyName = string.Format("{0} {1}", nameof(rib.IdentToRight), i),
                    ArrowSize = 0.5
                };
                annotations.Add(annotation2);
            }
            return annotations;
        }

        [PropertyName(nameof(Rib.DissolutionLeft))]
        private List<Annotation> GetRibsDissolutionsAnnotation(Plita plate)
        {
            List<Annotation> annotations = new List<Annotation>();
            double halfModelPlateLength = (double)plate.PlateLength * (double)this._scaleFactor / 2;
            double modelPlateWidth = (double)plate.PlateWidth * (double)this._scaleFactor;
            double modelPlateHeight = (double)plate.PlateThickness * (double)this._scaleFactor;
            double modelRibThickness = (double)plate.RibThickness * (double)this._scaleFactor;

            double ribLeftPositionY = -modelPlateWidth / 2;
            double ribRightPositionY = -modelPlateWidth / 2;
            for (int i = 0; i < plate.RibCount; i++)
            {
                var rib = plate.RibsCollection[i];
                double modelRibDistanceLeft = (double)rib.DistanceLeft * (double)this._scaleFactor;
                double modelRibDistanceRight = (double)rib.DistanceRight * (double)this._scaleFactor;
                double modelRibIdentToLeft = (double)rib.IdentToLeft * (double)this._scaleFactor;
                double modelRibIdentToRight = (double)rib.IdentToRight * (double)this._scaleFactor;
                double modelRibDissolutionLeft = (double)rib.DissolutionLeft * (double)this._scaleFactor;
                double modelRibDissolutionRight = (double)rib.DissolutionRight * (double)this._scaleFactor;

                ribLeftPositionY += modelRibDistanceLeft;
                ribRightPositionY += modelRibDistanceRight;

                double offsetX = 0;
                double basePlateLength = halfModelPlateLength;
                switch (plate.ScoseType)
                {
                    case ScoseTypes.SlopeLeft:
                    case ScoseTypes.SlopeRight:
                        double positionRatio = ((ribLeftPositionY + ribRightPositionY) / 2 + modelPlateWidth / 2) / modelPlateWidth * 2 - 1;
                        offsetX = ((plate.ScoseType == ScoseTypes.SlopeLeft) ? -this._slopeOffset : this._slopeOffset) * positionRatio;
                        break;

                    case ScoseTypes.TrapezoidTop:
                    case ScoseTypes.TrapezoidBottom:
                        double centerY = (ribLeftPositionY + ribRightPositionY) / 2 + modelRibThickness / 2;
                        double trapezoidPositionRatio = (centerY + modelPlateWidth / 2) / modelPlateWidth;

                        if (plate.ScoseType == ScoseTypes.TrapezoidTop)
                            basePlateLength *= (1 - this._trapezoidRatio * trapezoidPositionRatio);
                        else
                            basePlateLength *= (1 - this._trapezoidRatio * (1 - trapezoidPositionRatio));
                        break;
                }

                double centerX = offsetX + (modelRibIdentToLeft - modelRibIdentToRight) / 2;

                Point3D A = this.CreatePoint(-basePlateLength + modelRibIdentToLeft + centerX, ribLeftPositionY, modelPlateHeight);
                Point3D B = this.CreatePoint(-basePlateLength + modelRibIdentToLeft + centerX, ribLeftPositionY, modelPlateHeight);
                Point3D C = this.CreatePoint(-basePlateLength + modelRibIdentToLeft + modelRibDissolutionLeft + centerX, ribLeftPositionY, modelPlateHeight);
                Point3D D = this.CreatePoint(-basePlateLength + modelRibIdentToLeft + modelRibDissolutionLeft + centerX, ribLeftPositionY, modelPlateHeight);

                Annotation annotation1 = new Annotation(new Point3DCollection() { A, B, C, D })
                {
                    Text = this.ToString(rib.DissolutionLeft),
                    PropertyName = string.Format("{0} {1}", nameof(rib.DissolutionLeft), i),
                    ArrowSize = 0.5
                };
                annotations.Add(annotation1);

                A = this.CreatePoint(basePlateLength - modelRibIdentToRight + centerX, ribRightPositionY, modelPlateHeight);
                B = this.CreatePoint(basePlateLength - modelRibIdentToRight + centerX, ribRightPositionY, modelPlateHeight);
                C = this.CreatePoint(basePlateLength - modelRibIdentToRight - modelRibDissolutionRight + centerX, ribRightPositionY, modelPlateHeight);
                D = this.CreatePoint(basePlateLength - modelRibIdentToRight - modelRibDissolutionRight + centerX, ribRightPositionY, modelPlateHeight);

                Annotation annotation2 = new Annotation(new Point3DCollection() { A, B, C, D })
                {
                    Text = this.ToString(rib.DissolutionRight),
                    PropertyName = string.Format("{0} {1}", nameof(rib.DissolutionRight), i),
                    ArrowSize = 0.5
                };
                annotations.Add(annotation2);
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
            };

            foreach (var item in this.GetRibsDistancsesAnnotation(plate))
            {
                item.IsVisible = false;
                annotationsList.Add(item);
            }

            foreach (var item in this.GetRibsIdentsAnnotation(plate))
            {
                item.IsVisible = false;
                annotationsList.Add(item);
            }

            foreach (var item in this.GetRibsDissolutionsAnnotation(plate))
            {
                item.IsVisible = false;
                annotationsList.Add(item);
            }

            return new ObservableCollection<Annotation>(annotationsList);
        }

        public string ToString(decimal param) => string.Format("{0} mm", param);
        //public string ToString(object param) => param.ToString();
    }
}

using System;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ForRobot.Libr.Attributes;
using ForRobot.Model.Detals;
using ForRobot.Model.File3D;
using System.Linq;

namespace ForRobot.Services
{
    public interface IAnnotationService
    {
        ObservableCollection<Annotation> GetAnnotations(Detal detal);

        string ToString(decimal param);
    }

    /// <summary>
    /// Класс сервис для составления коллекции <see cref="Annotation"/> отражающую параметры детали
    /// </summary>
    public sealed class AnnotationService : IAnnotationService
    {
        #region Public variables

        /// <summary>
        /// Масштабный коэффициент: 1 единица модели = <see cref="ScaleFactor"/> мм. реальных размеров
        /// </summary>
        public decimal ScaleFactor { get; set; } = 1.00M / 100.00M;

        #endregion Public variables

        #region Constructor

        public AnnotationService(decimal _scaleFactor)
        {
            this.ScaleFactor = _scaleFactor;
        }

        #endregion

        #region Private functions

        private List<Annotation> GetDetalAnnotations(Detal detal)
        {
            List<Annotation> annotations = new List<Annotation>()
            {
                //this.GetPlateLengthAnnotation(detal)
                //this.GetPlateWidthAnnotation(detal)
                //this.GetPlateThicknessAnnotation(detal)
            };
            return annotations;
        }

        private List<Annotation> GetPlateAnnotations(Plita plate)
        {
            List<Annotation> annotations = new List<Annotation>()
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
                annotations.Add(item);
            //annotations.Union(this.GetRibsDistancseAnnotation(plate));
            return annotations;
        }

        #region Detal's properties

        //private Annotation GetRibHeightAnnotation(Detal detal)
        //{
        //    return new Annotation()
        //    {
        //        Position = new Point3D(0, 10, 5),
        //        Text = $"RibHeight: {detal.RibHeight} mm",
        //        PropertyName = nameof(detal.RibHeight)
        //    };
        //}

        //private Annotation GetRibThicknessAnnotation(Detal detal)
        //{
        //    return new Annotation()
        //    {
        //        Position = new Point3D(0, 10, 10),
        //        Text = $"RibThickness: {detal.RibThickness} mm",
        //        PropertyName = nameof(detal.RibThickness)
        //    };
        //}

        //private Annotation GetTechOffsetSeamStartAnnotation(Detal detal)
        //{
        //    return new Annotation()
        //    {
        //        Position = new Point3D(0, 20, 5),
        //        Text = $"TechOffsetSeamStart: {detal.TechOffsetSeamStart} mm",
        //        PropertyName = nameof(detal.TechOffsetSeamStart)
        //    };
        //}

        //private Annotation GetTechOffsetSeamEndAnnotation(Detal detal)
        //{
        //    return new Annotation()
        //    {
        //        Position = new Point3D(0, 25, 5),
        //        Text = $"TechOffsetSeamEnd: {detal.TechOffsetSeamEnd} mm",
        //        PropertyName = nameof(detal.TechOffsetSeamEnd)
        //    };
        //}

        #region Plita

        //[PropertyCameraAttribute(nameof(Plita.PlateLength), new Vector3D (0, -6, -722), new Vector3D(0, -1, 0))]
        private Annotation GetPlateLengthAnnotation(Plita plate)
        {
            double modelPlateWidth = (double)plate.PlateWidth * (double)ScaleFactor;

            double halfLength = (double)plate.PlateLength * (double)ScaleFactor / 2;
            double halfWidth = modelPlateWidth / 2;
            
            double offsetX = 0;
            switch (plate.ScoseType)
            {
                case ScoseTypes.SlopeLeft:
                case ScoseTypes.SlopeRight:
                    offsetX = (plate.ScoseType == ScoseTypes.SlopeLeft) ? -ModelingService.SlopeOffset : ModelingService.SlopeOffset;
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
            double halfLength = (double)plate.PlateLength * (double)ScaleFactor / 2;
            double halfWidth = (double)plate.PlateWidth * (double)ScaleFactor / 2;

            //double bottomHalfLen = halfLength, topHalfLen = halfLength;
            double offsetX = 0;
            switch (plate.ScoseType)
            {
                case ScoseTypes.SlopeLeft:
                case ScoseTypes.SlopeRight:
                    offsetX = -ModelingService.SlopeOffset;
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

        private Annotation GetPlateThicknessAnnotation(Plita plate)
        {
            double halfLength = (double)plate.PlateLength * (double)ScaleFactor / 2;
            double halfWidth = (double)plate.PlateWidth * (double)ScaleFactor / 2;

            Point3D A = new Point3D();
            Point3D B = new Point3D();
            Point3D C = new Point3D();
            Point3D D = new Point3D();

            switch (plate.ScoseType)
            {
                case ScoseTypes.Rect:
                    break;
            }

            Point3DCollection points = new Point3DCollection() { A, B, C, D };
            
            return new Annotation(points)
            {
                Text = ToString(plate.PlateThickness),
                PropertyName = nameof(plate.PlateThickness)
            };
        }

        private Annotation GetBevelToLeftAnnotation(Plita plate)
        {
            double halfLength = (double)plate.PlateLength * (double)ScaleFactor / 2;
            double halfWidth = (double)plate.PlateWidth * (double)ScaleFactor / 2;

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
                    double offsetX = -ModelingService.SlopeOffset;
                    A = this.CreatePoint(-halfLength - offsetX, -halfWidth, 0);
                    B = this.CreatePoint(-halfLength + offsetX, -halfWidth, 0);
                    C = this.CreatePoint(-halfLength + offsetX, halfWidth, 0);
                    D = this.CreatePoint(-halfLength + offsetX, halfWidth, 0);
                    arrowSide = Annotation.ArrowSide.AB;
                    break;

                case ScoseTypes.SlopeRight:
                    offsetX = ModelingService.SlopeOffset;
                    A = this.CreatePoint(-halfLength - offsetX, -halfWidth, 0);
                    B = this.CreatePoint(-halfLength - offsetX, -halfWidth, 0);
                    C = this.CreatePoint(-halfLength - offsetX, halfWidth, 0);
                    D = this.CreatePoint(-halfLength + offsetX, halfWidth, 0);
                    arrowSide = Annotation.ArrowSide.CD;
                    break;
                
                case ScoseTypes.TrapezoidTop:
                case ScoseTypes.TrapezoidBottom:
                    double maxXPoint = halfLength * (1 - ModelingService.TrapezoidRatio);
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
            double halfLength = (double)plate.PlateLength * (double)ScaleFactor / 2;
            double halfWidth = (double)plate.PlateWidth * (double)ScaleFactor / 2;

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
                    double offsetX = -ModelingService.SlopeOffset;
                    A = this.CreatePoint(halfLength - offsetX, -halfWidth, 0);
                    B = this.CreatePoint(halfLength - offsetX, -halfWidth, 0);
                    C = this.CreatePoint(halfLength - offsetX, halfWidth, 0);
                    D = this.CreatePoint(halfLength + offsetX, halfWidth, 0);
                    arrowSide = Annotation.ArrowSide.CD;
                    break;

                case ScoseTypes.SlopeRight:
                    offsetX = ModelingService.SlopeOffset;
                    A = this.CreatePoint(halfLength - offsetX, -halfWidth, 0);
                    B = this.CreatePoint(halfLength + offsetX, -halfWidth, 0);
                    C = this.CreatePoint(halfLength + offsetX, halfWidth, 0);
                    D = this.CreatePoint(halfLength + offsetX, halfWidth, 0);
                    arrowSide = Annotation.ArrowSide.AB;
                    break;

                case ScoseTypes.TrapezoidTop:
                case ScoseTypes.TrapezoidBottom:
                    double maxXPoint = halfLength * (1 - ModelingService.TrapezoidRatio);
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
            double modelPlateLength = (double)plate.PlateLength * (double)ScaleFactor;
            double modelPlateWidth = (double)plate.PlateWidth * (double)ScaleFactor;
            double modelPlateHeight = (double)plate.PlateThickness * (double)ScaleFactor;
            double modelRibThickness = (double)plate.RibThickness * (double)ScaleFactor;

            double ribLeftPositionY = -modelPlateWidth / 2; // Начальная позиция по Y
            double ribRightPositionY = -modelPlateWidth / 2;
            for (int i = 0; i < plate.RibCount; i++)
            {
                var rib = plate.RibsCollection[i];
                double modelRibDistanceLeft = (double)rib.DistanceLeft * (double)ScaleFactor;
                double modelRibDistanceRight = (double)rib.DistanceRight * (double)ScaleFactor;
                double modelRibIdentToLeft = (double)rib.IdentToLeft * (double)ScaleFactor;
                double modelRibIdentToRight = (double)rib.IdentToRight * (double)ScaleFactor;

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
                        offsetX = ((plate.ScoseType == ScoseTypes.SlopeLeft) ? -ModelingService.SlopeOffset : ModelingService.SlopeOffset) * positionRatio;
                        break;

                    case ScoseTypes.TrapezoidTop:
                    case ScoseTypes.TrapezoidBottom:
                        double trapezoidPositionRatio = (centerY + modelPlateWidth / 2) / modelPlateWidth;

                        if (plate.ScoseType == ScoseTypes.TrapezoidTop)
                            basePlateLength = modelPlateLength * (1 - ModelingService.TrapezoidRatio * trapezoidPositionRatio);
                        else
                            basePlateLength = modelPlateLength * (1 - ModelingService.TrapezoidRatio * (1 - trapezoidPositionRatio));
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
                    Text = this.ToString(plate.BevelToLeft),
                    PropertyName = string.Format("{0} {1}", nameof(plate.BevelToLeft), i),
                    ArrowSize = 0.5
                };
                annotations.Add(annotation1);

                //A = new Point3D(halfRibLength + centerX, ribRightPositionY, modelPlateHeight);
                //B = new Point3D(halfRibLength + centerX, ribRightPositionY, modelPlateHeight);
                //if (i > 0)
                //    ribRightPositionY -= modelRibThickness;
                //C = new Point3D(halfRibLength + centerX, ribRightPositionY, modelPlateHeight);
                //D = new Point3D(halfRibLength + centerX, ribRightPositionY, modelPlateHeight);

                //Annotation annotation2 = new Annotation(new Point3DCollection() { A, B, C, D }, Annotation.ArrowSide.DA)
                //{
                //    Text = this.ToString(plate.BevelToRight),
                //    PropertyName = string.Format("{0} {1}", nameof(plate.BevelToRight), i),
                //    ArrowSize = 0.5
                //};
                //annotations.Add(annotation2);

                ribLeftPositionY += modelRibThickness;
                ribRightPositionY += modelRibThickness;
            }
            return annotations;
        }

        #endregion Plita

        #endregion Properties

        // Создание вершины с точным округлением
        private Point3D CreatePoint(double x, double y, double z) => new Point3D(Math.Round(x, 4), Math.Round(y, 4), Math.Round(z, 4));

        #endregion Private functions

        #region Public functions

        public ObservableCollection<Annotation> GetAnnotations(Detal detal)
        {
            List<Annotation> annotationsList = this.GetDetalAnnotations(detal);

            switch (detal.DetalType)
            {
                case DetalTypes.Plita:
                    annotationsList.AddRange(this.GetPlateAnnotations(detal as Plita));
                    break;

                default:
                    return null;
            }
            return new ObservableCollection<Annotation>(annotationsList);
        }

        public string ToString(decimal param) => string.Format("{0} mm", param);
        //public string ToString(object param) => param.ToString();

        #endregion
    }
}

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
                this.GetPlateLengthAnnotation(plate)
                //this.GetPlateWidthAnnotation(plate)
                //this.GetBevelToLeftAnnotation(plate)

                //this.GetRibHeightAnnotation(plate),
                //this.GetRibThicknessAnnotation(plate),
                //this.GetTechOffsetSeamStartAnnotation(plate),
                //this.GetTechOffsetSeamEndAnnotation(plate),
                //this.GetBevelToRightAnnotation(plate)
            };
            return annotations;
        }

        #region Properties

        private Annotation GetPlateThicknessAnnotation(Detal detal)
        {
            double modelPlateWidth = (double)detal.PlateWidth * (double)ScaleFactor;
            double modelPlateLength = (double)detal.PlateLength * (double)ScaleFactor;
            double modelPlateHeight = (double)detal.PlateThickness * (double)ScaleFactor;

            Point3DCollection points = new Point3DCollection(new List<Point3D>()
            {
                new Point3D(-modelPlateLength / 2,
                            modelPlateWidth / 2,
                            -modelPlateHeight / 2),

                new Point3D(-modelPlateLength / 2,
                            modelPlateWidth / 2,
                            modelPlateHeight / 2),

                new Point3D(-modelPlateLength / 2  + Annotation.DefaultAnnotationWidth,
                            modelPlateWidth,
                            modelPlateHeight / 2),

                new Point3D(-modelPlateLength / 2 + Annotation.DefaultAnnotationWidth,
                            modelPlateWidth / 2,
                            -modelPlateHeight / 2),
            });

            return new Annotation(points)
            {
                Text = ToString(detal.PlateThickness),
                PropertyName = nameof(detal.PlateThickness)
            };
        }

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

            //double leftOffsetDirection = 0;
            //double rightOffsetDirection = 0;
            double offsetX = 0;
            switch (plate.ScoseType)
            {
                case ScoseTypes.SlopeLeft:
                    offsetX = (plate.ScoseType == ScoseTypes.SlopeLeft) ? -ModelingService.SlopeOffset : ModelingService.SlopeOffset;
                    break;

                case ScoseTypes.SlopeRight:
                    //rightOffsetDirection = ModelingService.SlopeOffset;
                    break;
            }

            Point3D A = this.CreatePoint(-halfLength + offsetX, halfWidth, 0);
            Point3D B = this.CreatePoint(-halfLength + offsetX, halfWidth + Annotation.DefaultAnnotationWidth, 0);
            Point3D C = this.CreatePoint(halfLength + offsetX, halfWidth + Annotation.DefaultAnnotationWidth, 0);
            Point3D D = this.CreatePoint(halfLength + offsetX, halfWidth, 0);

            Point3DCollection points = new Point3DCollection() { A, B, C, D };

            //if (plate.ScoseType == ScoseTypes.TrapezoidTop)
            //{
            //    for(int i = 0; i < points.Count; i++)
            //    {
            //        Point3D point = points[i];
            //        point.Y -= modelPlateWidth;
            //    }

            //    //ModelingService.RotateVerticesAroundX(points.ToArray(), 180);
            //}
            
            //ModelingService.RotateVerticesAroundX(points.ToArray(), 180);
            
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

            double bottomHalfLen = halfLength, topHalfLen = halfLength;
            double offsetDirection = 0;
            switch (plate.ScoseType)
            {
                case ScoseTypes.SlopeLeft:
                case ScoseTypes.SlopeRight:
                    offsetDirection = (plate.ScoseType == ScoseTypes.SlopeLeft) ? -ModelingService.SlopeOffset : ModelingService.SlopeOffset;
                    break;

                case ScoseTypes.TrapezoidTop:
                    bottomHalfLen = halfLength;
                    topHalfLen = halfLength * (1 - ModelingService.TrapezoidRatio);
                    break;

                case ScoseTypes.TrapezoidBottom:
                    bottomHalfLen = halfLength * (1 - ModelingService.TrapezoidRatio);
                    topHalfLen = halfLength;
                    break;
            }

            Point3D A = this.CreatePoint(-bottomHalfLen, -halfWidth, 0);
            Point3D B = this.CreatePoint(-bottomHalfLen - Annotation.DefaultAnnotationWidth, -halfWidth, 0);
            Point3D C = this.CreatePoint(-topHalfLen - Annotation.DefaultAnnotationWidth, halfWidth, 0);
            Point3D D = this.CreatePoint(-topHalfLen, halfWidth, 0);

            Point3DCollection points = new Point3DCollection() { A, B, C, D };
            
            return new Annotation(points)
            {
                Text = ToString(plate.PlateWidth),
                PropertyName = nameof(plate.PlateWidth)
            };
        }

        private Annotation GetBevelToLeftAnnotation(Plita plate)
        {
            Point3D A = new Point3D();
            Point3D B = new Point3D();
            Point3D C = new Point3D();
            Point3D D = new Point3D();

            switch (plate.ScoseType)
            {
                case ScoseTypes.Rect:
                    return null;

                case ScoseTypes.SlopeLeft:
                    break;

                case ScoseTypes.SlopeRight:
                    break;
                
                case ScoseTypes.TrapezoidTop:
                    break;

                case ScoseTypes.TrapezoidBottom:
                    break;
            }

            Point3DCollection points = new Point3DCollection() { A, B, C, D };

            return new Annotation(points)
            {
                Text = $"BevelToLeft: {plate.BevelToLeft} mm",
                PropertyName = nameof(plate.BevelToLeft)
            };
        }

        private Annotation GetBevelToRightAnnotation(Plita plate)
        {
            Point3D A = new Point3D();
            Point3D B = new Point3D();
            Point3D C = new Point3D();
            Point3D D = new Point3D();

            switch (plate.ScoseType)
            {
                case ScoseTypes.Rect:
                    return null;

                case ScoseTypes.SlopeLeft:
                    break;

                case ScoseTypes.SlopeRight:
                    break;

                case ScoseTypes.TrapezoidTop:
                    break;

                case ScoseTypes.TrapezoidBottom:
                    break;
            }

            Point3DCollection points = new Point3DCollection() { A, B, C, D };

            return new Annotation(points)
            {
                Text = $"BevelToRight: {plate.BevelToRight} mm",
                PropertyName = nameof(plate.BevelToRight)
            };
        }

        //private List<Annotation> Get

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

        #endregion
    }
}

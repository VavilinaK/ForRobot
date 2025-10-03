using System;
using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ForRobot.Model.Detals;
using ForRobot.Model.File3D;

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

                //this.GetRibHeightAnnotation(plate),
                //this.GetRibThicknessAnnotation(plate),
                //this.GetTechOffsetSeamStartAnnotation(plate),
                //this.GetTechOffsetSeamEndAnnotation(plate),
                //this.GetBevelToLeftAnnotation(plate),
                //this.GetBevelToRightAnnotation(plate)
            };
            return annotations;
        }

        #region Properties

        private Annotation GetPlateWidthAnnotation(Detal detal)
        {
            double halfLength = (double)detal.PlateLength * (double)ScaleFactor / 2;
            double halfWidth = (double)detal.PlateWidth * (double)ScaleFactor / 2;

            Point3DCollection points = new Point3DCollection(new List<Point3D>()
            {
                new Point3D(-halfLength, halfWidth, 0),
                new Point3D(-halfLength, -halfWidth, 0),
                new Point3D(-(halfLength + Annotation.DefaultAnnotationWidth), -halfWidth , 0),
                new Point3D(-(halfLength + Annotation.DefaultAnnotationWidth), halfWidth, 0)
            });

            return new Annotation(points)
            {
                Text = ToString(detal.PlateWidth),
                PropertyName = nameof(detal.PlateWidth)
            };
        }

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

        private Annotation GetPlateLengthAnnotation(Plita plate)
        {
            double halfLength = (double)plate.PlateLength * (double)ScaleFactor / 2;
            double halfWidth = (double)plate.PlateWidth * (double)ScaleFactor / 2;

            double offsetDirection = 0;
            switch (plate.ScoseType)
            {
                case ScoseTypes.SlopeLeft:
                case ScoseTypes.SlopeRight:
                    offsetDirection = (plate.ScoseType == ScoseTypes.SlopeLeft) ? -ModelingService.SlopeOffset : ModelingService.SlopeOffset;
                    break;
            }

            Point3D A = this.CreatePoint(halfLength, -halfWidth + offsetDirection, 0);
            Point3D B = this.CreatePoint(halfLength, -(halfWidth + Annotation.DefaultAnnotationWidth), 0);
            Point3D C = this.CreatePoint(-halfLength, -halfWidth - offsetDirection, 0);
            Point3D D = this.CreatePoint(-halfLength, -(halfWidth + Annotation.DefaultAnnotationWidth), 0);

            Point3DCollection points = new Point3DCollection() { A, B, C, D };
            //Point3DCollection points = new Point3DCollection(new List<Point3D>()
            //{
            //    new Point3D(-halfLength, -halfWidth, 0),
            //    new Point3D(halfLength, -halfWidth, 0),
            //    new Point3D(halfLength, -(halfWidth + Annotation.DefaultAnnotationWidth), 0),
            //    new Point3D(-halfLength, -(halfWidth + Annotation.DefaultAnnotationWidth), 0)
            //});

            return new Annotation(points, Annotation.ArrowSide.BC)
            {
                Text = ToString(plate.PlateLength),
                PropertyName = nameof(plate.PlateLength),
            };
        }

        //private Annotation GetBevelToLeftAnnotation(Plita plate)
        //{
        //    return new Annotation()
        //    {
        //        Position = new Point3D(10, 10, 5),
        //        Text = $"BevelToLeft: {plate.BevelToLeft} mm",
        //        PropertyName = nameof(plate.BevelToLeft)
        //    };
        //}

        //private Annotation GetBevelToRightAnnotation(Plita plate)
        //{
        //    return new Annotation()
        //    {
        //        Position = new Point3D(10, 15, 5),
        //        Text = $"BevelToRight: {plate.BevelToRight} mm",
        //        PropertyName = nameof(plate.BevelToRight)
        //    };
        //}

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

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
    }

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
                this.GetPlateLengthAnnotation(detal),
                //this.GetPlateWidthAnnotation(detal)
                //this.GetPlateThicknessAnnotation(detal),
            };
            return annotations;
        }

        private List<Annotation> PlateAnnotations(Plita plate)
        {
            List<Annotation> annotations = new List<Annotation>()
            {
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

        private Annotation GetPlateLengthAnnotation(Detal detal)
        {
            double modelPlateWidth = (double)detal.PlateWidth * (double)ScaleFactor;
            double modelPlateLength = (double)detal.PlateLength * (double)ScaleFactor;

            Point3DCollection points = new Point3DCollection(new List<Point3D>()
            {
                new Point3D(modelPlateLength / 2,
                            0,
                            -modelPlateWidth / 2),

                new Point3D(-modelPlateLength / 2,
                            0,
                            -modelPlateWidth / 2),

                new Point3D(-modelPlateLength / 2,
                            0,
                            -modelPlateWidth/2 - Annotation.DefaultAnnotationWidth),

                new Point3D(modelPlateLength / 2,
                            0,
                            -modelPlateWidth/2 - Annotation.DefaultAnnotationWidth),
            });

            return new Annotation(points)
            {
                Text = ParamToString(detal.PlateLength),
                PropertyName = nameof(detal.PlateLength),                
            };
        }

        private Annotation GetPlateWidthAnnotation(Detal detal)
        {
            double modelPlateWidth = (double)detal.PlateWidth * (double)ScaleFactor;
            double modelPlateLength = (double)detal.PlateLength * (double)ScaleFactor;

            Point3DCollection points = new Point3DCollection(new List<Point3D>()
            {
                new Point3D(modelPlateLength / 2 + Annotation.DefaultAnnotationWidth,
                            0,
                            modelPlateWidth / 2),

                new Point3D(modelPlateLength / 2,
                            0,
                            modelPlateWidth / 2),

                new Point3D(modelPlateLength / 2,
                            0,
                            -modelPlateWidth / 2),

                new Point3D(modelPlateLength / 2 + Annotation.DefaultAnnotationWidth,
                            0,
                            -modelPlateWidth / 2),
            });

            return new Annotation(points, Annotation.ArrowSide.Left)
            {
                Text = ParamToString(detal.PlateWidth),
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
                            -modelPlateHeight,
                            -modelPlateWidth / 2),

                new Point3D(-modelPlateLength / 2,
                            modelPlateHeight,
                            -modelPlateWidth / 2),

                new Point3D(-modelPlateLength / 2 - Annotation.DefaultAnnotationWidth,
                            modelPlateHeight,
                            -modelPlateWidth / 2),

                new Point3D(-modelPlateLength / 2 - Annotation.DefaultAnnotationWidth,
                            -modelPlateHeight,
                            -modelPlateWidth / 2),
            });

            return new Annotation(points)
            {
                Text = $"Thickness: {detal.PlateThickness} mm",
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

        #endregion Private functions

        #region Public functions

        public ObservableCollection<Annotation> GetAnnotations(Detal detal)
        {
            List<Annotation> annotationsList = this.GetDetalAnnotations(detal);

            switch (detal.DetalType)
            {
                case DetalTypes.Plita:
                    annotationsList.AddRange(this.PlateAnnotations(detal as Plita));
                    break;

                default:
                    return null;
            }
            return new ObservableCollection<Annotation>(annotationsList);
        }

        public static string ParamToString(decimal param) => string.Format("{0} mm", param);

        #endregion
    }
}

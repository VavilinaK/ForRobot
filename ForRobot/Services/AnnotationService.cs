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
        public decimal ScaleFactor { get; set; } = 1.00M / 150.00M;

        #region Properties

        private Annotation GetPlateLengthAnnotation(Detal detal)
        {
            double modelPlateWidth = (double)detal.PlateWidth * (double)ScaleFactor;
            double modelPlateLength = (double)detal.PlateLength * (double)ScaleFactor;

            Point3DCollection points = new Point3DCollection(new List<Point3D>()
            {
                new Point3D(modelPlateWidth / 2 + 5,
                            modelPlateLength / 2,
                            0),

                new Point3D(modelPlateWidth / 2,
                            modelPlateLength / 2,
                            0),

                new Point3D(modelPlateWidth / 2,
                            -modelPlateLength / 2,
                            0),

                new Point3D(modelPlateWidth / 2 + 5,
                            -modelPlateLength / 2,
                            0),
            });

            return new Annotation(points, Annotation.ArrowSide.DA)
            {
                //StartPoint = new Point3D(modelPlateWidth / 2 + 20,
                //                         modelPlateLength / 2,
                //                         0),

                //EndPoint = new Point3D(modelPlateWidth / 2 + 20,
                //                      -modelPlateLength / 2,
                //                       0),

                //StartPoint = new Point3D(-((double)modelPlateWidth / 2),
                //                         (double)modelPlateLength / 2,
                //                         0),

                //EndPoint = new Point3D(-((double)modelPlateWidth / 2),
                //                       (double)modelPlateLength / 2 + 10,
                //                       0),

                //StartPoint = new Point3D(20, 10, 0),
                //EndPoint = new Point3D(30, 15, 0),

                Text = $"Length: {detal.PlateLength} mm",
                PropertyName = nameof(detal.PlateLength)
            };
        }

        //private Annotation GetPlateWidthAnnotation(Detal detal)
        //{
        //    return new Annotation()
        //    {
        //        Position = new Point3D(15, 10, 0),
        //        Text = $"Width: {detal.PlateWidth} mm",
        //        PropertyName = nameof(detal.PlateWidth)
        //    };
        //}

        //private Annotation GetPlateThicknessAnnotation(Detal detal)
        //{
        //    return new Annotation()
        //    {
        //        Position = new Point3D(10, 10, 0),
        //        Text = $"Thickness: {detal.PlateThickness} mm",
        //        PropertyName = nameof(detal.PlateThickness)
        //    };
        //}

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

        private ObservableCollection<Annotation> PlateAnnotations(Plita plate)
        {
            List<Annotation> annotations = new List<Annotation>()
            {
                this.GetPlateLengthAnnotation(plate)
                //this.GetPlateWidthAnnotation(plate),
                //this.GetPlateThicknessAnnotation(plate),
                //this.GetRibHeightAnnotation(plate),
                //this.GetRibThicknessAnnotation(plate),
                //this.GetTechOffsetSeamStartAnnotation(plate),
                //this.GetTechOffsetSeamEndAnnotation(plate),
                //this.GetBevelToLeftAnnotation(plate),
                //this.GetBevelToRightAnnotation(plate)
            };
            return new ObservableCollection<Annotation>(annotations);
        }

        public ObservableCollection<Annotation> GetAnnotations(Detal detal)
        {
            switch (detal.DetalType)
            {
                case DetalTypes.Plita:
                    return this.PlateAnnotations(detal as Plita);

                default:
                    return null;
            }
        }
    }
}

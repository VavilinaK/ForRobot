using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ForRobot.Model.Detals;
using ForRobot.Model.File3D;

namespace ForRobot.Services
{
    public interface IWeldService
    {
        ObservableCollection<Weld> GetWelds(Detal plate);
    }

    public sealed class WeldService : IWeldService
    {
        public decimal ScaleFactor { get; set; } = 1.00M / 100.00M;

        public WeldService(decimal _scaleFactor)
        {
            this.ScaleFactor = _scaleFactor;
        }

        #region Private functions

        private List<Weld> GetPlateWelds(Plita plate)
        {
            // Преобразование реальных размеров в модельные.
            double modelPlateWidth = (double)plate.PlateWidth * (double)ScaleFactor;
            double modelPlateHeight = (double)plate.PlateThickness * (double)ScaleFactor;
            double modelPlateLength = (double)plate.PlateLength * (double)ScaleFactor;
            double modelRibHeight = (double)plate.RibHeight * (double)ScaleFactor;
            double modelRibThickness = (double)plate.RibThickness * (double)ScaleFactor;

            List<Weld> welds = new List<Weld>();

            double weldPositionXAxes = 0; // Позиционирование швов

            for (int i = 0; i < plate.RibCount; i++)
            {
                var rib = plate.RibsCollection[i];

                double modelRibDistanceLeft = (double)rib.DistanceLeft * (double)ScaleFactor;
                double modelRibDistanceRight = (double)rib.DistanceRight * (double)ScaleFactor;
                double modelRibIdentToLeft = (double)rib.IdentToLeft * (double)ScaleFactor;
                double modelRibIdentToRight = (double)rib.IdentToRight * (double)ScaleFactor;
                double modelRibDissolutionLeft = (double)rib.DissolutionLeft * (double)ScaleFactor;
                double modelRibDissolutionRight = (double)rib.DissolutionRight * (double)ScaleFactor;

                double ribLength = modelPlateLength - (modelRibIdentToLeft + modelRibIdentToRight); // Длина ребра.

                weldPositionXAxes += modelRibDistanceLeft; // Расчёт позиции шва.
                double xPosition = (weldPositionXAxes - modelPlateWidth / 2) - modelRibThickness;

                double zStart = ribLength / 2 - modelRibDissolutionLeft;
                double zEnd = modelRibDissolutionRight - ribLength / 2;

                // Швы добавляются с обеих сторон ребра
                string name = String.Format("Weld {0}", Convert.ToDouble(welds.Count / 2 + 1));
                welds.Add(new Weld()
                {
                    Name = name + ".1",
                    StartPoint = new System.Windows.Media.Media3D.Point3D(xPosition, modelPlateHeight, zStart),
                    EndPoint = new System.Windows.Media.Media3D.Point3D(xPosition, modelPlateHeight, zEnd)
                });

                xPosition = (weldPositionXAxes - modelPlateWidth / 2) + modelRibThickness;

                welds.Add(new Weld()
                {
                    Name = name + ".2",
                    StartPoint = new System.Windows.Media.Media3D.Point3D(xPosition, modelPlateHeight, zStart),
                    EndPoint = new System.Windows.Media.Media3D.Point3D(xPosition, modelPlateHeight, zEnd)
                });

                //welds.Add(new Weld()
                //{
                //    StartPoint = new System.Windows.Media.Media3D.Point3D(xPosition,
                //                                                         yStart,
                //                                                         modelPlateHeight),
                //    EndPoint = new System.Windows.Media.Media3D.Point3D(xPosition,
                //                                                        yEnd,
                //                                                        modelPlateHeight)
                //});

                //xPosition = (weldPositionXAxes - modelPlateWidth / 2) + modelRibThickness;

                //welds.Add(new Weld()
                //{
                //    StartPoint = new System.Windows.Media.Media3D.Point3D(xPosition,
                //                                         yStart,
                //                                         modelPlateHeight),
                //    EndPoint = new System.Windows.Media.Media3D.Point3D(xPosition,
                //                                        yEnd,
                //                                        modelPlateHeight)
                //});

                // Перемещение позиции для следующего ребра
                if (!plate.ParalleleRibs)
                    weldPositionXAxes += modelRibThickness + modelRibDistanceRight;
            }
            return welds;
        }

        #endregion Private functions

        #region Public functions
        
        public ObservableCollection<Weld> GetWelds(Detal detal)
        {
            switch (detal.DetalType)
            {
                case DetalTypes.Plita:
                     return new ObservableCollection<Weld>(this.GetPlateWelds(detal as Plita));

                default:
                    return null;
            }
        }

        #endregion Public functions
    }
}

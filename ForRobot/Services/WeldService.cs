using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ForRobot.Model.Detals;

namespace ForRobot.Services
{
    public interface IWeldService
    {
        ObservableCollection<Weld> GetWelds(Plita plate);
    }

    public sealed class WeldService : IWeldService
    {
        public decimal ScaleFactor { get; set; } = 1.00M / 150.00M;

        public ObservableCollection<Weld> GetWelds(Plita plate)
        {
            // Преобразование реальных размеров в модельные.
            decimal modelPlateWidth = plate.PlateWidth * ScaleFactor;
            decimal modelPlateHeight = plate.PlateThickness * ScaleFactor;
            decimal modelPlateLength = plate.PlateLength * ScaleFactor;
            decimal modelRibHeight = plate.RibHeight * ScaleFactor;
            decimal modelRibThickness = plate.RibThickness * ScaleFactor;

            List<Weld> welds = new List<Weld>();

            double weldPositionXAxes = 0; // Позиционирование швов

            for (int i=0; i< plate.RibCount; i++)
            {
                var rib = plate.RibsCollection[i];

                decimal modelRibDistanceLeft = rib.DistanceLeft * ScaleFactor;
                decimal modelRibDistanceRight = rib.DistanceRight * ScaleFactor;
                decimal modelRibIdentToLeft = rib.IdentToLeft * ScaleFactor;
                decimal modelRibIdentToRight = rib.IdentToRight * ScaleFactor;
                decimal modelRibDissolutionLeft = rib.DissolutionLeft * ScaleFactor;
                decimal modelRibDissolutionRight = rib.DissolutionRight * ScaleFactor;

                decimal ribLength = modelPlateLength - modelRibIdentToLeft - modelRibIdentToRight; // Длина ребра.

                weldPositionXAxes += (double)modelRibDistanceLeft; // Расчёт позиции шва.
                double xPosition = (weldPositionXAxes - (double)modelPlateWidth / 2) + (double)modelRibThickness;

                double yStart = (double)ribLength / 2 - (double)modelRibDissolutionRight;
                double yEnd = (double)modelRibDissolutionLeft - (double)ribLength / 2;

                welds.Add(new Weld()
                {
                    StartPoint = new System.Windows.Media.Media3D.Point3D(xPosition,
                                                                         yStart,
                                                                         (double)modelPlateHeight),
                    EndPoint = new System.Windows.Media.Media3D.Point3D(xPosition,
                                                                        yEnd,
                                                                        (double)modelPlateHeight)
                });

                // Перемещение позиции для следующего ребра
                if (!plate.ParalleleRibs)
                    weldPositionXAxes += (double)modelRibThickness + (double)modelRibDistanceRight;
            }
            return new ObservableCollection<Weld>(welds);
        }
    }
}

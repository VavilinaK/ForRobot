using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;

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

        private void AddWeld(List<Weld> welds, Point3D startPoint, Point3D endPoint, string weldName = null)
        {
            welds.Add(new Weld()
            {
                Name = weldName,
                StartPoint = startPoint,
                EndPoint = endPoint
            });
        }

        private List<Weld> GetPlateWelds(Plita plate)
        {
            // Преобразование реальных размеров в модельные.
            double modelPlateWidth = (double)plate.PlateWidth * (double)ScaleFactor;
            double modelPlateHeight = (double)plate.PlateThickness * (double)ScaleFactor;
            double modelPlateLength = (double)plate.PlateLength * (double)ScaleFactor;
            double modelRibHeight = (double)plate.RibHeight * (double)ScaleFactor;
            double modelRibThickness = (double)plate.RibThickness * (double)ScaleFactor;

            List<Weld> welds = new List<Weld>();

            double weldPositionY = -modelPlateWidth / 2; // Начальная позиция по Y

            for (int i = 0; i < plate.RibCount; i++)
            {
                var rib = plate.RibsCollection[i];

                double modelRibDistanceLeft = (double)rib.DistanceLeft * (double)ScaleFactor;
                double modelRibDistanceRight = (double)rib.DistanceRight * (double)ScaleFactor;
                double modelRibIdentToLeft = (double)rib.IdentToLeft * (double)ScaleFactor;
                double modelRibIdentToRight = (double)rib.IdentToRight * (double)ScaleFactor;
                double modelRibDissolutionLeft = (double)rib.DissolutionLeft * (double)ScaleFactor;
                double modelRibDissolutionRight = (double)rib.DissolutionRight * (double)ScaleFactor;

                // Перемещение к позиции шва по Y - отступ от торца слева.
                weldPositionY += modelRibDistanceLeft;

                // Длина шва (с учётом отступов и роспусков)
                double weldLength = modelPlateLength - modelRibIdentToLeft - modelRibIdentToRight;
                
                double xStart = weldLength / 2 - modelRibDissolutionLeft;
                double xEnd = modelRibDissolutionRight - weldLength / 2;

                // Швы добавляются с обеих сторон ребра
                string weldName = String.Format("Weld {0}", Convert.ToDouble(welds.Count / 2 + 1));

                this.AddWeld(welds,
                             new Point3D(xStart, weldPositionY, modelPlateHeight),
                             new Point3D(xEnd, weldPositionY, modelPlateHeight),
                             weldName + ".1");

                this.AddWeld(welds,
                             new Point3D(xStart, weldPositionY + modelRibThickness, modelPlateHeight),
                             new Point3D(xEnd, weldPositionY + modelRibThickness, modelPlateHeight),
                             weldName + ".2");

                // Перемещение позиции до ребра
                if (!plate.ParalleleRibs)
                    weldPositionY += modelRibThickness + modelRibDistanceRight;
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

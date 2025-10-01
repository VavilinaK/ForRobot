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
        ObservableCollection<Weld> GetWelds(Detal detal);
    }

    /// <summary>
    /// Класс сервис добавления швов под тип детали
    /// </summary>
    public sealed class WeldService : IWeldService
    {
        private double _slopeOffset = ModelingService.SlopeOffset;

        public const double DEFAULT_WELD_THICKNESS = 5.0;

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
            double weldLeftPositionY = weldPositionY;
            double weldRightPositionY = weldPositionY;

            for (int i = 0; i < 1; i++)
            {
                var rib = plate.RibsCollection[i];

                double modelRibDistanceLeft = (double)rib.DistanceLeft * (double)ScaleFactor;
                double modelRibDistanceRight = (double)rib.DistanceRight * (double)ScaleFactor;
                double modelRibIdentToLeft = (double)rib.IdentToLeft * (double)ScaleFactor;
                double modelRibIdentToRight = (double)rib.IdentToRight * (double)ScaleFactor;
                double modelRibDissolutionLeft = (double)rib.DissolutionLeft * (double)ScaleFactor;
                double modelRibDissolutionRight = (double)rib.DissolutionRight * (double)ScaleFactor;

                weldLeftPositionY += modelRibDistanceLeft;
                weldRightPositionY += modelRibDistanceRight;

                // Длина шва (с учётом отступов ребра и роспусков)
                double weldLength = modelPlateLength - modelRibIdentToLeft - modelRibIdentToRight - modelRibDissolutionLeft - modelRibDissolutionRight;

                double ribYOffset = 0;  // Смещение задней части ребра для скоса
                switch (plate.ScoseType)
                {
                    case ScoseTypes.SlopeLeft:
                    case ScoseTypes.SlopeRight:
                        ribYOffset = (plate.ScoseType == ScoseTypes.SlopeLeft) ? -ModelingService.SlopeOffset : ModelingService.SlopeOffset;
                        break;

                    case ScoseTypes.TrapezoidTop:
                    case ScoseTypes.TrapezoidBottom:
                        // Положение шва по ширине плиты
                        double positionRatio = ((weldLeftPositionY + weldRightPositionY) / 2 + modelPlateWidth / 2) / modelPlateWidth;

                        // Длина основания в позиции ребра
                        double baseLength;
                        if (plate.ScoseType == ScoseTypes.TrapezoidTop)
                            baseLength = modelPlateLength * (1 - ModelingService.TrapezoidRatio * positionRatio);
                        else
                            baseLength = modelPlateLength * (1 - ModelingService.TrapezoidRatio * (1 - positionRatio));

                        // Длина шва (с учётом отступов ребра и роспусков)
                        weldLength = baseLength - modelPlateLength - modelRibIdentToLeft - modelRibIdentToRight - modelRibDissolutionLeft - modelRibDissolutionRight;
                        weldLength = Math.Max(weldLength, ModelingService.MIN_RIB_LENGTH);
                        break;
                }

                Point3D startPoint = new Point3D(-weldLength/2, weldLeftPositionY, modelPlateHeight);
                Point3D endPoint = new Point3D(weldLength/2, (weldRightPositionY) + ribYOffset, modelPlateHeight);

                // Швы добавляются с обеих сторон ребра
                string weldName = String.Format("Weld {0}", Convert.ToDouble(welds.Count / 2 + 1));
                this.AddWeld(welds, startPoint, endPoint, weldName + ".1");

                startPoint.Y += modelRibThickness;
                endPoint.Y += modelRibThickness;

                this.AddWeld(welds, startPoint, endPoint, weldName + ".2");
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

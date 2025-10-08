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

            //weldLeftPositionY -= modelRibThickness / 2;
            //weldRightPositionY -= modelRibThickness / 2;

            for (int i = 0; i < plate.RibCount; i++)
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

                // Длина шва
                double weldLength = modelPlateLength;

                double ribYOffset = 0;  // Смещение задней части ребра для скоса
                //double positionRatio = 0;
                switch (plate.ScoseType)
                {
                    case ScoseTypes.SlopeLeft:
                    case ScoseTypes.SlopeRight:

                        ribYOffset = (plate.ScoseType == ScoseTypes.SlopeLeft) ? -ModelingService.SlopeOffset
                                                                               : ModelingService.SlopeOffset;

                        //positionRatio = (plate.ScoseType == ScoseTypes.SlopeLeft)
                        //          ? Math.Abs((modelPlateLength / 2 - modelRibIdentToLeft) -
                        //                     (modelRibIdentToRight - modelPlateLength / 2)) / weldLength
                        //          : Math.Abs((modelRibIdentToRight - modelPlateLength / 2) -
                        //                     (modelPlateLength / 2 - modelRibIdentToLeft)) / weldLength;
                        break;

                    case ScoseTypes.TrapezoidTop:
                    case ScoseTypes.TrapezoidBottom:
                        // Положение шва по ширине плиты
                        double positionRatio = ((weldLeftPositionY + weldRightPositionY) / 2 + modelPlateWidth / 2) / modelPlateWidth;

                        if (plate.ScoseType == ScoseTypes.TrapezoidTop)
                            weldLength = modelPlateLength * (1 - ModelingService.TrapezoidRatio * positionRatio);
                        else
                            weldLength = modelPlateLength * (1 - ModelingService.TrapezoidRatio * (1 - positionRatio));
                        
                        weldLength = Math.Max(weldLength, ModelingService.MIN_RIB_LENGTH);
                        break;
                }
                double xStart = (modelRibIdentToLeft + modelRibDissolutionLeft) - weldLength / 2;
                double xEnd = weldLength / 2 - modelRibIdentToRight - modelRibDissolutionRight;

                Point3D startPoint = new Point3D(xStart, weldLeftPositionY, modelPlateHeight);
                Point3D endPoint = new Point3D(xEnd, weldRightPositionY + ribYOffset, modelPlateHeight);

                // Швы добавляются с обеих сторон ребра
                string weldName = String.Format("Weld {0}", Convert.ToDouble(welds.Count / 2 + 1));

                welds.Add(new Weld()
                {
                    Name = weldName + ".1",
                    StartPoint = startPoint,
                    EndPoint = endPoint
                });

                startPoint.Y += modelRibThickness * 1.5; // Костыль.
                endPoint.Y += modelRibThickness * 1.5;
                
                welds.Add(new Weld()
                {
                    Name = weldName + ".2",
                    StartPoint = startPoint,
                    EndPoint = endPoint
                });
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

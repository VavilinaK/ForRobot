using ForRobot.Models.Detals;
using ForRobot.Models.File3D;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Media3D;

namespace ForRobot.Libr.Services
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
        public const double DEFAULT_WELD_THICKNESS = 2.0;

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
            double modelRibHeight = (double)plate.RibsHeight * (double)ScaleFactor;
            double modelRibThickness = (double)plate.RibsThickness * (double)ScaleFactor;

            List<Weld> welds = new List<Weld>();

            double weldPositionY = -modelPlateWidth / 2; // Начальная позиция по Y
            double weldLeftPositionY = weldPositionY;
            double weldRightPositionY = weldPositionY;

            for (int i = 0; i < plate.RibsCount; i++)
            {
                var rib = plate.RibsCollection[i];

                double modelRibDistanceLeft = (double)rib.DistanceLeft * (double)ScaleFactor;
                double modelRibDistanceRight = (double)rib.DistanceRight * (double)ScaleFactor;
                double modelRibIdentToLeft = (double)rib.IdentToLeft * (double)ScaleFactor;
                double modelRibIdentToRight = (double)rib.IdentToRight * (double)ScaleFactor;
                double modelRibDissolutionLeft = (double)rib.DissolutionLeft * (double)ScaleFactor;
                double modelRibDissolutionRight = (double)rib.DissolutionRight * (double)ScaleFactor;

                double centerX = (modelRibIdentToLeft - modelRibIdentToRight) / 2;

                weldLeftPositionY += modelRibDistanceLeft;
                weldRightPositionY += modelRibDistanceRight;

                // Длина шва
                double weldLength = modelPlateLength;

                double weldYOffset = 0;  // Смещение задней части ребра для скоса
                switch (plate.ScoseType)
                {
                    case ScoseTypes.SlopeLeft:
                    case ScoseTypes.SlopeRight:
                        double positionRatio = ((weldLeftPositionY + weldRightPositionY) / 2 + modelPlateWidth / 2) / modelPlateWidth * 2 - 1;
                        weldYOffset = ((plate.ScoseType == ScoseTypes.SlopeLeft) ? -ModelingService.SLOPE_OFF_SET : ModelingService.SLOPE_OFF_SET) * positionRatio;
                        break;

                    case ScoseTypes.TrapezoidTop:
                    case ScoseTypes.TrapezoidBottom:
                        double trapezoidPositionRatio = ((weldLeftPositionY + weldRightPositionY) / 2 + modelPlateWidth / 2) / modelPlateWidth;

                        if (plate.ScoseType == ScoseTypes.TrapezoidTop)
                            weldLength = modelPlateLength * (1 - ModelingService.TRAPEZOID_RATIO * trapezoidPositionRatio);
                        else
                            weldLength = modelPlateLength * (1 - ModelingService.TRAPEZOID_RATIO * (1 - trapezoidPositionRatio));

                        weldLength = Math.Max(weldLength, ModelingService.MIN_RIB_LENGTH);
                        break;
                }

                double xStart = (modelRibIdentToLeft + modelRibDissolutionLeft) - weldLength / 2;
                double xEnd = weldLength / 2 - modelRibIdentToRight - modelRibDissolutionRight;

                Point3D startPoint = new Point3D(xStart + weldYOffset, weldLeftPositionY, modelPlateHeight);
                Point3D endPoint = new Point3D(xEnd + weldYOffset, weldRightPositionY, modelPlateHeight);

                // Швы добавляются с обеих сторон ребра
                string weldName = String.Format("Weld {0}", i + 1);

                welds.Add(new Weld(ForRobot.Themes.Colors.WeldColor, ForRobot.Themes.Colors.LeftSideWeldColor, ForRobot.Themes.Colors.RightSideWeldColor)
                {
                    Name = weldName + ".1",
                    StartPoint = startPoint,
                    EndPoint = endPoint
                });

                startPoint.Y += modelRibThickness;
                endPoint.Y += modelRibThickness;

                welds.Add(new Weld(ForRobot.Themes.Colors.WeldColor, ForRobot.Themes.Colors.LeftSideWeldColor, ForRobot.Themes.Colors.RightSideWeldColor)
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

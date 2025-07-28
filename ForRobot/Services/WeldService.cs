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

    /// <summary>
    /// Класс сервис добавления швов под тип детали
    /// </summary>
    public sealed class WeldService : IWeldService
    {
        private double _slopeOffset = ModelingService.SlopeOffset;

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
                                
                double weldLength = modelPlateLength; // Длина шва

                double xStart = weldLength / 2 - modelRibIdentToLeft - modelRibDissolutionLeft;
                double xEnd = (modelRibIdentToRight + modelRibDissolutionRight) - weldLength / 2;
                
                Point3D startPoint, endPoint;
                switch (plate.ScoseType)
                {
                    case ScoseTypes.SlopeLeft:
                    case ScoseTypes.SlopeRight:

                        double baseY = weldPositionY - modelRibThickness/2;

                        double ribYOffset = (plate.ScoseType == ScoseTypes.SlopeLeft) ? -this._slopeOffset
                                                                                      : this._slopeOffset;

                        double totalLength = modelPlateLength - modelRibIdentToLeft - modelRibIdentToRight;
                        //double positionRatio = (plate.ScoseType == ScoseTypes.SlopeLeft)
                        //                       ? Math.Abs(xStart - xEnd) / totalLength
                        //                       : Math.Abs(xEnd - xStart) / totalLength;
                        double positionRatio = (plate.ScoseType == ScoseTypes.SlopeLeft)
                                  ? Math.Abs((modelPlateLength / 2 - modelRibIdentToLeft) -
                                             (modelRibIdentToRight - modelPlateLength / 2)) / totalLength
                                  : Math.Abs((modelRibIdentToRight - modelPlateLength / 2) -
                                             (modelPlateLength / 2 - modelRibIdentToLeft)) / totalLength;

                        startPoint = new Point3D(xStart, baseY + ribYOffset * positionRatio, modelPlateHeight);
                        endPoint = new Point3D(xEnd, baseY, modelPlateHeight);

                        ////double centerY = weldPositionY;
                        //double centerY = weldPositionY - modelRibThickness / 2;
                        ////double centerY = (plate.ScoseType == ScoseTypes.SlopeLeft) ? weldPositionY - modelRibThickness / 2 : weldPositionY + modelRibThickness * 1.5;

                        //// Смещение задней части ребра для скоса
                        //double ribYOffset = (plate.ScoseType == ScoseTypes.SlopeLeft) ? -this._slopeOffset
                        //                                                              : this._slopeOffset;

                        //double totalLength = modelPlateLength - modelRibIdentToLeft - modelRibIdentToRight;
                        ////double positionRatio = (xStart - xEnd) / totalLength;
                        //double positionRatio = (plate.ScoseType == ScoseTypes.SlopeLeft) 
                        //                       ? Math.Abs(xStart - xEnd) / totalLength
                        //                       : Math.Abs(xEnd - xStart) / totalLength;

                        ////if (plate.ScoseType == ScoseTypes.SlopeRight)
                        ////{
                        ////    startPoint = new Point3D(xStart, centerY, modelPlateHeight);
                        ////    endPoint = new Point3D(xEnd, centerY + ribYOffset * positionRatio, modelPlateHeight);
                        ////}
                        ////else
                        ////{
                        ////    startPoint = new Point3D(xStart, centerY + ribYOffset * positionRatio, modelPlateHeight);
                        ////    endPoint = new Point3D(xEnd, centerY, modelPlateHeight);
                        ////}

                        //startPoint = new Point3D(xStart, centerY + ribYOffset, modelPlateHeight);
                        ////startPoint = new Point3D(xStart, centerY + ribYOffset * positionRatio, modelPlateHeight);
                        //endPoint = new Point3D(xEnd, centerY, modelPlateHeight);
                        break;                        

                    case ScoseTypes.TrapezoidTop:
                    case ScoseTypes.TrapezoidBottom:
                        positionRatio = (weldPositionY + modelPlateWidth / 2) / modelPlateWidth;
                        if (plate.ScoseType == ScoseTypes.TrapezoidTop)
                        {
                            weldLength = modelPlateLength * (1 - ModelingService.TrapezoidRatio * (1 - positionRatio));
                        }
                        else
                        {
                            weldLength = modelPlateLength * (1 - ModelingService.TrapezoidRatio * positionRatio);
                        }
                        weldLength = Math.Max(weldLength, ModelingService.MIN_RIB_LENGTH);

                        xStart = weldLength / 2 - modelRibIdentToLeft - modelRibDissolutionLeft;
                        xEnd = (modelRibIdentToRight + modelRibDissolutionRight) - weldLength / 2;

                        startPoint = new Point3D(xStart, weldPositionY, modelPlateHeight);
                        endPoint = new Point3D(xEnd, weldPositionY, modelPlateHeight);
                        break;

                    default:
                        startPoint = new Point3D(xStart, weldPositionY, modelPlateHeight);
                        endPoint = new Point3D(xEnd, weldPositionY, modelPlateHeight);
                        break;
                }
                
                // Швы добавляются с обеих сторон ребра
                string weldName = String.Format("Weld {0}", Convert.ToDouble(welds.Count / 2 + 1));

                this.AddWeld(welds, startPoint, endPoint, weldName + ".1");

                startPoint.Y += modelRibThickness;
                endPoint.Y += modelRibThickness;

                this.AddWeld(welds, startPoint, endPoint, weldName + ".2");

                // Перемещение позиции до следующего ребра
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

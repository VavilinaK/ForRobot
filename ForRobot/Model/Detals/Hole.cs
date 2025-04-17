using System;
using System.Windows.Media.Media3D;

namespace ForRobot.Model.Detals
{
    public class Hole
    {
        public double Diameter { get; set; } = 25;       // Диаметр отверстия
        public Point3D Position { get; set; }            // Координаты центра
        //public bool RequiresCountersink { get; set; }    // Нужна ли зенковка (2×2 мм)
    }
}

using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

using ForRobot.Model.File3D;

namespace ForRobot.Model.Detals
{
    public class Weld
    {
        /// <summary>
        /// Начало шва
        /// </summary>
        public Point3D StartPoint { get; set; }

        /// <summary>
        /// Конец шва
        /// </summary>
        public Point3D EndPoint { get; set; }

        /// <summary>
        /// Толщина линии шва
        /// </summary>
        public double Thickness { get; set; } = 4;

        /// <summary>
        /// Цвет шва
        /// </summary>
        public Color Color { get; set; } = (Materials.DefaultWeldBrush as SolidColorBrush).Color;
    }
}

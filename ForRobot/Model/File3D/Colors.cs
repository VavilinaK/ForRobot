using System;
using System.Windows.Media;
using ForRobot.Libr.Attributes;

namespace ForRobot.Model.File3D
{
    /// <summary>
    /// Цвета элементов 3д просмотрщика
    /// </summary>
    public static class Colors
    {
        private static Color DefaultPlateColor => Color.FromRgb(23, 230, 75);
        private static Color DefaultRibsColor => Color.FromRgb(23, 230, 75);
        private static Color DefaultWeldColor => Color.FromRgb(255, 142, 20);
        private static Color DefaultLeftWeldColor => Color.FromRgb(43, 255, 209);
        private static Color DefaultRightWeldColor => Color.FromRgb(255, 31, 53);
        private static Color DefaultAnnotationColor => Color.FromRgb(0, 191, 255);

        [PropertyName("Плита")]
        public static Color PlateColor { get; set; }

        [PropertyName("Ребра")]
        public static Color RibsColor { get; set; }
        
        [PropertyName("Сварочные швы")]
        public static Color WeldColor { get; set; }
        
        [PropertyName("Левая сторона шва")]
        public static Color LeftWeldColor { get; set; }
        
        [PropertyName("Правая сторона шва")]
        public static Color RightWeldColor { get; set; }

        [PropertyName("Параметры детали")]
        public static Color AnnotationColor { get; set; }

        static Colors()
        {
            PlateColor = DefaultPlateColor;
            RibsColor = DefaultRibsColor;
            WeldColor = DefaultWeldColor;
            LeftWeldColor = DefaultLeftWeldColor;
            RightWeldColor = DefaultRightWeldColor;
            AnnotationColor = DefaultAnnotationColor;
        }
    }
}

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
        private static Color DefaultRobotColor => Color.FromRgb(255, 132, 0);
        private static Color DefaultPcColor => Color.FromRgb(66, 66, 66);
        private static Color DefaultWatcherColor => Color.FromRgb(217, 0, 0);
        private static Color DefaultPlateColor => Color.FromRgb(23, 230, 75);
        private static Color DefaultRibsColor => Color.FromRgb(23, 230, 75);
        private static Color DefaultWeldColor => Color.FromRgb(255, 142, 20);
        private static Color DefaultLeftWeldColor => Color.FromRgb(43, 255, 209);
        private static Color DefaultRightWeldColor => Color.FromRgb(255, 31, 53);
        private static Color DefaultAnnotationArrowsColor => Color.FromRgb(0, 191, 255);
        private static Color DefaultAnnotationTextColor => Color.FromRgb(0, 0, 0);
        private static Color DefaultSelectorBoxColor => Color.FromRgb(255, 218, 33);

        [PropertyName("Робот")]
        public static Color RobotColor { get; set; } = DefaultRobotColor;

        [PropertyName("ПК")]
        public static Color PcColor { get; set; } = DefaultPcColor;

        [PropertyName("Наблюдатель")]
        public static Color WatcherColor { get; set; } = DefaultWatcherColor;

        [PropertyName("Плита")]
        public static Color PlateColor { get; set; } = DefaultPlateColor;

        [PropertyName("Ребра")]
        public static Color RibsColor { get; set; } = DefaultRibsColor;

        [PropertyName("Сварочные швы")]
        public static Color WeldColor { get; set; } = DefaultWeldColor;

        [PropertyName("Левая сторона шва")]
        public static Color LeftWeldColor { get; set; } = DefaultLeftWeldColor;

        [PropertyName("Правая сторона шва")]
        public static Color RightWeldColor { get; set; } = DefaultRightWeldColor;

        [PropertyName("Параметры детали (стрелки)")]
        public static Color AnnotationArrowsColor { get; set; } = DefaultAnnotationArrowsColor;

        [PropertyName("Параметры детали (тескт)")]
        public static Color AnnotationTextColor { get; set; } = DefaultAnnotationTextColor;

        [PropertyName("Бокс выбора модели/части модели")]
        public static Color SelectorBoxColor { get; set; } = DefaultSelectorBoxColor;

        /// <summary>
        /// Установка стандартных значений
        /// </summary>
        public static void DefaultColors()
        {
            PlateColor = DefaultPlateColor;
            RibsColor = DefaultRibsColor;
            WeldColor = DefaultWeldColor;
            LeftWeldColor = DefaultLeftWeldColor;
            RightWeldColor = DefaultRightWeldColor;
            AnnotationArrowsColor = DefaultAnnotationArrowsColor;
            AnnotationTextColor = DefaultAnnotationTextColor;
            SelectorBoxColor = DefaultSelectorBoxColor;
        }
    }
}

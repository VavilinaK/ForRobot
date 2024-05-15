using System;

namespace ForRobot.Model
{
    /// <summary>
    /// Перечень типов скосов настила
    /// </summary>
    public static class ScoseTypes
    {
        /// <summary>
        /// Прямоугольная форма
        /// </summary>
        public static readonly string Rect = "d_rect";

        /// <summary>
        /// Наклон влево
        /// </summary>
        public static readonly string SlopeLeft = "d_slope_leftt";

        /// <summary>
        /// Наклон вправо
        /// </summary>
        public static readonly string SlopeRight = "d_slope_right";

        /// <summary>
        /// Трапеция
        /// </summary>
        public static readonly string TrapezoidTop = "d_trapezoid_top";

        /// <summary>
        /// Перевёрнутая трапеция
        /// </summary>
        public static readonly string TrapezoidBottom = "d_trapezoid_bottom";
    }
}

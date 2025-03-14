using System;
using System.Linq;
using System.ComponentModel;

namespace ForRobot.Model.Detals
{
    /// <summary>
    /// Перечень типов скосов настила
    /// </summary>
    public static class ScoseTypes
    {
        [Description("Прямоугольник")]
        /// <summary>
        /// Прямоугольная форма
        /// </summary>
        public static readonly string Rect = "d_rect";

        [Description("С наклоном влево")]
        /// <summary>
        /// Наклон влево
        /// </summary>
        public static readonly string SlopeLeft = "d_slope_left";

        [Description("С наклоном вправо")]
        /// <summary>
        /// Наклон вправо
        /// </summary>
        public static readonly string SlopeRight = "d_slope_right";

        [Description("Трапеция")]
        /// <summary>
        /// Трапеция
        /// </summary>
        public static readonly string TrapezoidTop = "d_trapezoid_top";

        [Description("Перевёрнутая трапеция")]
        /// <summary>
        /// Перевёрнутая трапеция
        /// </summary>
        public static readonly string TrapezoidBottom = "d_trapezoid_bottom";

        public static string[] Descriptions
        {
            get
            {
                var Descriptions = typeof(ForRobot.Model.Detals.ScoseTypes).GetFields().Select(field => field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).SingleOrDefault() as System.ComponentModel.DescriptionAttribute);
                return Descriptions.Where(item => item != null).Select(item => item.Description).ToArray<string>();
            }
        }

        public static object FieldByDescription(string sDiscription)
        {
            var v = typeof(ForRobot.Model.Detals.ScoseTypes).GetFields().Where(field => (field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).SingleOrDefault() as System.ComponentModel.DescriptionAttribute).Description == sDiscription);
            return v.First().GetValue(null);
        }
    }
}

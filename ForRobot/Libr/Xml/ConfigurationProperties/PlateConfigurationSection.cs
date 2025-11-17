using System;
using System.Configuration;

using ForRobot.Model.Detals;

namespace ForRobot.Libr.ConfigurationProperties
{
    /// <summary>
    /// Класс для вывода стандартных свойств детали типа <see cref="ForRobot.Model.Detals.DetalTypes.Plita"/> из app.config
    /// </summary>
    public class PlateConfigurationSection : System.Configuration.ConfigurationSection
    {
        #region Detail's Properties

        [ConfigurationProperty("detail_reverse_deflection")]
        /// <summary>
        /// Обратный прогиб детали
        /// </summary>
        public decimal ReverseDeflection
        {
            get { return (decimal)this[nameof(Plita.ReverseDeflection)]; }
            set { this[nameof(Plita.ReverseDeflection)] = value; }
        }

        [ConfigurationProperty("base_width")]
        /// <summary>
        /// Ширина настила
        /// </summary>
        public decimal PlateWidth
        {
            get { return (decimal)this[nameof(Plita.PlateWidth)]; }
            set { this[nameof(Plita.PlateWidth)] = value; }
        }

        [ConfigurationProperty("base_length")]
        /// <summary>
        /// Длина настила
        /// </summary>
        public decimal PlateLength
        {
            get { return (decimal)this[nameof(Plita.PlateLength)]; }
            set { this[nameof(Plita.PlateLength)] = value; }
        }

        [ConfigurationProperty("base_thickness")]
        /// <summary>
        /// Толщина настила
        /// </summary>
        public decimal PlateThickness
        {
            get { return (decimal)this[nameof(Plita.PlateThickness)]; }
            set { this[nameof(Plita.PlateThickness)] = value; }
        }

        [ConfigurationProperty("base_bevel_left")]
        /// <summary>
        /// Скос настила слева
        /// </summary>
        public decimal PlateBevelToLeft
        {
            get { return (decimal)this[nameof(Plita.PlateBevelToLeft)]; }
            set { this[nameof(Plita.PlateBevelToLeft)] = value; }
        }

        [ConfigurationProperty("base_bevel_right")]
        /// <summary>
        /// Скос настила справа
        /// </summary>
        public decimal PlateBevelToRight
        {
            get { return (decimal)this[nameof(Plita.PlateBevelToRight)]; }
            set { this[nameof(Plita.PlateBevelToRight)] = value; }
        }

        #endregion Detail's Properties

        [ConfigurationProperty("wall_height")]
        /// <summary>
        /// Высота ребра
        /// </summary>
        public decimal RibsHeight
        {
            get { return (decimal)this[nameof(Plita.RibsHeight)]; }
            set { this[nameof(Plita.RibsHeight)] = value; }
        }

        [ConfigurationProperty("wall_thickness")]
        /// <summary>
        /// Толщина ребра
        /// </summary>
        public decimal RibsThickness
        {
            get { return (decimal)this[nameof(Plita.RibsThickness)]; }
            set { this[nameof(Plita.RibsThickness)] = value; }
        }

        [ConfigurationProperty("wall_count")]
        /// <summary>
        /// Количество ребер
        /// </summary>
        public int RibsCount
        {
            get { return (int)this[nameof(Plita.RibsCount)]; }
            set { this[nameof(Plita.RibsCount)] = value; }
        }

        [ConfigurationProperty("DistanceToFirstRib")]
        /// <summary>
        /// Поперечное расстояние по ширине до осевой линии первого ребра
        /// </summary>
        public decimal DistanceToFirstRib
        {
            get { return (decimal)this[nameof(Plita.DistanceToFirstRib)]; }
            set { this[nameof(Plita.DistanceToFirstRib)] = value; }
        }

        [ConfigurationProperty("DistanceBetweenRibs")]
        /// <summary>
        /// Поперечное расстояние между осевыми линиями рёбер
        /// </summary>
        public decimal DistanceBetweenRibs
        {
            get { return (decimal)this[nameof(Plita.DistanceBetweenRibs)]; }
            set { this[nameof(Plita.DistanceBetweenRibs)] = value; }
        }

        [ConfigurationProperty("wall_long_dist_left")]
        /// <summary>
        /// Продольное расстояние до ребер по левому краю
        /// </summary>
        public decimal IdentToLeft
        {
            get { return (decimal)this[nameof(Plita.IdentToLeft)]; }
            set { this[nameof(Plita.IdentToLeft)] = value; }
        }

        [ConfigurationProperty("wall_long_dist_right")]
        /// <summary>
        /// Продольное расстояние до ребер по правому краю
        /// </summary>
        public decimal IdentToRight
        {
            get { return (decimal)this[nameof(Plita.IdentToRight)]; }
            set { this[nameof(Plita.IdentToRight)] = value; }
        }

        [ConfigurationProperty("weld_offset_left")]
        /// <summary>
        /// Отступ шва от левого края ребер (роспуск, выкружка)
        /// </summary>
        public decimal DissolutionLeft
        {
            get { return (decimal)this[nameof(Plita.DissolutionLeft)]; }
            set { this[nameof(Plita.DissolutionLeft)] = value; }
        }

        [ConfigurationProperty("weld_offset_right")]
        /// <summary>
        /// Отступ шва от правого края ребер (роспуск, выкружка)
        /// </summary>
        public decimal DissolutionRight
        {
            get { return (decimal)this[nameof(Plita.DissolutionRight)]; }
            set { this[nameof(Plita.DissolutionRight)] = value; }
        }
    }
}

using System;
using System.Configuration;

using ForRobot.Model.Detals;

namespace ForRobot.Libr.Xml.ConfigurationProperties
{
    /// <summary>
    /// Класс для вывода стандартных свойств сварки детали типа <see cref="ForRobot.Model.Detals.DetalTypes.Plita"/> из app.config
    /// </summary>
    public class PlateWeldingConfigurationSection : System.Configuration.ConfigurationSection
    {
        [ConfigurationProperty("search_offset_start")]
        /// <summary>
        /// Отступ поиска в начале шва
        /// </summary>
        public decimal SearchOffsetStart
        {
            get { return (decimal)this[nameof(WeldingProperties.SearchOffsetStart)]; }
            set { this[nameof(WeldingProperties.SearchOffsetStart)] = value; }
        }

        [ConfigurationProperty("search_offset_end")]
        /// <summary>
        /// Отступ поиска в конце шва
        /// </summary>
        public decimal SearchOffsetEnd
        {
            get { return (decimal)this[nameof(WeldingProperties.SearchOffsetEnd)]; }
            set { this[nameof(WeldingProperties.SearchOffsetEnd)] = value; }
        }

        [ConfigurationProperty("weld_tech_offset_start")]
        /// <summary>
        /// Технологический отступ начала шва
        /// </summary>
        public decimal TechOffsetSeamStart
        {
            get { return (decimal)this[nameof(WeldingProperties.TechOffsetSeamStart)]; }
            set { this[nameof(WeldingProperties.TechOffsetSeamStart)] = value; }
        }

        [ConfigurationProperty("weld_tech_offset_end")]
        /// <summary>
        /// Технологический отступ конца шва
        /// </summary>
        public decimal TechOffsetSeamEnd
        {
            get { return (decimal)this[nameof(WeldingProperties.TechOffsetSeamEnd)]; }
            set { this[nameof(WeldingProperties.TechOffsetSeamEnd)] = value; }
        }

        [ConfigurationProperty("weld_overlap")]
        /// <summary>
        /// Перекрытие швов в месте соединения
        /// </summary>
        public decimal SeamsOverlap
        {
            get { return (decimal)this[nameof(WeldingProperties.SeamsOverlap)]; }
            set { this[nameof(WeldingProperties.SeamsOverlap)] = value; }
        }

        [ConfigurationProperty("weld_job")]
        /// <summary>
        /// Номер используемого джоба (ячейки) на источнике
        /// </summary>
        public int ProgramNom
        {
            get { return (int)this[nameof(WeldingProperties.ProgramNom)]; }
            set { this[nameof(WeldingProperties.ProgramNom)] = value; }
        }

        [ConfigurationProperty("weld_velocity")]
        /// <summary>
        /// Скорость сварки
        /// </summary>
        public int WeldingSpead
        {
            get { return (int)this[nameof(WeldingProperties.WeldingSpead)]; }
            set { this[nameof(WeldingProperties.WeldingSpead)] = value; }
        }

        [ConfigurationProperty("gantry_radius_weld")]
        /// <summary>
        /// Дистанция до позиционера для сварки
        /// </summary>
        public decimal DistanceForWelding
        {
            get { return (decimal)this[nameof(WeldingProperties.DistanceForWelding)]; }
            set { this[nameof(WeldingProperties.DistanceForWelding)] = value; }
        }

        [ConfigurationProperty("gantry_radius_search")]
        /// <summary>
        /// Дистанция до позиционера для поиска
        /// </summary>
        public decimal DistanceForSearch
        {
            get { return (decimal)this[nameof(WeldingProperties.DistanceForSearch)]; }
            set { this[nameof(WeldingProperties.DistanceForSearch)] = value; }
        }
    }
}

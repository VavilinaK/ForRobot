using System;
using System.Configuration;

using ForRobot.Models.Detals;

namespace ForRobot.Libr.Configuration.ConfigurationProperties
{
    /// <summary>
    /// Класс для вывода стандартных свойств детали типа <see cref="ForRobot.Models.Detals.DetalTypes.Plita"/> из app.config
    /// </summary>
    public class PlateConfigurationSection : BaseConfigurationSection
    {
        public string PlitaGenerator => GetValue<string>("PlitaGenerator");
        public string PlitaProgramName => GetValue<string>("PlitaProgramName");

        public decimal ReverseDeflection => GetValue<decimal>("detail_reverse_deflection");
        public decimal PlateWidth => GetValue<decimal>("base_width");
        public decimal PlateLength => GetValue<decimal>("base_length");
        public decimal PlateThickness => GetValue<decimal>("base_thickness");
        public decimal PlateBevelToLeft => GetValue<decimal>("base_bevel_left");
        public decimal PlateBevelToRight => GetValue<decimal>("base_bevel_right");

        public decimal RibsHeight => GetValue<decimal>("RibsHeight");
        public decimal RibsThickness => GetValue<decimal>("wall_thickness");
        public int RibsCount => GetValue<int>("wall_count");
        public decimal DistanceToFirstRib => GetValue<decimal>("DistanceToFirstRib");
        public decimal DistanceBetweenRibs => GetValue<decimal>("DistanceBetweenRibs");
        public decimal RibsIdentToLeft => GetValue<decimal>("wall_long_dist_left");
        public decimal RibsIdentToRight => GetValue<decimal>("wall_long_dist_right");
        public decimal WeldsDissolutionLeft => GetValue<decimal>("weld_offset_left");
        public decimal WeldsDissolutionRight => GetValue<decimal>("weld_offset_right");

        public decimal SearchOffsetStart => GetValue<decimal>("search_offset_start");
        public decimal SearchOffsetEnd => GetValue<decimal>("search_offset_end");
        public decimal TechOffsetSeamStart => GetValue<decimal>("weld_tech_offset_start");
        public decimal TechOffsetSeamEnd => GetValue<decimal>("weld_tech_offset_end");
        public decimal SeamsOverlap => GetValue<decimal>("weld_overlap");
        public decimal ProgramNom => GetValue<decimal>("weld_job");
        public decimal WeldingSpead => GetValue<decimal>("weld_velocity");
        public decimal DistanceForWelding => GetValue<decimal>("gantry_radius_weld");
        public decimal DistanceForSearch => GetValue<decimal>("gantry_radius_search");
    }
}

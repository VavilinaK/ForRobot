using System;
using System.Configuration;

namespace ForRobot.Libr
{
    /// <summary>
    /// Класс для вывода стандартных свойств настила из app.config
    /// </summary>
    public class PlitaConfigurationSection : System.Configuration.ConfigurationSection
    {
        [ConfigurationProperty("SumReber")]
        public int SumReber
        {
            get { return (int)this["SumReber"]; }
            set { this["SumReber"] = value; }
        }

        [ConfigurationProperty("Long")]
        public decimal Long
        {
            get { return (decimal)this["Long"]; }
            set { this["Long"] = value; }
        }

        [ConfigurationProperty("Wight")]
        public decimal Wight
        {
            get { return (decimal)this["Wight"]; }
            set { this["Wight"] = value; }
        }

        [ConfigurationProperty("Hight")]
        public decimal Hight
        {
            get { return (decimal)this["Hight"]; }
            set { this["Hight"] = value; }
        }

        [ConfigurationProperty("DistanceToFirst")]
        public decimal DistanceToFirst
        {
            get { return (decimal)this["DistanceToFirst"]; }
            set { this["DistanceToFirst"] = value; }
        }

        [ConfigurationProperty("DistanceBetween")]
        public decimal DistanceBetween
        {
            get { return (decimal)this["DistanceBetween"]; }
            set { this["DistanceBetween"] = value; }
        }

        [ConfigurationProperty("DistanceToStart")]
        public decimal DistanceToStart
        {
            get { return (decimal)this["DistanceToStart"]; }
            set { this["DistanceToStart"] = value; }
        }

        [ConfigurationProperty("DistanceToEnd")]
        public decimal DistanceToEnd
        {
            get { return (decimal)this["DistanceToEnd"]; }
            set { this["DistanceToEnd"] = value; }
        }

        [ConfigurationProperty("DissolutionStart")]
        public decimal DissolutionStart
        {
            get { return (decimal)this["DissolutionStart"]; }
            set { this["DissolutionStart"] = value; }
        }

        [ConfigurationProperty("DissolutionEnd")]
        public decimal DissolutionEnd
        {
            get { return (decimal)this["DissolutionEnd"]; }
            set { this["DissolutionEnd"] = value; }
        }

        [ConfigurationProperty("ThicknessPlita")]
        public decimal ThicknessPlita
        {
            get { return (decimal)this["ThicknessPlita"]; }
            set { this["ThicknessPlita"] = value; }
        }

        [ConfigurationProperty("ThicknessRebro")]
        public decimal ThicknessRebro
        {
            get { return (decimal)this["ThicknessRebro"]; }
            set { this["ThicknessRebro"] = value; }
        }

        [ConfigurationProperty("SearchOffsetStart")]
        public decimal SearchOffsetStart
        {
            get { return (decimal)this["SearchOffsetStart"]; }
            set { this["SearchOffsetStart"] = value; }
        }

        [ConfigurationProperty("SearchOffsetEnd")]
        public decimal SearchOffsetEnd
        {
            get { return (decimal)this["SearchOffsetEnd"]; }
            set { this["SearchOffsetEnd"] = value; }
        }

        [ConfigurationProperty("SeamsOverlap")]
        public decimal SeamsOverlap
        {
            get { return (decimal)this["SeamsOverlap"]; }
            set { this["SeamsOverlap"] = value; }
        }

        [ConfigurationProperty("TechOffsetSeamStart")]
        public decimal TechOffsetSeamStart
        {
            get { return (decimal)this["TechOffsetSeamStart"]; }
            set { this["TechOffsetSeamStart"] = value; }
        }

        [ConfigurationProperty("TechOffsetSeamEnd")]
        public decimal TechOffsetSeamEnd
        {
            get { return (decimal)this["TechOffsetSeamEnd"]; }
            set { this["TechOffsetSeamEnd"] = value; }
        }

        [ConfigurationProperty("BevelToStart")]
        public decimal BevelToStart
        {
            get { return (decimal)this["BevelToStart"]; }
            set { this["BevelToStart"] = value; }
        }

        [ConfigurationProperty("BevelToEnd")]
        public decimal BevelToEnd
        {
            get { return (decimal)this["BevelToEnd"]; }
            set { this["BevelToEnd"] = value; }
        }

        [ConfigurationProperty("WildingSpead")]
        public int WildingSpead
        {
            get { return (int)this["WildingSpead"]; }
            set { this["WildingSpead"] = value; }
        }

        [ConfigurationProperty("ProgramNom")]
        public int ProgramNom
        {
            get { return (int)this["ProgramNom"]; }
            set { this["ProgramNom"] = value; }
        }
    }
}

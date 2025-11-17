using System;
using System.Configuration;

namespace ForRobot.Libr.ConfigurationProperties
{
    /// <summary>
    /// Класс для вывода стандартных свойств робота из app.config
    /// </summary>
    public class RobotConfigurationSection : System.Configuration.ConfigurationSection
    {
        [ConfigurationProperty("FolderOfGeneration")]
        public string PathForGeneration
        {
            get { return (string)this["FolderOfGeneration"]; }
            set { this["FolderOfGeneration"] = value; }
        }

        [ConfigurationProperty("ControlerFolder")]
        public string PathControllerFolder
        {
            get { return (string)this["ControlerFolder"]; }
            set { this["ControlerFolder"] = value; }
        }
    }
}

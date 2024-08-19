using System;
using System.Configuration;

namespace ForRobot.Libr
{
    /// <summary>
    /// Класс для вывода стандартных свойств робота из app.config
    /// </summary>
    public class RobotConfigurationSection : System.Configuration.ConfigurationSection
    {
        [ConfigurationProperty("ControlerFolder")]
        public string PathControllerFolder
        {
            get { return (string)this["ControlerFolder"]; }
            set { this["ControlerFolder"] = value; }
        }
    }
}

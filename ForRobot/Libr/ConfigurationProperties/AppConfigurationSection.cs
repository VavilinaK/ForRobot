using System;
using System.Configuration;

namespace ForRobot.Libr.ConfigurationProperties
{
    /// <summary>
    /// Класс для вывода настроек приложения из app.config
    /// </summary>
    public class AppConfigurationSection : System.Configuration.ConfigurationSection
    {
        [ConfigurationProperty("PlitaGenerator")]
        public string PlitaGenerator
        {
            get { return (string)this["PlitaGenerator"]; }
            set { this["PlitaGenerator"] = value; }
        }

        [ConfigurationProperty("PlitaProgramName")]
        public string PlitaProgramName
        {
            get { return (string)this["PlitaProgramName"]; }
            set { this["PlitaProgramName"] = value; }
        }
    }
}

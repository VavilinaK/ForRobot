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

        [ConfigurationProperty("PlitaStringerGenerator")]
        public string PlitaStringerGenerator
        {
            get { return (string)this["PlitaStringerGenerator"]; }
            set { this["PlitaStringerGenerator"] = value; }
        }

        [ConfigurationProperty("PlitaStringerProgramName")]
        public string PlitaStringerProgramName
        {
            get { return (string)this["PlitaStringerProgramName"]; }
            set { this["PlitaStringerProgramName"] = value; }
        }

        [ConfigurationProperty("PlitaTreugolnikGenerator")]
        public string PlitaTreugolnikGenerator
        {
            get { return (string)this["PlitaTreugolnikGenerator"]; }
            set { this["PlitaTreugolnikGenerator"] = value; }
        }

        [ConfigurationProperty("PlitaTreugolnikProgramName")]
        public string PlitaTreugolnikProgramName
        {
            get { return (string)this["PlitaTreugolnikProgramName"]; }
            set { this["PlitaTreugolnikProgramName"] = value; }
        }
    }
}

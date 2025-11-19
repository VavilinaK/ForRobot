using System;
using System.Configuration;

namespace ForRobot.Libr.Configuration.ConfigurationProperties
{
    /// <summary>
    /// Класс для выбора стандартных свойств <see cref="ForRobot.Models.Robot"/> из app.config
    /// </summary>
    public class RobotConfigurationSection : BaseConfigurationSection
    {
        public string PathForGeneration => GetValue<string>("FolderOfGeneration");
        public string PathControllerFolder => GetValue<string>("ControlerFolder");
    }
}

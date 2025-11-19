using System;
using System.Configuration;

using ForRobot.Libr.Configuration.ConfigurationProperties;

namespace ForRobot.Libr.Configuration
{
    public class ConfigurationProvider : ForRobot.Libr.Services.Providers.IConfigurationProvider
    {
        public PlateConfigurationSection GetPlitaConfig() => this.SelectConfig("plate") as PlateConfigurationSection;

        public RobotConfigurationSection GetRobotConfig() => this.SelectConfig("robot") as RobotConfigurationSection;

        private System.Configuration.ConfigurationSection SelectConfig(string configName)
        {
            var config = ConfigurationManager.GetSection(configName);

            if (config == null)
                throw new ConfigurationErrorsException(string.Format("Конфигурация для '{0}' не найдена", configName));

            return config as System.Configuration.ConfigurationSection;
        }
    }
}

using System;

using ForRobot.Libr.Configuration.ConfigurationProperties;

namespace ForRobot.Libr.Services.Providers
{
    public interface IConfigurationProvider
    {
        PlateConfigurationSection GetPlitaConfig();
        PlateWeldingConfigurationSection GetPlateWeldingConfig();
    }
}

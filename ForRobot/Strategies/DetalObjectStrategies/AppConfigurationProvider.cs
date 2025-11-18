using System;
using System.Collections.Generic;
using System.Configuration;

using ForRobot.Libr.Services.Providers;

namespace ForRobot.Strategies.DetalObjectStrategies
{
    //public class AppConfigurationProvider : IConfigurationProvider
    //{
    //    private readonly IConfigurationProvider _innerProvider;
    //    private readonly object _lock = new object();
    //    private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

    //    public PlateConfigurationSection GetPlitaConfig() => this.SelectConfig("plate") as ForRobot.Libr.Xml.ConfigurationProperties.PlateConfigurationSection;

    //    public PlateWeldingConfigurationSection GetPlateWeldingConfig() => this.SelectConfig("plateWelding") as ForRobot.Libr.Xml.ConfigurationProperties.PlateWeldingConfigurationSection;

    //    private System.Configuration.ConfigurationSection SelectConfig(string configName)
    //    {
    //        var config = ConfigurationManager.GetSection(configName);

    //        if (config == null)
    //            throw new ConfigurationErrorsException(string.Format("Конфигурация для '{0}' не найдена", configName));

    //        return config as System.Configuration.ConfigurationSection;
    //    }

    //    private T GetCached<T>(string key, Func<T> factory) where T : class
    //    {
    //        lock (_lock)
    //        {
    //            if (!_cache.ContainsKey(key))
    //            {
    //                _cache[key] = factory();
    //            }
    //            return (T)_cache[key];
    //        }
    //    }
    //}
}

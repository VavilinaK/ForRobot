using System;
using System.Configuration;

namespace ForRobot.Libr.Configuration.ConfigurationProperties
{
    public abstract class BaseConfigurationSection : ConfigurationSection
    {
        protected T GetValue<T>(string propertyName, T defaultValue = default(T))
        {
            try
            {
                var value = this[propertyName];
                return value != null ? (T)value : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}

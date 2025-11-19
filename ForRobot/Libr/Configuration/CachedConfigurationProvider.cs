using System;
using System.Collections.Generic;

using ForRobot.Libr.Configuration.ConfigurationProperties;
using ForRobot.Libr.Services.Providers;

namespace ForRobot.Libr.Configuration
{
    public class CachedConfigurationProvider : IConfigurationProvider
    {
        private readonly IConfigurationProvider _innerProvider;
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
        private readonly object _lock = new object();

        public CachedConfigurationProvider(IConfigurationProvider innerProvider)
        {
            _innerProvider = innerProvider ?? throw new ArgumentNullException(nameof(innerProvider));
        }

        public PlateConfigurationSection GetPlitaConfig() => GetCached("plate", () => _innerProvider.GetPlitaConfig());

        public RobotConfigurationSection GetRobotConfig() => GetCached("robot", () => _innerProvider.GetRobotConfig());

        private T GetCached<T>(string key, Func<T> factory) where T : class
        {
            lock (_lock)
            {
                if (!_cache.ContainsKey(key))
                {
                    _cache[key] = factory();
                }
                return (T)_cache[key];
            }
        }

        public void ClearCache()
        {
            lock (_lock)
            {
                _cache.Clear();
            }
        }
    }
}

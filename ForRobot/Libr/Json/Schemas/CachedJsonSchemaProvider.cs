using System;
using System.Collections.Generic;

using ForRobot.Libr.Services.Providers;

namespace ForRobot.Libr.Json.Schemas
{
    /// <summary>
    /// Кэшерованная коллекция json-схем для разных видом <see cref="ForRobot.Models.Detals.DetalType"/>
    /// </summary>
    public class CachedJsonSchemaProvider : IJsonSchemaProvider
    {
        private readonly IJsonSchemaProvider _innerProvider;
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
        private readonly object _lock = new object();

        public CachedJsonSchemaProvider(IJsonSchemaProvider innerProvider)
        {
            _innerProvider = innerProvider ?? throw new ArgumentNullException(nameof(innerProvider));
        }

        public PlateJsonSchemaSection GetPlitaJsonSchema()
        {
            throw new NotImplementedException();
        }

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

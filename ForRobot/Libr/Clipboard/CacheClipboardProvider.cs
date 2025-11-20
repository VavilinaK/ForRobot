using System;
using System.Collections.Generic;

using ForRobot.Libr.Clipboard.UndoRedo;

namespace ForRobot.Libr.Clipboard
{
    public class CacheClipboardProvider
    {
        //private readonly IConfigurationProvider _innerProvider;
        private readonly Dictionary<string, Tuple<Stack<IUndoableCommand>, Stack<IUndoableCommand>>> _cache = new Dictionary<string, Tuple<Stack<IUndoableCommand>, Stack<IUndoableCommand>>>();
        private readonly object _lock = new object();

        public T GetOrAdd<T>(string key, Func<T> factory, CacheItemPolicy policy = null) where T : class
        {
            T item = (T)_cache.Get(key);

            return item;
        }

        public void ClearCache()
        {
            lock (_lock)
            {
                //_cache.Clear();
            }
        }
    }
}

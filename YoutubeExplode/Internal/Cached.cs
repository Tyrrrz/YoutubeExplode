using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace YoutubeExplode.Internal
{
    internal abstract class Cached
    {
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

        protected T Cache<T>(Func<T> resolver, [CallerMemberName] string key = "")
        {
            // If the value is in cache - return it
            if (_cache.TryGetValue(key, out var cachedValue))
                return (T) cachedValue;

            // Resolve the value
            var value = resolver();

            // Save the value to cache
            _cache[key] = value;

            return value;
        }
    }
}
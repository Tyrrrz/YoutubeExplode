using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace YoutubeExplode.Internal.Decoders
{
    internal abstract class DecoderBase
    {
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

        protected T Cache<T>(Func<T> resolver, [CallerMemberName] string key = "")
        {
            // If value is cached - return
            if (_cache.TryGetValue(key, out var cached))
                return (T) cached;

            // Otherwise - resolve, save to cache and return
            var resolved = resolver();
            _cache[key] = resolved;

            return resolved;
        }
    }
}
using System;
using System.Collections.Generic;

namespace YoutubeExplode.Utils
{
    // Helper utility used to cache the result of a function
    internal class Memo
    {
        private readonly Dictionary<string, object?> _cachedValues = new(StringComparer.Ordinal);

        private T Wrap<T>(string key, Func<T> getValue)
        {
            if (_cachedValues.TryGetValue(key, out var cachedValue) &&
                cachedValue is T convertedCachedValue)
            {
                return convertedCachedValue;
            }

            var value = getValue();

            _cachedValues[key] = value;

            return value;
        }

        public T Wrap<T>(Func<T> getValue) => Wrap("Auto_" + getValue.Method.GetHashCode(), getValue);
    }
}
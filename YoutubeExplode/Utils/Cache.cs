using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YoutubeExplode.Utils
{
    internal class Cache
    {
        private readonly Dictionary<string, object?> _cachedValues = new(StringComparer.Ordinal);

        public async ValueTask<T> WrapAsync<T>(string key, Func<ValueTask<T>> getValueAsync)
        {
            if (_cachedValues.TryGetValue(key, out var cachedValue) &&
                cachedValue is T convertedCachedValue)
            {
                return convertedCachedValue;
            }

            var value = await getValueAsync();

            _cachedValues[key] = value;

            return value;
        }

        public async ValueTask<T> WrapAsync<T>(Func<ValueTask<T>> getValueAsync) =>
            await WrapAsync("Auto_" + getValueAsync.GetHashCode(), getValueAsync);
    }
}
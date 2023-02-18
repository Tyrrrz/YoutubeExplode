// ReSharper disable CheckNamespace

#if NETSTANDARD2_0 || NET461
using System;
using System.Collections.Generic;

internal static class CollectionPolyfills
{
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
    {
        key = pair.Key;
        value = pair.Value;
    }

    public static string[] Split(this string input, params string[] separators) =>
        input.Split(separators, StringSplitOptions.RemoveEmptyEntries);
}

namespace System.Collections.Generic
{
    internal static class CollectionPolyfills
    {
        public static TValue GetValueOrDefault<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> dic,
            TKey key) =>
            dic.TryGetValue(key!, out var result) ? result! : default!;
    }
}
#endif
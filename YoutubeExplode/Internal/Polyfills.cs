// ReSharper disable CheckNamespace

// Polyfills to bridge the missing APIs in older versions of the framework/standard.

#if NETSTANDARD2_0 || NET461
namespace System
{
    internal static class Extensions
    {
        public static string[] Split(this string input, params string[] separators) =>
            input.Split(separators, StringSplitOptions.RemoveEmptyEntries);
    }
}

namespace System.Collections.Generic
{
    internal static class Extensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key) =>
            dic.TryGetValue(key, out var result) ? result! : default!;

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source) => new HashSet<T>(source);
    }
}

namespace System.IO
{
    using Threading;
    using Threading.Tasks;

    internal static class Extensions
    {
        public static async Task<int> ReadAsync(this Stream stream, byte[] buffer, CancellationToken cancellationToken) =>
            await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
    }
}
#endif
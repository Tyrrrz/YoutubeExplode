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

    internal static class HashCode
    {
        public static int Combine<T>(T value) => value?.GetHashCode() ?? 0;

        public static int Combine<T1, T2>(T1 value1, T2 value2)
        {
            var h1 = value1?.GetHashCode() ?? 0;
            var h2 = value2?.GetHashCode() ?? 0;
            var rol5 = ((uint) h1 << 5) | ((uint) h1 >> 27);
            return ((int) rol5 + h1) ^ h2;
        }
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
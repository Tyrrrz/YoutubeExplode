// ReSharper disable CheckNamespace
// ReSharper disable RedundantUsingDirective

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#if NETSTANDARD2_0 || NET461
internal static partial class PolyfillExtensions
{
    public static bool Contains(
        this string str,
        string subStr,
        StringComparison comparison = StringComparison.Ordinal) =>
        str.IndexOf(subStr, comparison) >= 0;

    public static string[] Split(this string input, params string[] separators) =>
        input.Split(separators, StringSplitOptions.RemoveEmptyEntries);
}

namespace System
{
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
    internal static class PolyfillExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> dic,
            TKey key) =>
            dic.TryGetValue(key!, out var result) ? result! : default!;

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source) => new(source);
    }
}

internal static partial class PolyfillExtensions
{
    public static async Task<int> ReadAsync(this Stream stream, byte[] buffer, CancellationToken cancellationToken) =>
        await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
}
#endif

#if !NET5_0
internal static partial class PolyfillExtensions
{
    public static async Task<string> ReadAsStringAsync(
        this HttpContent httpContent,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await httpContent.ReadAsStringAsync();
    }
}
#endif
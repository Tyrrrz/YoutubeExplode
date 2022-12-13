// ReSharper disable CheckNamespace

#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET461
using System.Collections.Generic;
using System.Linq;

internal static class CollectionPolyfills
{
    public static IEnumerable<(TFirst left, TSecond right)> Zip<TFirst, TSecond>(
        this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second) =>
        first.Zip(second, (x, y) => (x, y));
}
#endif
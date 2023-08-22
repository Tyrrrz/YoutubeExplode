using System.Collections.Generic;

namespace YoutubeExplode.Converter.Utils.Extensions;

internal static class CollectionExtensions
{
    public static IEnumerable<(T value, int index)> WithIndex<T>(this IEnumerable<T> source)
    {
        var i = 0;
        foreach (var o in source)
            yield return (o, i++);
    }
}

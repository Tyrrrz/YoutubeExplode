using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Utils.Extensions;

internal static class CollectionExtensions
{
    public static IEnumerable<(T value, int index)> WithIndex<T>(this IEnumerable<T> source)
    {
        var i = 0;
        foreach (var o in source)
            yield return (o, i++);
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class
    {
        foreach (var i in source)
        {
            if (i is not null)
                yield return i;
        }
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : struct
    {
        foreach (var i in source)
        {
            if (i is not null)
                yield return i.Value;
        }
    }

    public static T? ElementAtOrNull<T>(this IEnumerable<T> source, int index) where T : struct
    {
        var sourceAsList = source as IReadOnlyList<T> ?? source.ToArray();
        return index < sourceAsList.Count
            ? sourceAsList[index]
            : null;
    }

    public static T? FirstOrNull<T>(this IEnumerable<T> source) where T : struct
    {
        foreach (var i in source)
            return i;

        return null;
    }
}
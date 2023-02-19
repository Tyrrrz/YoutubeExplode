using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace YoutubeExplode.Utils.Extensions;

internal static class AsyncCollectionExtensions
{
    public static async IAsyncEnumerable<T> TakeAsync<T>(this IAsyncEnumerable<T> source, int count)
    {
        var currentCount = 0;

        await foreach (var i in source)
        {
            if (currentCount >= count)
                yield break;

            yield return i;
            currentCount++;
        }
    }

    public static async IAsyncEnumerable<T> SelectManyAsync<TSource, T>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, IEnumerable<T>> transform)
    {
        await foreach (var i in source)
        {
            foreach (var j in transform(i))
                yield return j;
        }
    }

    public static async IAsyncEnumerable<T> OfTypeAsync<T>(this IAsyncEnumerable<object> source)
    {
        await foreach (var i in source)
        {
            if (i is T match)
                yield return match;
        }
    }

    public static async ValueTask<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        var list = new List<T>();

        await foreach (var i in source)
            list.Add(i);

        return list;
    }

    public static ValueTaskAwaiter<List<T>> GetAwaiter<T>(this IAsyncEnumerable<T> source) =>
        source.ToListAsync().GetAwaiter();
}
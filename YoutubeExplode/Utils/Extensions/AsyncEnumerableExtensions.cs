using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YoutubeExplode.Utils.Extensions
{
    internal static class AsyncEnumerableExtensions
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

        public static async IAsyncEnumerable<T> OfType<TSource, T>(this IAsyncEnumerable<TSource> source)
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
    }
}
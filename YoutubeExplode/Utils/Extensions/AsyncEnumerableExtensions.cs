using System.Collections.Generic;
using System.Threading.Tasks;

namespace YoutubeExplode.Utils.Extensions
{
    internal static class AsyncEnumerableExtensions
    {
        public static async IAsyncEnumerable<T> TakeAsync<T>(this IAsyncEnumerable<T> asyncEnumerable, int count)
        {
            var currentCount = 0;

            await foreach (var i in asyncEnumerable)
            {
                if (currentCount >= count)
                    yield break;

                yield return i;
                currentCount++;
            }
        }

        public static async ValueTask<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            var list = new List<T>();

            await foreach (var i in asyncEnumerable)
                list.Add(i);

            return list;
        }
    }
}
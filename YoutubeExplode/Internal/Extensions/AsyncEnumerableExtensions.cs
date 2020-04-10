using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace YoutubeExplode.Internal.Extensions
{
    internal static class AsyncEnumerableExtensions
    {
        public static async IAsyncEnumerable<T> TakeAsync<T>(this IAsyncEnumerable<T> asyncEnumerable, int count)
        {
            var current = 0;

            await foreach (var i in asyncEnumerable)
            {
                if (current >= count)
                    yield break;

                yield return i;
                current++;
            }
        }

        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            var list = new List<T>();

            await foreach (var i in asyncEnumerable)
                list.Add(i);

            return list;
        }
    }
}
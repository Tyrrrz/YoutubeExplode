using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace YoutubeExplode.Utils.Extensions;

internal static class AsyncCollectionExtensions
{
    extension<T>(IAsyncEnumerable<T> source)
    {
        public async IAsyncEnumerable<T> TakeAsync(int count)
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

        public async IAsyncEnumerable<T1> SelectManyAsync<T1>(Func<T, IEnumerable<T1>> transform)
        {
            await foreach (var i in source)
            {
                foreach (var j in transform(i))
                    yield return j;
            }
        }

        public async ValueTask<List<T>> ToListAsync()
        {
            var list = new List<T>();

            await foreach (var i in source)
                list.Add(i);

            return list;
        }

        public ValueTaskAwaiter<List<T>> GetAwaiter() => source.ToListAsync().GetAwaiter();
    }

    extension(IAsyncEnumerable<object> source)
    {
        public async IAsyncEnumerable<T> OfTypeAsync<T>()
        {
            await foreach (var i in source)
            {
                if (i is T match)
                    yield return match;
            }
        }
    }
}

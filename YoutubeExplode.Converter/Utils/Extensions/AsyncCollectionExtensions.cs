using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace YoutubeExplode.Converter.Utils.Extensions;

internal static class AsyncCollectionExtensions
{
    extension<T>(IAsyncEnumerable<T> source)
    {
        public async ValueTask<List<T>> ToListAsync()
        {
            var list = new List<T>();

            await foreach (var i in source)
                list.Add(i);

            return list;
        }

        public ValueTaskAwaiter<List<T>> GetAwaiter() => source.ToListAsync().GetAwaiter();
    }
}

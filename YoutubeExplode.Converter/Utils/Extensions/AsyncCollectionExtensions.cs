using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace YoutubeExplode.Converter.Utils.Extensions;

internal static class AsyncCollectionExtensions
{
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

using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Common;

internal static class BatchExtensions
{
    extension<T>(IAsyncEnumerable<Batch<T>> source)
        where T : IBatchItem
    {
        public IAsyncEnumerable<T> FlattenAsync() => source.SelectMany(b => b.Items);
    }
}

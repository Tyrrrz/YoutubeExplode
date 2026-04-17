using System.Collections.Generic;

namespace YoutubeExplode.Utils.Extensions;

internal static class AsyncCollectionExtensions
{
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

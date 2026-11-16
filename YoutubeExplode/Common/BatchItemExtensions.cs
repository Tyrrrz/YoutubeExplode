using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace YoutubeExplode.Common;

/// <summary>
/// Extensions for <see cref="IBatchItem" />.
/// </summary>
public static class BatchItemExtensions
{
    // We want to enable some convenience methods on instances of IAsyncEnumerable<T>
    // exposed by the library.
    // However, we don't want these extensions to apply to other IAsyncEnumerable<T>
    // as that could cause unwanted noise for the user.
    // To that end, we use a marker interface and a generic constraint to limit the
    // set of types that these extension methods can be used on.

    /// <inheritdoc cref="BatchItemExtensions" />
    extension<T>(IAsyncEnumerable<T> source)
        where T : IBatchItem
    {
        /// <summary>
        /// Enumerates all items in the sequence and buffers them in memory.
        /// </summary>
        public async ValueTask<IReadOnlyList<T>> CollectAsync() => await source.ToListAsync();

        /// <summary>
        /// Enumerates a subset of items in the sequence and buffers them in memory.
        /// </summary>
        public async ValueTask<IReadOnlyList<T>> CollectAsync(int count) =>
            await source.Take(count).ToListAsync();

        /// <summary>
        /// Enumerates all items in the sequence and buffers them in memory.
        /// </summary>
        public ValueTaskAwaiter<IReadOnlyList<T>> GetAwaiter() =>
            source.CollectAsync().GetAwaiter();
    }
}

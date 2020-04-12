using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode.Internal.Extensions;
using YoutubeExplode.Videos;

namespace YoutubeExplode
{
    /// <summary>
    /// Extensions to make working with <see cref="YoutubeExplode"/> a bit more comfortable.
    /// </summary>
    public static class AccessibilityExtensions
    {
        /// <summary>
        /// Buffers the asynchronous enumerable in memory.
        /// </summary>
        public static async Task<IReadOnlyList<Video>> BufferAsync(this IAsyncEnumerable<Video> asyncVideoEnumerable) =>
            await asyncVideoEnumerable.ToListAsync();

        /// <summary>
        /// Buffers the asynchronous enumerable in memory, up to the specified number of videos.
        /// </summary>
        public static async Task<IReadOnlyList<Video>> BufferAsync(this IAsyncEnumerable<Video> asyncVideoEnumerable, int count) =>
            await asyncVideoEnumerable.TakeAsync(count).BufferAsync();
    }
}
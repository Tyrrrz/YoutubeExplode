using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Playlists
{
    /// <summary>
    /// Enumerable extensions for playlists.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Buffers the asynchronous list of playlist videos in memory.
        /// </summary>
        public static async ValueTask<IReadOnlyList<PlaylistVideoBatch>> BufferAsync(
            this IAsyncEnumerable<PlaylistVideoBatch> asyncVideoEnumerable) =>
            await asyncVideoEnumerable.ToListAsync();

        /// <summary>
        /// Buffers the asynchronous list of playlist videos in memory, up to the specified number of videos.
        /// </summary>
        public static async ValueTask<IReadOnlyList<PlaylistVideoBatch>> BufferAsync(
            this IAsyncEnumerable<PlaylistVideoBatch> asyncVideoEnumerable,
            int count) =>
            await asyncVideoEnumerable.TakeAsync(count).BufferAsync();

        /// <summary>
        /// Gets the awaiter that encapsulates an operation that buffers a list of playlist videos in-memory.
        /// </summary>
        public static ValueTaskAwaiter<IReadOnlyList<PlaylistVideoBatch>> GetAwaiter(
            this IAsyncEnumerable<PlaylistVideoBatch> asyncVideoEnumerable) =>
            asyncVideoEnumerable.BufferAsync().GetAwaiter();

        /// <summary>
        /// Buffers the asynchronous list of playlist videos in memory.
        /// </summary>
        public static async ValueTask<IReadOnlyList<PlaylistVideo>> BufferAsync(
            this IAsyncEnumerable<PlaylistVideo> asyncVideoEnumerable) =>
            await asyncVideoEnumerable.ToListAsync();

        /// <summary>
        /// Buffers the asynchronous list of playlist videos in memory, up to the specified number of videos.
        /// </summary>
        public static async ValueTask<IReadOnlyList<PlaylistVideo>> BufferAsync(
            this IAsyncEnumerable<PlaylistVideo> asyncVideoEnumerable,
            int count) =>
            await asyncVideoEnumerable.TakeAsync(count).BufferAsync();

        /// <summary>
        /// Gets the awaiter that encapsulates an operation that buffers a list of playlist videos in-memory.
        /// </summary>
        public static ValueTaskAwaiter<IReadOnlyList<PlaylistVideo>> GetAwaiter(
            this IAsyncEnumerable<PlaylistVideo> asyncVideoEnumerable) =>
            asyncVideoEnumerable.BufferAsync().GetAwaiter();
    }
}
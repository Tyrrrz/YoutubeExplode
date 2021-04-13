using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Search
{
    /// <summary>
    /// Enumerable extensions for search results.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Buffers the asynchronous list of search results in memory.
        /// </summary>
        public static async ValueTask<IReadOnlyList<SearchResultBatch>> BufferAsync(
            this IAsyncEnumerable<SearchResultBatch> asyncVideoEnumerable) =>
            await asyncVideoEnumerable.ToListAsync();

        /// <summary>
        /// Buffers the asynchronous list of search results in memory, up to the specified number of items.
        /// </summary>
        public static async ValueTask<IReadOnlyList<SearchResultBatch>> BufferAsync(
            this IAsyncEnumerable<SearchResultBatch> asyncVideoEnumerable,
            int count) =>
            await asyncVideoEnumerable.TakeAsync(count).BufferAsync();

        /// <summary>
        /// Gets the awaiter that encapsulates an operation that buffers a list of search results in-memory.
        /// </summary>
        public static ValueTaskAwaiter<IReadOnlyList<SearchResultBatch>> GetAwaiter(
            this IAsyncEnumerable<SearchResultBatch> asyncVideoEnumerable) =>
            asyncVideoEnumerable.BufferAsync().GetAwaiter();

        /// <summary>
        /// Buffers the asynchronous list of search results in memory.
        /// </summary>
        public static async ValueTask<IReadOnlyList<ISearchResult>> BufferAsync(
            this IAsyncEnumerable<ISearchResult> asyncVideoEnumerable) =>
            await asyncVideoEnumerable.ToListAsync();

        /// <summary>
        /// Buffers the asynchronous list of search results in memory, up to the specified number of items.
        /// </summary>
        public static async ValueTask<IReadOnlyList<ISearchResult>> BufferAsync(
            this IAsyncEnumerable<ISearchResult> asyncVideoEnumerable,
            int count) =>
            await asyncVideoEnumerable.TakeAsync(count).BufferAsync();

        /// <summary>
        /// Gets the awaiter that encapsulates an operation that buffers a list of search results in-memory.
        /// </summary>
        public static ValueTaskAwaiter<IReadOnlyList<ISearchResult>> GetAwaiter(
            this IAsyncEnumerable<ISearchResult> asyncVideoEnumerable) =>
            asyncVideoEnumerable.BufferAsync().GetAwaiter();
    }
}
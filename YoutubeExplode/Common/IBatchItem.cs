using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Common;

/// <summary>
/// Represents an item that can be included in <see cref="Batch{T}" />.
/// This interface is used as a marker to enable extension methods.
/// </summary>
public interface IBatchItem
{
}

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

    /// <summary>
    /// Enumerates all items in the sequence and buffers them in memory.
    /// </summary>
    public static async ValueTask<IReadOnlyList<T>> CollectAsync<T>(
        this IAsyncEnumerable<T> source) where T : IBatchItem => await source.ToListAsync();

    /// <summary>
    /// Enumerates a subset of items in the sequence and buffers them in memory.
    /// </summary>
    public static async ValueTask<IReadOnlyList<T>> CollectAsync<T>(
        this IAsyncEnumerable<T> source,
        int count) where T : IBatchItem => await source.TakeAsync(count).ToListAsync();

    /// <inheritdoc cref="CollectAsync{T}(System.Collections.Generic.IAsyncEnumerable{T})" />
    public static ValueTaskAwaiter<IReadOnlyList<T>> GetAwaiter<T>(
        this IAsyncEnumerable<T> source) where T : IBatchItem => source.CollectAsync().GetAwaiter();
}
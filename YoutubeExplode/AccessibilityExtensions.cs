using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode.Internal.Extensions;
using YoutubeExplode.Videos;

namespace YoutubeExplode
{
    public static class AccessibilityExtensions
    {
        public static async Task<IReadOnlyList<Video>> BufferAsync(this IAsyncEnumerable<Video> asyncVideoEnumerable, int count) =>
            await asyncVideoEnumerable.TakeAsync(count).ToListAsync();

        public static async Task<IReadOnlyList<Video>> BufferAsync(this IAsyncEnumerable<Video> asyncVideoEnumerable) =>
            await asyncVideoEnumerable.ToListAsync();
    }
}
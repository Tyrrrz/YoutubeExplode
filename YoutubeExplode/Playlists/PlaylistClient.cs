using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode.Common;
using YoutubeExplode.Internal.Extensions;
using YoutubeExplode.ReverseEngineering;
using YoutubeExplode.ReverseEngineering.Responses;

namespace YoutubeExplode.Playlists
{
    /// <summary>
    /// Queries related to YouTube playlists.
    /// </summary>
    public class PlaylistClient
    {
        private readonly YoutubeHttpClient _httpClient;

        /// <summary>
        /// Initializes an instance of <see cref="PlaylistClient"/>.
        /// </summary>
        internal PlaylistClient(YoutubeHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Gets the metadata associated with the specified playlist.
        /// </summary>
        public async Task<Playlist> GetAsync(PlaylistId id)
        {
            var response = await PlaylistResponse.GetAsync(_httpClient, id);

            var thumbnails = response
                .GetPlaylistVideos()
                .FirstOrDefault()?
                .GetId()
                .Pipe(i => new ThumbnailSet(i));

            return new Playlist(
                id,
                response.TryGetTitle() ?? "",
                response.TryGetAuthor() ?? "",
                response.TryGetDescription() ?? "",
                response.TryGetViewCount() ?? 0,
                thumbnails
                );
        }

        /// <summary>
        /// Enumerates videos included in the specified playlist.
        /// </summary>
        public async IAsyncEnumerable<PlaylistVideo> GetVideosAsync(PlaylistId id)
        {
            var encounteredVideoIds = new HashSet<string>();

            var index = 0;
            while (true)
            {
                var response = await PlaylistResponse.GetAsync(_httpClient, id, index);

                var countDelta = 0;
                foreach (var video in response.GetPlaylistVideos())
                {
                    var videoId = video.GetId();

                    // Skip already encountered videos
                    if (!encounteredVideoIds.Add(videoId))
                        continue;

                    // Skip deleted videos
                    if (string.IsNullOrEmpty(video.GetChannelId()))
                        continue;

                    yield return new PlaylistVideo(
                        videoId,
                        video.GetTitle(),
                        video.GetAuthor(),
                        video.GetChannelId(),
                        video.GetDescription(),
                        video.GetDuration(),
                        video.GetViewCount(),
                        new ThumbnailSet(videoId)
                    );

                    countDelta++;
                }

                // Videos loop around, so break when we stop seeing new videos
                if (countDelta <= 0)
                    break;

                index += countDelta;
            }
        }
    }
}
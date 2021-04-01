using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using YoutubeExplode.Common;
using YoutubeExplode.ReverseEngineering;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Playlists
{
    /// <summary>
    /// Queries related to YouTube playlists.
    /// </summary>
    public class PlaylistClient
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes an instance of <see cref="PlaylistClient"/>.
        /// </summary>
        public PlaylistClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Gets the metadata associated with the specified playlist.
        /// </summary>
        public async ValueTask<Playlist> GetAsync(PlaylistId playlistId)
        {
            var response = await PlaylistResponse.GetAsync(_httpClient, playlistId);

            var thumbnails = response
                .GetPlaylistVideos()
                .FirstOrDefault()?
                .GetId()
                .Pipe(i => new ThumbnailSet(i));

            return new Playlist(
                playlistId,
                response.TryGetTitle() ?? "",
                response.TryGetAuthor() ?? "",
                response.TryGetDescription() ?? "",
                response.TryGetViewCount() ?? 0,
                thumbnails
                );
        }

        /// <summary>
        /// Enumerates the videos included in the specified playlist.
        /// </summary>
        public async IAsyncEnumerable<PlaylistVideo> GetVideosAsync(PlaylistId playlistId)
        {
            var encounteredVideoIds = new HashSet<string>();
            var continuationToken = "";

            while (true)
            {
                var response = await PlaylistResponse.GetAsync(_httpClient, playlistId, continuationToken);

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
                }

                continuationToken = response.TryGetContinuationToken();
                if (string.IsNullOrEmpty(continuationToken))
                    break;
            }
        }
    }
}
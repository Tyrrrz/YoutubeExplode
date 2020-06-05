using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode.Common;
using YoutubeExplode.ReverseEngineering;
using YoutubeExplode.ReverseEngineering.Responses;
using YoutubeExplode.Videos;

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

            return new Playlist(
                id,
                response.GetTitle(),
                response.TryGetAuthor(),
                response.TryGetDescription() ?? "", new Engagement(
                    response.TryGetViewCount() ?? 0,
                    response.TryGetLikeCount() ?? 0,
                    response.TryGetDislikeCount() ?? 0
                ));
        }

        /// <summary>
        /// Enumerates videos included in the specified playlist.
        /// </summary>
        public async IAsyncEnumerable<Video> GetVideosAsync(PlaylistId id)
        {
            var encounteredVideoIds = new HashSet<string>();

            var index = 0;
            while (true)
            {
                var response = await PlaylistResponse.GetAsync(_httpClient, id, index);

                var countDelta = 0;
                foreach (var video in response.GetVideos())
                {
                    var videoId = video.GetId();

                    // Skip already encountered videos
                    if (!encounteredVideoIds.Add(videoId))
                        continue;

                    yield return new Video(
                        videoId,
                        video.GetTitle(),
                        video.GetAuthor(),
                        video.GetUploadDate(),
                        video.GetDescription(),
                        video.GetDuration(),
                        new ThumbnailSet(videoId),
                        video.GetKeywords(),
                        new Engagement(
                            video.GetViewCount(),
                            video.GetLikeCount(),
                            video.GetDislikeCount()
                        )
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
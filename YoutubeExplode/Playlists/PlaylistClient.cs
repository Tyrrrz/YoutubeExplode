using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using YoutubeExplode.Common;
using YoutubeExplode.ReverseEngineering.Responses;
using YoutubeExplode.Videos;

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
        public async Task<Playlist> GetAsync(PlaylistId id)
        {
            var response = await PlaylistResponse.GetAsync(_httpClient, id);

            return new Playlist(
                id,
                response.TryGetAuthor(),
                response.GetTitle(),
                response.TryGetDescription() ?? "",
                new Engagement(
                    response.TryGetViewCount() ?? 0,
                    response.TryGetLikeCount() ?? 0,
                    response.TryGetDislikeCount() ?? 0
                )
            );
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

                    // Yield the video if it's distinct
                    if (!encounteredVideoIds.Add(videoId))
                        continue;

                    yield return new Video(
                        videoId,
                        video.GetTitle(),
                        video.GetAuthor(),
                        video.GetUploadDate(),
                        video.GetDescription(), // TODO
                        video.GetDuration(),
                        Array.Empty<Thumbnail>(),
                        video.GetKeywords(),
                        new Engagement(
                            video.GetViewCount(),
                            video.GetLikeCount(),
                            video.GetDislikeCount()
                        ));

                    countDelta++;
                }

                // If no distinct videos were added to the list - break
                if (countDelta <= 0)
                    break;

                index += countDelta;
            }
        }
    }
}
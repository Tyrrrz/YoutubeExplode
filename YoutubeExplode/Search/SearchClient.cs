using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using YoutubeExplode.Bridge;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Playlists;

namespace YoutubeExplode.Search
{
    /// <summary>
    /// Operations related to YouTube search.
    /// </summary>
    public class SearchClient
    {
        private readonly YoutubeController _controller;

        /// <summary>
        /// Initializes an instance of <see cref="SearchClient"/>.
        /// </summary>
        public SearchClient(HttpClient httpClient)
        {
            _controller = new YoutubeController(httpClient);
        }

        /// <summary>
        /// Enumerates the videos returned by the specified search query.
        /// </summary>
        public async IAsyncEnumerable<PlaylistVideo> GetVideosAsync(
            string searchQuery,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var encounteredVideoIds = new HashSet<string>(StringComparer.Ordinal);
            var continuationToken = default(string?);

            do
            {
                var searchResults =
                    await _controller.GetSearchResultsAsync(searchQuery, continuationToken, cancellationToken);

                foreach (var videoExtractor in searchResults.GetVideos())
                {
                    var id =
                        videoExtractor.TryGetVideoId() ??
                        throw new YoutubeExplodeException("Could not extract video ID.");

                    // Don't yield the same video twice
                    if (!encounteredVideoIds.Add(id))
                        continue;

                    var title =
                        videoExtractor.TryGetVideoTitle() ??
                        throw new YoutubeExplodeException("Could not extract video title.");

                    var author =
                        videoExtractor.TryGetVideoAuthor() ??
                        throw new YoutubeExplodeException("Could not extract video author.");

                    var channelId =
                        videoExtractor.TryGetVideoChannelId() ??
                        throw new YoutubeExplodeException("Could not extract video channel ID.");

                    var description = videoExtractor.TryGetVideoDescription() ?? "";

                    var duration = videoExtractor.TryGetVideoDuration();

                    var viewCount = videoExtractor.TryGetVideoViewCount() ?? 0;

                    var thumbnails = new List<Thumbnail>();

                    thumbnails.AddRange(Thumbnail.GetDefaultSet(id));

                    foreach (var thumbnailExtractor in videoExtractor.GetVideoThumbnails())
                    {
                        var thumbnailUrl =
                            thumbnailExtractor.TryGetUrl() ??
                            throw new YoutubeExplodeException("Could not extract thumbnail URL.");

                        var thumbnailWidth =
                            thumbnailExtractor.TryGetWidth() ??
                            throw new YoutubeExplodeException("Could not extract thumbnail width.");

                        var thumbnailHeight =
                            thumbnailExtractor.TryGetHeight() ??
                            throw new YoutubeExplodeException("Could not extract thumbnail height.");

                        var thumbnailResolution = new Resolution(thumbnailWidth, thumbnailHeight);

                        var thumbnail = new Thumbnail(thumbnailUrl, thumbnailResolution);

                        thumbnails.Add(thumbnail);
                    }

                    var video = new PlaylistVideo(
                        id,
                        title,
                        author,
                        channelId,
                        description,
                        duration,
                        viewCount,
                        thumbnails
                    );

                    yield return video;
                }

                continuationToken = searchResults.TryGetContinuationToken();
            } while (!string.IsNullOrWhiteSpace(continuationToken));
        }
    }
}
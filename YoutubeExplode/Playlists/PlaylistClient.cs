using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Playlists
{
    /// <summary>
    /// Operations related to YouTube playlists.
    /// </summary>
    public class PlaylistClient
    {
        private readonly YoutubeController _controller;

        internal PlaylistClient(YoutubeController controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Initializes an instance of <see cref="PlaylistClient"/>.
        /// </summary>
        public PlaylistClient(HttpClient httpClient)
            : this(new YoutubeController(httpClient))
        {
        }

        /// <summary>
        /// Gets the metadata associated with the specified playlist.
        /// </summary>
        public async ValueTask<Playlist> GetAsync(
            PlaylistId playlistId,
            CancellationToken cancellationToken = default)
        {
            var playlistExtractor = await _controller.GetPlaylistAsync(playlistId, cancellationToken);

            var title =
                playlistExtractor.TryGetPlaylistTitle() ??
                throw new YoutubeExplodeException("Could not extract playlist title.");

            // System playlists have no author
            var channelId = playlistExtractor.TryGetPlaylistChannelId();
            var channelTitle = playlistExtractor.TryGetPlaylistAuthor();

            var author = channelId is not null && channelTitle is not null
                ? new Author(channelId, channelTitle)
                : null;

            var description = playlistExtractor.TryGetPlaylistDescription() ?? "";

            var thumbnails = new List<Thumbnail>();

            foreach (var thumbnailExtractor in playlistExtractor.GetPlaylistThumbnails())
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

            return new Playlist(
                playlistId,
                title,
                author,
                description,
                thumbnails
            );
        }

        /// <summary>
        /// Enumerates batches of videos included in the specified playlist.
        /// Each batch represents one request.
        /// </summary>
        public async IAsyncEnumerable<Batch<PlaylistVideo>> GetVideoBatchesAsync(
            PlaylistId playlistId,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var encounteredVideoIds = new HashSet<string>(StringComparer.Ordinal);
            var continuationToken = default(string?);

            do
            {
                var playlistExtractor =
                    await _controller.GetPlaylistAsync(playlistId, continuationToken, cancellationToken);

                var videos = new List<PlaylistVideo>();

                foreach (var videoExtractor in playlistExtractor.GetVideos())
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

                    var channelTitle =
                        videoExtractor.TryGetVideoAuthor() ??
                        throw new YoutubeExplodeException("Could not extract video author.");

                    var channelId =
                        videoExtractor.TryGetVideoChannelId() ??
                        throw new YoutubeExplodeException("Could not extract video channel ID.");

                    var duration = videoExtractor.TryGetVideoDuration();

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
                        new Author(channelId, channelTitle),
                        duration,
                        thumbnails
                    );

                    videos.Add(video);
                }

                yield return Batch.Create(videos);

                continuationToken = playlistExtractor.TryGetContinuationToken();
            } while (!string.IsNullOrWhiteSpace(continuationToken));
        }

        /// <summary>
        /// Enumerates videos included in the specified playlist.
        /// </summary>
        public IAsyncEnumerable<PlaylistVideo> GetVideosAsync(
            PlaylistId playlistId,
            CancellationToken cancellationToken = default) =>
            GetVideoBatchesAsync(playlistId, cancellationToken).FlattenAsync();
    }
}
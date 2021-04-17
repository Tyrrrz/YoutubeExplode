using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using YoutubeExplode.Bridge;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils.Extensions;

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
        /// Enumerates batches of search results returned by the specified query.
        /// </summary>
        public async IAsyncEnumerable<Batch<ISearchResult>> GetResultBatchesAsync(
            string searchQuery,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var encounteredIds = new HashSet<string>(StringComparer.Ordinal);
            var continuationToken = default(string?);

            do
            {
                var results = new List<ISearchResult>();

                var searchResults =
                    await _controller.GetSearchResultsAsync(searchQuery, continuationToken, cancellationToken);

                foreach (var videoExtractor in searchResults.GetVideos())
                {
                    var id =
                        videoExtractor.TryGetVideoId() ??
                        throw new YoutubeExplodeException("Could not extract video ID.");

                    // Don't yield the same result twice
                    if (!encounteredIds.Add(id))
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

                    var video = new VideoSearchResult(
                        id,
                        title,
                        new Author(channelId, channelTitle),
                        duration,
                        thumbnails
                    );

                    results.Add(video);
                }

                foreach (var playlistExtractor in searchResults.GetPlaylists())
                {
                    var id =
                        playlistExtractor.TryGetPlaylistId() ??
                        throw new YoutubeExplodeException("Could not extract playlist ID.");

                    // Don't yield the same result twice
                    if (!encounteredIds.Add(id))
                        continue;

                    var title =
                        playlistExtractor.TryGetPlaylistTitle() ??
                        throw new YoutubeExplodeException("Could not extract playlist title.");

                    // System playlists have no author
                    var channelId = playlistExtractor.TryGetPlaylistChannelId();
                    var channelTitle = playlistExtractor.TryGetPlaylistAuthor();

                    var author = channelId is not null && channelTitle is not null
                        ? new Author(channelId, channelTitle)
                        : null;

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

                    var playlist = new PlaylistSearchResult(
                        id,
                        title,
                        author,
                        thumbnails
                    );

                    results.Add(playlist);
                }

                foreach (var channelExtractor in searchResults.GetChannels())
                {
                    var channelId =
                        channelExtractor.TryGetChannelId() ??
                        throw new YoutubeExplodeException("Could not extract channel ID.");

                    var title =
                        channelExtractor.TryGetChannelTitle() ??
                        throw new YoutubeExplodeException("Could not extract channel title.");

                    var thumbnails = new List<Thumbnail>();

                    foreach (var thumbnailExtractor in channelExtractor.GetChannelThumbnails())
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

                    var channel = new ChannelSearchResult(
                        channelId,
                        title,
                        thumbnails
                    );

                    results.Add(channel);
                }

                yield return Batch.Create(results);

                continuationToken = searchResults.TryGetContinuationToken();
            } while (!string.IsNullOrWhiteSpace(continuationToken));
        }

        /// <summary>
        /// Enumerates search results returned by the specified query.
        /// </summary>
        public IAsyncEnumerable<ISearchResult> GetResultsAsync(
            string searchQuery,
            CancellationToken cancellationToken = default) =>
            GetResultBatchesAsync(searchQuery, cancellationToken).FlattenAsync();

        /// <summary>
        /// Enumerates video search results returned by the specified query.
        /// </summary>
        public IAsyncEnumerable<VideoSearchResult> GetVideosAsync(
            string searchQuery,
            CancellationToken cancellationToken = default) =>
            GetResultBatchesAsync(searchQuery, cancellationToken)
                .FlattenAsync()
                .OfType<ISearchResult, VideoSearchResult>();

        /// <summary>
        /// Enumerates playlist search results returned by the specified query.
        /// </summary>
        public IAsyncEnumerable<PlaylistSearchResult> GetPlaylistsAsync(
            string searchQuery,
            CancellationToken cancellationToken = default) =>
            GetResultBatchesAsync(searchQuery, cancellationToken)
                .FlattenAsync()
                .OfType<ISearchResult, PlaylistSearchResult>();

        /// <summary>
        /// Enumerates channel search results returned by the specified query.
        /// </summary>
        public IAsyncEnumerable<ChannelSearchResult> GetChannelsAsync(
            string searchQuery,
            CancellationToken cancellationToken = default) =>
            GetResultBatchesAsync(searchQuery, cancellationToken)
                .FlattenAsync()
                .OfType<ISearchResult, ChannelSearchResult>();
    }
}
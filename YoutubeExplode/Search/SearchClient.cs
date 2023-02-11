using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Search;

/// <summary>
/// Operations related to YouTube search.
/// </summary>
public class SearchClient
{
    private readonly SearchController _controller;

    /// <summary>
    /// Initializes an instance of <see cref="SearchClient" />.
    /// </summary>
    public SearchClient(HttpClient http) =>
        _controller = new SearchController(http);

    /// <summary>
    /// Enumerates batches of search results returned by the specified query.
    /// </summary>
    public async IAsyncEnumerable<Batch<ISearchResult>> GetResultBatchesAsync(
        string searchQuery,
        SearchFilter searchFilter,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var encounteredIds = new HashSet<string>(StringComparer.Ordinal);
        var continuationToken = default(string?);

        do
        {
            var results = new List<ISearchResult>();

            var searchResults = await _controller.GetSearchResultsAsync(
                searchQuery,
                searchFilter,
                continuationToken,
                cancellationToken
            );

            // Video results
            foreach (var videoExtractor in searchResults.GetVideos())
            {
                if (searchFilter is not SearchFilter.None and not SearchFilter.Video)
                {
                    Debug.Fail("Did not expect videos in search results.");
                    break;
                }

                var videoId =
                    videoExtractor.TryGetVideoId() ??
                    throw new YoutubeExplodeException("Could not extract video ID.");

                // Don't yield the same result twice
                if (!encounteredIds.Add(videoId))
                    continue;

                var videoTitle =
                    videoExtractor.TryGetVideoTitle() ??
                    throw new YoutubeExplodeException("Could not extract video title.");

                var videoChannelTitle =
                    videoExtractor.TryGetVideoAuthor() ??
                    throw new YoutubeExplodeException("Could not extract video author.");

                var videoChannelId =
                    videoExtractor.TryGetVideoChannelId() ??
                    throw new YoutubeExplodeException("Could not extract video channel ID.");

                var videoDuration = videoExtractor.TryGetVideoDuration();

                var videoThumbnails = videoExtractor
                    .GetVideoThumbnails()
                    .Select(t =>
                    {
                        var thumbnailUrl =
                            t.TryGetUrl() ??
                            throw new YoutubeExplodeException("Could not extract video thumbnail URL.");

                        var thumbnailWidth =
                            t.TryGetWidth() ??
                            throw new YoutubeExplodeException("Could not extract video thumbnail width.");

                        var thumbnailHeight =
                            t.TryGetHeight() ??
                            throw new YoutubeExplodeException("Could not extract video thumbnail height.");

                        var thumbnailResolution = new Resolution(thumbnailWidth, thumbnailHeight);

                        return new Thumbnail(thumbnailUrl, thumbnailResolution);
                    })
                    .Concat(Thumbnail.GetDefaultSet(videoId))
                    .ToArray();

                var video = new VideoSearchResult(
                    videoId,
                    videoTitle,
                    new Author(videoChannelId, videoChannelTitle),
                    videoDuration,
                    videoThumbnails
                );

                results.Add(video);
            }

            // Playlist results
            foreach (var playlistExtractor in searchResults.GetPlaylists())
            {
                if (searchFilter is not SearchFilter.None and not SearchFilter.Playlist)
                {
                    Debug.Fail("Did not expect playlists in search results.");
                    break;
                }

                var playlistId =
                    playlistExtractor.TryGetPlaylistId() ??
                    throw new YoutubeExplodeException("Could not extract playlist ID.");

                // Don't yield the same result twice
                if (!encounteredIds.Add(playlistId))
                    continue;

                var playlistTitle =
                    playlistExtractor.TryGetPlaylistTitle() ??
                    throw new YoutubeExplodeException("Could not extract playlist title.");

                // System playlists have no author
                var playlistChannelId = playlistExtractor.TryGetPlaylistChannelId();
                var playlistChannelTitle = playlistExtractor.TryGetPlaylistAuthor();
                var playlistAuthor = playlistChannelId is not null && playlistChannelTitle is not null
                    ? new Author(playlistChannelId, playlistChannelTitle)
                    : null;

                var playlistThumbnails = playlistExtractor
                    .GetPlaylistThumbnails()
                    .Select(t =>
                    {
                        var thumbnailUrl =
                            t.TryGetUrl() ??
                            throw new YoutubeExplodeException("Could not extract playlist thumbnail URL.");

                        var thumbnailWidth =
                            t.TryGetWidth() ??
                            throw new YoutubeExplodeException("Could not extract playlist thumbnail width.");

                        var thumbnailHeight =
                            t.TryGetHeight() ??
                            throw new YoutubeExplodeException("Could not extract playlist thumbnail height.");

                        var thumbnailResolution = new Resolution(thumbnailWidth, thumbnailHeight);

                        return new Thumbnail(thumbnailUrl, thumbnailResolution);
                    })
                    .ToArray();

                var playlist = new PlaylistSearchResult(
                    playlistId,
                    playlistTitle,
                    playlistAuthor,
                    playlistThumbnails
                );

                results.Add(playlist);
            }

            // Channel results
            foreach (var channelExtractor in searchResults.GetChannels())
            {
                if (searchFilter is not SearchFilter.None and not SearchFilter.Channel)
                {
                    Debug.Fail("Did not expect channels in search results.");
                    break;
                }

                var channelId =
                    channelExtractor.TryGetChannelId() ??
                    throw new YoutubeExplodeException("Could not extract channel ID.");

                var channelTitle =
                    channelExtractor.TryGetChannelTitle() ??
                    throw new YoutubeExplodeException("Could not extract channel title.");

                var channelThumbnails = channelExtractor
                    .GetChannelThumbnails()
                    .Select(t =>
                    {
                        var thumbnailUrl =
                            t.TryGetUrl() ??
                            throw new YoutubeExplodeException("Could not extract channel thumbnail URL.");

                        var thumbnailWidth =
                            t.TryGetWidth() ??
                            throw new YoutubeExplodeException("Could not extract channel thumbnail width.");

                        var thumbnailHeight =
                            t.TryGetHeight() ??
                            throw new YoutubeExplodeException("Could not extract channel thumbnail height.");

                        var thumbnailResolution = new Resolution(thumbnailWidth, thumbnailHeight);

                        return new Thumbnail(thumbnailUrl, thumbnailResolution);
                    })
                    .ToArray();

                var channel = new ChannelSearchResult(
                    channelId,
                    channelTitle,
                    channelThumbnails
                );

                results.Add(channel);
            }

            yield return Batch.Create(results);

            continuationToken = searchResults.TryGetContinuationToken();
        } while (!string.IsNullOrWhiteSpace(continuationToken));
    }

    /// <summary>
    /// Enumerates batches of search results returned by the specified query.
    /// </summary>
    public IAsyncEnumerable<Batch<ISearchResult>> GetResultBatchesAsync(
        string searchQuery,
        CancellationToken cancellationToken = default) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.None, cancellationToken);

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
        GetResultBatchesAsync(searchQuery, SearchFilter.Video, cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<VideoSearchResult>();

    /// <summary>
    /// Enumerates playlist search results returned by the specified query.
    /// </summary>
    public IAsyncEnumerable<PlaylistSearchResult> GetPlaylistsAsync(
        string searchQuery,
        CancellationToken cancellationToken = default) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.Playlist, cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<PlaylistSearchResult>();

    /// <summary>
    /// Enumerates channel search results returned by the specified query.
    /// </summary>
    public IAsyncEnumerable<ChannelSearchResult> GetChannelsAsync(
        string searchQuery,
        CancellationToken cancellationToken = default) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.Channel, cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<ChannelSearchResult>();
}
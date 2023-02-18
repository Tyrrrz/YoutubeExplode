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
    public SearchClient(HttpClient http) => _controller = new SearchController(http);

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

            var searchResults = await _controller.GetSearchResponseAsync(
                searchQuery,
                searchFilter,
                continuationToken,
                cancellationToken
            );

            // Video results
            foreach (var videoData in searchResults.Videos)
            {
                if (searchFilter is not SearchFilter.None and not SearchFilter.Video)
                {
                    Debug.Fail("Did not expect videos in search results.");
                    break;
                }

                var videoId =
                    videoData.Id ??
                    throw new YoutubeExplodeException("Could not extract video ID.");

                // Don't yield the same result twice
                if (!encounteredIds.Add(videoId))
                    continue;

                var videoTitle =
                    videoData.Title ??
                    throw new YoutubeExplodeException("Could not extract video title.");

                var videoChannelTitle =
                    videoData.Author ??
                    throw new YoutubeExplodeException("Could not extract video author.");

                var videoChannelId =
                    videoData.ChannelId ??
                    throw new YoutubeExplodeException("Could not extract video channel ID.");

                var videoThumbnails = videoData.Thumbnails.Select(t =>
                {
                    var thumbnailUrl =
                        t.Url ??
                        throw new YoutubeExplodeException("Could not extract video thumbnail URL.");

                    var thumbnailWidth =
                        t.Width ??
                        throw new YoutubeExplodeException("Could not extract video thumbnail width.");

                    var thumbnailHeight =
                        t.Height ??
                        throw new YoutubeExplodeException("Could not extract video thumbnail height.");

                    var thumbnailResolution = new Resolution(thumbnailWidth, thumbnailHeight);

                    return new Thumbnail(thumbnailUrl, thumbnailResolution);
                }).Concat(Thumbnail.GetDefaultSet(videoId)).ToArray();

                var video = new VideoSearchResult(
                    videoId,
                    videoTitle,
                    new Author(videoChannelId, videoChannelTitle),
                    videoData.Duration,
                    videoThumbnails
                );

                results.Add(video);
            }

            // Playlist results
            foreach (var playlistData in searchResults.Playlists)
            {
                if (searchFilter is not SearchFilter.None and not SearchFilter.Playlist)
                {
                    Debug.Fail("Did not expect playlists in search results.");
                    break;
                }

                var playlistId =
                    playlistData.Id ??
                    throw new YoutubeExplodeException("Could not extract playlist ID.");

                // Don't yield the same result twice
                if (!encounteredIds.Add(playlistId))
                    continue;

                var playlistTitle =
                    playlistData.Title ??
                    throw new YoutubeExplodeException("Could not extract playlist title.");

                // System playlists have no author
                var playlistAuthor =
                    !string.IsNullOrWhiteSpace(playlistData.ChannelId) &&
                    !string.IsNullOrWhiteSpace(playlistData.Author)
                        ? new Author(playlistData.ChannelId, playlistData.Author)
                        : null;

                var playlistThumbnails = playlistData.Thumbnails.Select(t =>
                {
                    var thumbnailUrl =
                        t.Url ??
                        throw new YoutubeExplodeException("Could not extract playlist thumbnail URL.");

                    var thumbnailWidth =
                        t.Width ??
                        throw new YoutubeExplodeException("Could not extract playlist thumbnail width.");

                    var thumbnailHeight =
                        t.Height ??
                        throw new YoutubeExplodeException("Could not extract playlist thumbnail height.");

                    var thumbnailResolution = new Resolution(thumbnailWidth, thumbnailHeight);

                    return new Thumbnail(thumbnailUrl, thumbnailResolution);
                }).ToArray();

                var playlist = new PlaylistSearchResult(
                    playlistId,
                    playlistTitle,
                    playlistAuthor,
                    playlistThumbnails
                );

                results.Add(playlist);
            }

            // Channel results
            foreach (var channelData in searchResults.Channels)
            {
                if (searchFilter is not SearchFilter.None and not SearchFilter.Channel)
                {
                    Debug.Fail("Did not expect channels in search results.");
                    break;
                }

                var channelId =
                    channelData.Id ??
                    throw new YoutubeExplodeException("Could not extract channel ID.");

                var channelTitle =
                    channelData.Title ??
                    throw new YoutubeExplodeException("Could not extract channel title.");

                var channelThumbnails = channelData.Thumbnails.Select(t =>
                {
                    var thumbnailUrl =
                        t.Url ??
                        throw new YoutubeExplodeException("Could not extract channel thumbnail URL.");

                    var thumbnailWidth =
                        t.Width ??
                        throw new YoutubeExplodeException("Could not extract channel thumbnail width.");

                    var thumbnailHeight =
                        t.Height ??
                        throw new YoutubeExplodeException("Could not extract channel thumbnail height.");

                    var thumbnailResolution = new Resolution(thumbnailWidth, thumbnailHeight);

                    return new Thumbnail(thumbnailUrl, thumbnailResolution);
                }).ToArray();

                var channel = new ChannelSearchResult(
                    channelId,
                    channelTitle,
                    channelThumbnails
                );

                results.Add(channel);
            }

            yield return Batch.Create(results);

            continuationToken = searchResults.ContinuationToken;
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
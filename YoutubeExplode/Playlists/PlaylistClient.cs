using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Playlists;

/// <summary>
/// Operations related to YouTube playlists.
/// </summary>
public class PlaylistClient
{
    private readonly PlaylistController _controller;

    /// <summary>
    /// Initializes an instance of <see cref="PlaylistClient"/>.
    /// </summary>
    public PlaylistClient(HttpClient http) =>
        _controller = new PlaylistController(http);

    /// <summary>
    /// Gets the metadata associated with the specified playlist.
    /// </summary>
    public async ValueTask<Playlist> GetAsync(
        PlaylistId playlistId,
        CancellationToken cancellationToken = default)
    {
        var playlistExtractor = await _controller.GetPlaylistDetailsAsync(playlistId, cancellationToken);


        var title =
            playlistExtractor.TryGetPlaylistTitle() ??
            throw new YoutubeExplodeException("Could not extract playlist title.");

        // System playlists have no author
        var channelId = playlistExtractor.TryGetPlaylistChannelId();
        var channelTitle = playlistExtractor.TryGetPlaylistAuthor();
        var author = channelId is not null && channelTitle is not null
            ? new Author(channelId, channelTitle)
            : null;
        //Can't get description from system playlists
        var description = playlistExtractor.TryGetPlaylistDescription() ?? "";

        //Can't get Thumbnails from system playlists, maybe use firt video thumbnail?
        var thumbnails = playlistExtractor
            .GetPlaylistThumbnails()
            .Select(t =>
            {
                var thumbnailUrl =
                    t.TryGetUrl() ??
                    throw new YoutubeExplodeException("Could not extract thumbnail URL.");

                var thumbnailWidth =
                    t.TryGetWidth() ??
                    throw new YoutubeExplodeException("Could not extract thumbnail width.");

                var thumbnailHeight =
                    t.TryGetHeight() ??
                    throw new YoutubeExplodeException("Could not extract thumbnail height.");

                var thumbnailResolution = new Resolution(thumbnailWidth, thumbnailHeight);

                return new Thumbnail(thumbnailUrl, thumbnailResolution);
            })
            .ToArray();

        return new Playlist(playlistId, title, author, description, thumbnails);
    }

    /// <summary>
    /// Enumerates batches of videos included in the specified playlist.
    /// </summary>
    public async IAsyncEnumerable<Batch<PlaylistVideo>> GetVideoBatchesAsync(
        PlaylistId playlistId,
        uint? numVideos,
        [EnumeratorCancellation] CancellationToken cancellationToken = default       
        )
    {
        var encounteredIds = new HashSet<string>(StringComparer.Ordinal);

        //Continuation token can't be used in mix playlist so we use an index and the last videoid
        var continuationToken = default(string?);
        var index = 0;
        string? lastVideoId = "";
        bool keepExtracting = false;

        do
        {
            var playlistExtractor =
                await _controller.GetPlaylistVideosAsync(playlistId,index: index,videoId: lastVideoId, continuationtoken: continuationToken, cancellationToken: cancellationToken);
            var videos = new List<PlaylistVideo>();

            //If numVideo is null replace with default values
            if(numVideos is null)
                numVideos = (playlistExtractor.IsMixPlaylist() ? 25 : uint.MaxValue);

            foreach (var videoExtractor in playlistExtractor.GetVideos())
            {
                var videoId =
                    videoExtractor.TryGetVideoId() ??
                    throw new YoutubeExplodeException("Could not extract video ID.");

                // Don't yield the same video twice
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

                var duration = videoExtractor.TryGetVideoDuration();

                var thumbnails = videoExtractor
                    .GetVideoThumbnails()
                    .Select(t =>
                    {
                        var thumbnailUrl =
                            t.TryGetUrl() ??
                            throw new YoutubeExplodeException("Could not extract thumbnail URL.");

                        var thumbnailWidth =
                            t.TryGetWidth() ??
                            throw new YoutubeExplodeException("Could not extract thumbnail width.");

                        var thumbnailHeight =
                            t.TryGetHeight() ??
                            throw new YoutubeExplodeException("Could not extract thumbnail height.");

                        var thumbnailResolution = new Resolution(thumbnailWidth, thumbnailHeight);

                        return new Thumbnail(thumbnailUrl, thumbnailResolution);
                    })
                    .Concat(Thumbnail.GetDefaultSet(videoId))
                    .ToArray();

                var video = new PlaylistVideo(
                    videoId,
                    videoTitle,
                    new Author(videoChannelId, videoChannelTitle),
                    duration,
                    thumbnails
                );

                videos.Add(video);

                
            }
            index += videos.Count;
            lastVideoId = videos.LastOrDefault()?.Id;
            //remove the excess videos
            if (index > numVideos) 
                videos.RemoveRange((int)(videos.Count - (index - numVideos) - 1), (int)(index - numVideos));

            yield return Batch.Create(videos);

            continuationToken = playlistExtractor.TryGetContinuationToken();
            //Its won't keep extracting if there is no continuationtoken for normal playlists or no videos for mix playlist
            keepExtracting = ((!string.IsNullOrWhiteSpace(continuationToken) || playlistExtractor.IsMixPlaylist())
                && index < numVideos && videos.Count != 0);
        } while (keepExtracting);
    }

    /// <summary>
    /// Enumerates videos included in the specified playlist.
    /// </summary>
    /// <param name="numVideos">
    /// Limits the number of videos that will try to enumerate.
    /// By default enumerates all videos or 50 videos if it's a mix playlist.
    /// </param>
    public IAsyncEnumerable<PlaylistVideo> GetVideosAsync(
        PlaylistId playlistId,
        uint? numVideos = default,
        CancellationToken cancellationToken = default) =>
        GetVideoBatchesAsync(playlistId ,numVideos,cancellationToken).FlattenAsync();
}
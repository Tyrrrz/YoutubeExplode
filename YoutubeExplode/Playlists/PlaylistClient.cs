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
        //Can't get description from mix playlists
        var description = playlistExtractor.TryGetPlaylistDescription() ?? "";

        //Can't get Thumbnails from mix playlists, maybe use firt video thumbnail?
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

        //Getting the firstVideoId means it's a mix playlist and we can't use the default playlist url 
        string? firstVideoId = playlistExtractor.GetVideos().FirstOrDefault()?.TryGetVideoId();

        if(firstVideoId is not null)
            return new Playlist(playlistId, title, author, description, thumbnails, 
                $"http://www.youtube.com/watch?v={firstVideoId}&list={playlistId}");
        else
            return new Playlist(playlistId, title, author, description, thumbnails);
    }

    /// <summary>
    /// Enumerates batches of videos included in the specified playlist.
    /// </summary>
    public async IAsyncEnumerable<Batch<PlaylistVideo>> GetVideoBatchesAsync(
        PlaylistId playlistId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var encounteredIds = new HashSet<string>(StringComparer.Ordinal);

        var visitorData = default(string?);
        var index = 0;
        string? lastVideoId = "";
        bool keepExtracting = false;
        do
        {
            var playlistExtractor =
                await _controller.GetPlaylistVideosAsync(playlistId,lastVideoId,index,visitorData,cancellationToken);
            var videos = new List<PlaylistVideo>();

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

            lastVideoId = playlistExtractor.TryGetLastVideoId();
            index = playlistExtractor.TryGetLastIndex() ?? 0;
            visitorData  = visitorData ?? playlistExtractor.TryGetVisitorData();

            yield return Batch.Create(videos);


            //Dont keep extracting if there are no new videos.
            keepExtracting = videos.Count != 0;
        } while (keepExtracting);
    }

    /// <summary>
    /// Enumerates videos included in the specified playlist.
    /// </summary>
    public IAsyncEnumerable<PlaylistVideo> GetVideosAsync(
        PlaylistId playlistId,
        CancellationToken cancellationToken = default) =>
        GetVideoBatchesAsync(playlistId ,cancellationToken).FlattenAsync();
}
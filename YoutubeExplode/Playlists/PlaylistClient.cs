using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos;

namespace YoutubeExplode.Playlists;

/// <summary>
/// Operations related to YouTube playlists.
/// </summary>
public class PlaylistClient
{
    private readonly PlaylistController _controller;

    /// <summary>
    /// Initializes an instance of <see cref="PlaylistClient" />.
    /// </summary>
    public PlaylistClient(HttpClient http) => _controller = new PlaylistController(http);

    /// <summary>
    /// Gets the metadata associated with the specified playlist.
    /// </summary>
    public async ValueTask<Playlist> GetAsync(
        PlaylistId playlistId,
        CancellationToken cancellationToken = default)
    {
        var response = await _controller.GetPlaylistResponseAsync(playlistId, cancellationToken);

        var title =
            response.Title ??
            throw new YoutubeExplodeException("Could not extract playlist title.");

        // System playlists have no author
        var channelId = response.ChannelId;
        var channelTitle = response.Author;
        var author = channelId is not null && channelTitle is not null
            ? new Author(channelId, channelTitle)
            : null;

        // System playlists have no description
        var description = response.Description ?? "";

        var thumbnails = response.Thumbnails.Select(t =>
        {
            var thumbnailUrl =
                t.Url ??
                throw new YoutubeExplodeException("Could not extract thumbnail URL.");

            var thumbnailWidth =
                t.Width ??
                throw new YoutubeExplodeException("Could not extract thumbnail width.");

            var thumbnailHeight =
                t.Height ??
                throw new YoutubeExplodeException("Could not extract thumbnail height.");

            var thumbnailResolution = new Resolution(thumbnailWidth, thumbnailHeight);

            return new Thumbnail(thumbnailUrl, thumbnailResolution);
        }).ToArray();

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
    /// </summary>
    public async IAsyncEnumerable<Batch<PlaylistVideo>> GetVideoBatchesAsync(
        PlaylistId playlistId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var encounteredIds = new HashSet<VideoId>();
        var lastVideoId = default(VideoId?);
        var lastVideoIndex = 0;
        var visitorData = default(string?);

        do
        {
            var response = await _controller.GetPlaylistNextResponseAsync(
                playlistId,
                lastVideoId,
                lastVideoIndex,
                visitorData,
                cancellationToken
            );

            var videos = new List<PlaylistVideo>();

            foreach (var videoData in response.Videos)
            {
                var videoId =
                    videoData.Id ??
                    throw new YoutubeExplodeException("Could not extract video ID.");

                lastVideoId = videoId;

                lastVideoIndex =
                    videoData.Index ??
                    throw new YoutubeExplodeException("Could not extract video index.");

                // Don't yield the same video twice
                if (!encounteredIds.Add(videoId))
                    continue;

                var videoTitle =
                    videoData.Title ??
                    // Videos without title are legal
                    // https://github.com/Tyrrrz/YoutubeExplode/issues/700
                    "";

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
                        throw new YoutubeExplodeException("Could not extract thumbnail URL.");

                    var thumbnailWidth =
                        t.Width ??
                        throw new YoutubeExplodeException("Could not extract thumbnail width.");

                    var thumbnailHeight =
                        t.Height ??
                        throw new YoutubeExplodeException("Could not extract thumbnail height.");

                    var thumbnailResolution = new Resolution(thumbnailWidth, thumbnailHeight);

                    return new Thumbnail(thumbnailUrl, thumbnailResolution);
                }).Concat(Thumbnail.GetDefaultSet(videoId)).ToArray();

                var video = new PlaylistVideo(
                    playlistId,
                    videoId,
                    videoTitle,
                    new Author(videoChannelId, videoChannelTitle),
                    videoData.Duration,
                    videoThumbnails
                );

                videos.Add(video);
            }

            // Stop extracting if there are no new videos
            if (!videos.Any())
                break;

            yield return Batch.Create(videos);

            visitorData ??= response.VisitorData;
        } while (true);
    }

    /// <summary>
    /// Enumerates videos included in the specified playlist.
    /// </summary>
    public IAsyncEnumerable<PlaylistVideo> GetVideosAsync(
        PlaylistId playlistId,
        CancellationToken cancellationToken = default) =>
        GetVideoBatchesAsync(playlistId, cancellationToken).FlattenAsync();
}
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Common;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.Videos;

/// <summary>
/// Operations related to YouTube videos.
/// </summary>
public class VideoClient
{
    private readonly VideoController _controller;

    /// <summary>
    /// Operations related to media streams of YouTube videos.
    /// </summary>
    public StreamClient Streams { get; }

    /// <summary>
    /// Operations related to closed captions of YouTube videos.
    /// </summary>
    public ClosedCaptionClient ClosedCaptions { get; }

    /// <summary>
    /// Initializes an instance of <see cref="VideoClient" />.
    /// </summary>
    public VideoClient(HttpClient http)
    {
        _controller = new VideoController(http);

        Streams = new StreamClient(http);
        ClosedCaptions = new ClosedCaptionClient(http);
    }

    /// <summary>
    /// Gets the metadata associated with the specified video.
    /// </summary>
    public async ValueTask<Video> GetAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default)
    {
        var watchPage = await _controller.GetVideoWatchPageAsync(videoId, cancellationToken);

        var playerResponse =
            watchPage.TryGetPlayerResponse() ??
            await _controller.GetPlayerResponseAsync(videoId, cancellationToken);

        var title =
            playerResponse.TryGetVideoTitle() ??
            throw new YoutubeExplodeException("Could not extract video title.");

        var channelTitle =
            playerResponse.TryGetVideoAuthor() ??
            throw new YoutubeExplodeException("Could not extract video author.");

        var channelId =
            playerResponse.TryGetVideoChannelId() ??
            throw new YoutubeExplodeException("Could not extract video channel ID.");

        var uploadDate =
            playerResponse.TryGetVideoUploadDate() ??
            throw new YoutubeExplodeException("Could not extract video upload date.");

        var description = playerResponse.TryGetVideoDescription() ?? "";
        var duration = playerResponse.TryGetVideoDuration();

        var thumbnails = playerResponse
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

        var keywords = playerResponse.GetVideoKeywords();

        // Engagement statistics may be hidden
        var viewCount = playerResponse.TryGetVideoViewCount() ?? 0;
        var likeCount = watchPage.TryGetVideoLikeCount() ?? 0;
        var dislikeCount = watchPage.TryGetVideoDislikeCount() ?? 0;

        return new Video(
            videoId,
            title,
            new Author(channelId, channelTitle),
            uploadDate,
            description,
            duration,
            thumbnails,
            keywords,
            new Engagement(viewCount, likeCount, dislikeCount)
        );
    }
}
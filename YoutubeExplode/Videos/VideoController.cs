using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Videos;

internal class VideoController : YoutubeControllerBase
{
    public VideoController(HttpClient http)
        : base(http)
    {
    }

    public async ValueTask<VideoWatchPageExtractor> GetVideoWatchPageAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://www.youtube.com/watch?v={videoId}&bpctr=9999999999&hl=en";

        var watchPage =
            await Retry.WhileNullAsync(
                async () => VideoWatchPageExtractor.TryCreate(
                    await SendHttpRequestAsync(url, cancellationToken)
                )
            ) ??
            throw new YoutubeExplodeException(
                "Video watch page is broken. " +
                "Please try again in a few minutes."
            );

        if (!watchPage.IsVideoAvailable())
            throw new VideoUnavailableException($"Video '{videoId}' is not available.");

        return watchPage;
    }

    public async ValueTask<PlayerResponseExtractor> GetPlayerResponseAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default)
    {
        const string url = $"https://www.youtube.com/youtubei/v1/player?key={ApiKey}";

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = Json.SerializeToHttpContent(new
            {
                videoId = videoId.Value,
                context = new
                {
                    client = new
                    {
                        clientName = "ANDROID",
                        clientVersion = "17.10.35",
                        androidSdkVersion = 30,
                        hl = "en",
                        gl = "US",
                        utcOffsetMinutes = 0
                    }
                }
            })
        };

        // User agent appears to be sometimes required when impersonating Android
        // https://github.com/iv-org/invidious/issues/3230#issuecomment-1226887639
        request.Headers.Add(
            "User-Agent",
            "com.google.android.youtube/17.10.35 (Linux; U; Android 12; GB) gzip"
        );

        var raw = await SendHttpRequestAsync(request, cancellationToken);
        var playerResponse = PlayerResponseExtractor.Create(raw);

        if (!playerResponse.IsVideoAvailable())
            throw new VideoUnavailableException($"Video '{videoId}' is not available.");

        return playerResponse;
    }

    public async ValueTask<PlayerResponseExtractor> GetPlayerResponseAsync(
        VideoId videoId,
        string signatureTimestamp,
        CancellationToken cancellationToken = default)
    {
        const string url = $"https://www.youtube.com/youtubei/v1/player?key={ApiKey}";

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = Json.SerializeToHttpContent(new
            {
                videoId = videoId.Value,
                context = new
                {
                    client = new
                    {
                        clientName = "TVHTML5_SIMPLY_EMBEDDED_PLAYER",
                        clientVersion = "2.0",
                        hl = "en",
                        gl = "US",
                        utcOffsetMinutes = 0
                    },
                    // Required for videos that don't support embedding on third-party websites
                    thirdParty = new
                    {
                        embedUrl = "https://www.youtube.com"
                    }
                },
                playbackContext = new
                {
                    contentPlaybackContext = new
                    {
                        signatureTimestamp
                    }
                }
            })
        };

        var raw = await SendHttpRequestAsync(request, cancellationToken);
        var playerResponse = PlayerResponseExtractor.Create(raw);

        if (!playerResponse.IsVideoAvailable())
            throw new VideoUnavailableException($"Video '{videoId}' is not available.");

        return playerResponse;
    }
}
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Videos;

internal class VideoController
{
    protected HttpClient Http { get; }

    public VideoController(HttpClient http) => Http = http;

    public async ValueTask<VideoWatchPage> GetVideoWatchPageAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default)
    {
        var retriesRemaining = 5;
        while (true)
        {
            var watchPage = VideoWatchPage.TryParse(
                await Http.GetStringAsync($"/watch?v={videoId}&bpctr=9999999999", cancellationToken)
            );

            if (watchPage is null)
            {
                if (retriesRemaining-- > 0)
                    continue;

                throw new YoutubeExplodeException(
                    "Video watch page is broken. " +
                    "Please try again in a few minutes."
                );
            }

            if (!watchPage.IsAvailable)
                throw new VideoUnavailableException($"Video '{videoId}' is not available.");

            return watchPage;
        }
    }

    public async ValueTask<PlayerResponse> GetPlayerResponseAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/youtubei/v1/player")
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

        using var response = await Http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var playerResponse = PlayerResponse.Parse(
            await response.Content.ReadAsStringAsync(cancellationToken)
        );

        if (!playerResponse.IsAvailable)
            throw new VideoUnavailableException($"Video '{videoId}' is not available.");

        return playerResponse;
    }

    public async ValueTask<PlayerResponse> GetPlayerResponseAsync(
        VideoId videoId,
        string? signatureTimestamp,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/youtubei/v1/player")
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

        using var response = await Http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var playerResponse = PlayerResponse.Parse(
            await response.Content.ReadAsStringAsync(cancellationToken)
        );

        if (!playerResponse.IsAvailable)
            throw new VideoUnavailableException($"Video '{videoId}' is not available.");

        return playerResponse;
    }
}
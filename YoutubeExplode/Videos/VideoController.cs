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
        for (var retriesRemaining = 5;; retriesRemaining--)
        {
            var watchPage = VideoWatchPage.TryParse(
                await Http.GetStringAsync($"https://www.youtube.com/watch?v={videoId}&bpctr=9999999999", cancellationToken)
            );

            if (watchPage is null)
            {
                if (retriesRemaining > 0)
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
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://www.youtube.com/youtubei/v1/player")
        {
            // ReSharper disable VariableHidesOuterVariable
            Content = new StringContent(Json.Create(x => x.Object(x => x
                .Property("videoId", x => x.String(videoId))
                .Property("context", x => x.Object(x => x
                    .Property("client", x => x.Object(x => x
                        .Property("clientName", x => x.String("ANDROID"))
                        .Property("clientVersion", x => x.String("17.10.35"))
                        .Property("androidSdkVersion", x => x.Number(30))
                        .Property("hl", x => x.String("en"))
                        .Property("gl", x => x.String("US"))
                        .Property("utcOffsetMinutes", x => x.Number(0))
                    ))
                ))
            )))
            // ReSharper restore VariableHidesOuterVariable
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
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://www.youtube.com/youtubei/v1/player")
        {
            // ReSharper disable VariableHidesOuterVariable
            Content = new StringContent(Json.Create(x => x.Object(x => x
                .Property("videoId", x => x.String(videoId))
                .Property("context", x => x.Object(x => x
                    .Property("client", x => x.Object(x => x
                        .Property("clientName", x => x.String("TVHTML5_SIMPLY_EMBEDDED_PLAYER"))
                        .Property("clientVersion", x => x.String("2.0"))
                        .Property("hl", x => x.String("en"))
                        .Property("gl", x => x.String("US"))
                        .Property("utcOffsetMinutes", x => x.Number(0))
                    ))
                    .Property("thirdParty", x => x.Object(x => x
                        .Property("embedUrl", x => x.String("https://www.youtube.com"))
                    ))
                ))
                .Property("playbackContext", x => x.Object(x => x
                    .Property("contentPlaybackContext", x => x.Object(x => x
                        .Property("signatureTimestamp", x => x.String(signatureTimestamp))
                    ))
                ))
            )))
            // ReSharper restore VariableHidesOuterVariable
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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Exceptions;

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
                await Http.GetStringAsync(
                    $"https://www.youtube.com/watch?v={videoId}&bpctr=9999999999",
                    cancellationToken
                )
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
        // The most optimal client to impersonate is the Android client, because
        // it doesn't require signature deciphering (for both normal and n-parameter signatures).
        // However, the regular Android client has a limitation, preventing it from downloading
        // multiple streams from the same manifest (or the same stream multiple times).
        // As a workaround, we're using ANDROID_TESTSUITE which appears to offer the same
        // functionality, but doesn't impose the aforementioned limitation.
        // https://github.com/Tyrrrz/YoutubeExplode/issues/705
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://www.youtube.com/youtubei/v1/player")
        {
            Content = new StringContent(
                // lang=json
                $$"""
                {
                    "videoId": "{{videoId}}",
                    "context": {
                        "client": {
                            "clientName": "ANDROID_TESTSUITE",
                            "clientVersion": "1.9",
                            "androidSdkVersion": 30,
                            "hl": "en",
                            "gl": "US",
                            "utcOffsetMinutes": 0
                        }
                    }
                }
                """
            )
        };

        // User agent appears to be sometimes required when impersonating Android
        // https://github.com/iv-org/invidious/issues/3230#issuecomment-1226887639
        request.Headers.Add(
            "User-Agent",
            "com.google.android.youtube/17.36.4 (Linux; U; Android 12; GB) gzip"
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
        // The only client that can handle age-restricted videos without authentication is the
        // TVHTML5_SIMPLY_EMBEDDED_PLAYER client.
        // This client does require signature deciphering, so we only use it as a fallback.
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://www.youtube.com/youtubei/v1/player")
        {
            Content = new StringContent(
                // lang=json
                $$"""
                {
                    "videoId": "{{videoId}}",
                    "context": {
                        "client": {
                            "clientName": "TVHTML5_SIMPLY_EMBEDDED_PLAYER",
                            "clientVersion": "2.0",
                            "hl": "en",
                            "gl": "US",
                            "utcOffsetMinutes": 0
                        },
                        "thirdParty": {
                            "embedUrl": "https://www.youtube.com"
                        }
                    },
                    "playbackContext": {
                        "contentPlaybackContext": {
                            "signatureTimestamp": "{{signatureTimestamp ?? "19369"}}"
                        }
                    }
                }
                """
            )
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
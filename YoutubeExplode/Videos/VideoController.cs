using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Videos;

internal class VideoController(HttpClient http)
{
    protected HttpClient Http { get; } = http;

    public async ValueTask<VideoWatchPage> GetVideoWatchPageAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default
    )
    {
        for (var retriesRemaining = 5; ; retriesRemaining--)
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
                    "Video watch page is broken. Please try again in a few minutes."
                );
            }

            if (!watchPage.IsAvailable)
                throw new VideoUnavailableException($"Video '{videoId}' is not available.");

            return watchPage;
        }
    }

    public async ValueTask<PlayerResponse> GetPlayerResponseAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default
    )
    {
        // The most optimal client to impersonate is any mobile client, because they
        // don't require signature deciphering (for both normal and n-parameter signatures).
        // However, we can't use the ANDROID client because it has a limitation, preventing it
        // from downloading multiple streams from the same manifest (or the same stream multiple times).
        // https://github.com/Tyrrrz/YoutubeExplode/issues/705
        // Previously, we were using ANDROID_TESTSUITE as a workaround, which appeared to offer the same
        // functionality, but without the aforementioned limitation. However, YouTube discontinued this
        // client, so now we have to use IOS instead.
        // https://github.com/Tyrrrz/YoutubeExplode/issues/817
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://www.youtube.com/youtubei/v1/player"
        );

        request.Content = new StringContent(
            // lang=json
            $$"""
            {
              "videoId": {{Json.Serialize(videoId)}},
              "contentCheckOk": true,
              "context": {
                "client": {
                  "clientName": "IOS",
                  "clientVersion": "19.29.1",
                  "deviceMake": "Apple",
                  "deviceModel": "iPhone16,2",
                  "hl": "en",
                  "osName": "iPhone",
                  "osVersion": "17.5.1.21F90",
                  "timeZone": "UTC",
                  "userAgent": "com.google.ios.youtube/19.29.1 (iPhone16,2; U; CPU iOS 17_5_1 like Mac OS X;)",
                  "gl": "US",
                  "utcOffsetMinutes": 0
                }
              }
            }
            """
        );

        // User agent appears to be sometimes required when impersonating Android
        // https://github.com/iv-org/invidious/issues/3230#issuecomment-1226887639
        request.Headers.Add(
            "User-Agent",
            "com.google.ios.youtube/19.29.1 (iPhone16,2; U; CPU iOS 17_5_1 like Mac OS X)"
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
        CancellationToken cancellationToken = default
    )
    {
        // The only client that can handle age-restricted videos without authentication is the
        // TVHTML5_SIMPLY_EMBEDDED_PLAYER client.
        // This client does require signature deciphering, so we only use it as a fallback.
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://www.youtube.com/youtubei/v1/player"
        );

        request.Content = new StringContent(
            // lang=json
            $$"""
            {
              "videoId": {{Json.Serialize(videoId)}},
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
                  "signatureTimestamp": {{Json.Serialize(signatureTimestamp)}}
                }
              }
            }
            """
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
}

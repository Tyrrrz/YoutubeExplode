using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PowerKit.Json.Extensions;
using YoutubeExplode.Bridge;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Videos;

internal class VideoController(HttpClient http)
{
    private string? _visitorData;

    protected HttpClient Http { get; } = http;

    private async ValueTask<string> ResolveVisitorDataAsync(
        CancellationToken cancellationToken = default
    )
    {
        if (!string.IsNullOrWhiteSpace(_visitorData))
            return _visitorData;

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://www.youtube.com/sw.js_data"
        );

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        request.Headers.Add(
            "User-Agent",
            "com.google.android.youtube/20.10.38 (Linux; U; ANDROID 11) gzip"
        );

        using var response = await Http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        // TODO: move this to a bridge wrapper
        var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
        if (jsonString.StartsWith(")]}'"))
            jsonString = jsonString[4..];

        var json = JsonElement.Parse(jsonString);

        // This is just an unstructured (but ordered) blob of data
        var value = json[0][2][0][0][13].GetStringOrNull();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new YoutubeExplodeException("Failed to resolve visitor data.");
        }

        return _visitorData = value;
    }

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

    private async ValueTask<PlayerResponse> GetPlayerResponseForVisionOsAsync(
        VideoId videoId,
        string visitorData,
        CancellationToken cancellationToken = default
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://www.youtube.com/youtubei/v1/player"
        );

        request.Content = new StringContent(
            Json.Encode(
                new Dictionary<string, object?>
                {
                    ["videoId"] = videoId.ToString(),
                    ["contentCheckOk"] = true,
                    ["racyCheckOk"] = true,
                    ["context"] = new Dictionary<string, object?>
                    {
                        ["client"] = new Dictionary<string, object?>
                        {
                            ["clientName"] = "VISIONOS",
                            ["clientVersion"] = "1.02",
                            ["deviceMake"] = "Apple",
                            ["deviceModel"] = "RealityDevice17,1",
                            ["osName"] = "visionOS",
                            ["osVersion"] = "26.5.23O471",
                            ["visitorData"] = visitorData,
                            ["hl"] = "en",
                            ["gl"] = "US",
                            ["utcOffsetMinutes"] = 0,
                        },
                    },
                }
            )
        );

        request.Headers.Add(
            "User-Agent",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 15_7_3) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/26.0 Safari/605.1.15"
        );

        using var response = await Http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var playerResponse = PlayerResponse.Parse(
            await response.Content.ReadAsStringAsync(cancellationToken)
        );

        if (!playerResponse.IsAvailable)
            throw new VideoUnavailableException($"Video '{videoId}' is not available.");

        if (!playerResponse.IsPlayable)
            throw new VideoUnplayableException($"Video '{videoId}' is unplayable.");

        return playerResponse;
    }

    private async ValueTask<PlayerResponse> GetPlayerResponseForAndroidAsync(
        VideoId videoId,
        string visitorData,
        CancellationToken cancellationToken = default
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://www.youtube.com/youtubei/v1/player"
        );

        request.Content = new StringContent(
            Json.Encode(
                new Dictionary<string, object?>
                {
                    ["videoId"] = videoId.ToString(),
                    ["contentCheckOk"] = true,
                    ["racyCheckOk"] = true,
                    ["context"] = new Dictionary<string, object?>
                    {
                        ["client"] = new Dictionary<string, object?>
                        {
                            ["clientName"] = "ANDROID",
                            ["clientVersion"] = "21.26.364",
                            ["androidSdkVersion"] = 30,
                            ["osName"] = "Android",
                            ["osVersion"] = "11",
                            ["visitorData"] = visitorData,
                            ["hl"] = "en",
                            ["gl"] = "US",
                            ["utcOffsetMinutes"] = 0,
                        },
                    },
                }
            )
        );

        request.Headers.Add(
            "User-Agent",
            "com.google.android.youtube/21.26.364 (Linux; U; Android 11) gzip"
        );

        using var response = await Http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var playerResponse = PlayerResponse.Parse(
            await response.Content.ReadAsStringAsync(cancellationToken)
        );

        if (!playerResponse.IsAvailable)
            throw new VideoUnavailableException($"Video '{videoId}' is not available.");

        if (!playerResponse.IsPlayable)
            throw new VideoUnplayableException($"Video '{videoId}' is unplayable.");

        return playerResponse;
    }

    private async ValueTask<PlayerResponse> GetPlayerResponseForTvAsync(
        VideoId videoId,
        string visitorData,
        string? signatureTimestamp,
        CancellationToken cancellationToken = default
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://www.youtube.com/youtubei/v1/player"
        );

        request.Content = new StringContent(
            Json.Encode(
                new Dictionary<string, object?>
                {
                    ["videoId"] = videoId.ToString(),
                    ["context"] = new Dictionary<string, object?>
                    {
                        ["client"] = new Dictionary<string, object?>
                        {
                            ["clientName"] = "TVHTML5_SIMPLY_EMBEDDED_PLAYER",
                            ["clientVersion"] = "2.0",
                            ["visitorData"] = visitorData,
                            ["hl"] = "en",
                            ["gl"] = "US",
                            ["utcOffsetMinutes"] = 0,
                        },
                        ["thirdParty"] = new Dictionary<string, object?>
                        {
                            ["embedUrl"] = "https://www.youtube.com",
                        },
                    },
                    ["playbackContext"] = new Dictionary<string, object?>
                    {
                        ["contentPlaybackContext"] = new Dictionary<string, object?>
                        {
                            ["signatureTimestamp"] = signatureTimestamp,
                        },
                    },
                }
            )
        );

        request.Headers.Add(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36"
        );

        using var response = await Http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var playerResponse = PlayerResponse.Parse(
            await response.Content.ReadAsStringAsync(cancellationToken)
        );

        if (!playerResponse.IsAvailable)
            throw new VideoUnavailableException($"Video '{videoId}' is not available.");

        if (!playerResponse.IsPlayable)
            throw new VideoUnplayableException($"Video '{videoId}' is unplayable.");

        return playerResponse;
    }

    public async ValueTask<PlayerResponse> GetPlayerResponseAsync(
        VideoId videoId,
        string? signatureTimestamp = null,
        CancellationToken cancellationToken = default
    )
    {
        var visitorData = await ResolveVisitorDataAsync(cancellationToken);

        // We use the TV client for age-restricted videos as it circumvents the age gate, but it
        // imposes signature ciphering, so we only use this client if we have a signature timestamp.
        if (!string.IsNullOrWhiteSpace(signatureTimestamp))
        {
            return await GetPlayerResponseForTvAsync(
                videoId,
                visitorData,
                signatureTimestamp,
                cancellationToken
            );
        }

        try
        {
            // VisionOS is the primary client, as it works for most videos
            return await GetPlayerResponseForVisionOsAsync(videoId, visitorData, cancellationToken);
        }
        catch (Exception ex) when (ex is VideoUnplayableException or VideoUnavailableException)
        {
            // Android is used as a fallback as it works for certain other videos, such as videos intended for kids
            return await GetPlayerResponseForAndroidAsync(videoId, visitorData, cancellationToken);
        }
    }

    public async ValueTask<PlayerResponse> GetPlayerResponseAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default
    ) => await GetPlayerResponseAsync(videoId, null, cancellationToken);
}

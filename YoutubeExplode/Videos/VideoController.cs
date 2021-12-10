using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Bridge.Extractors;
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

        for (var retry = 0; retry <= 5; retry++)
        {
            var raw = await SendHttpRequestAsync(url, cancellationToken);

            var watchPage = VideoWatchPageExtractor.TryCreate(raw);
            if (watchPage is not null)
            {
                if (!watchPage.IsVideoAvailable())
                {
                    throw new VideoUnavailableException($"Video '{videoId}' is not available.");
                }

                return watchPage;
            }
        }

        throw new YoutubeExplodeException(
            "Video watch page is broken. " +
            "Please try again in a few minutes."
        );
    }

    public async Task<PlayerResponseExtractor> GetPlayerResponseAsync(
        VideoId videoId,
        bool isEmbedded,
        CancellationToken cancellationToken = default)
    {
        const string url = $"https://www.youtube.com/youtubei/v1/player?key={ApiKey}";

        var payload = new Dictionary<string, object?>
        {
            ["contentCheckOk"] = true,
            ["racyCheckOk"] = true,
            ["videoId"] = videoId.Value,
            ["context"] = new Dictionary<string, object?>
            {
                ["client"] = new Dictionary<string, object?>
                {
                    ["clientName"] = "ANDROID",
                    ["clientScreen"] = isEmbedded ? "EMBED" : null,
                    ["clientVersion"] = "16.46.37",
                    ["hl"] = "en",
                    ["gl"] = "US",
                    ["utcOffsetMinutes"] = 0
                },
                ["thirdParty"] = new Dictionary<string, object?>
                {
                    ["embedUrl"] = "https://www.youtube.com"
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = Json.SerializeToHttpContent(payload)
        };

        var raw = await SendHttpRequestAsync(request, cancellationToken);

        return PlayerResponseExtractor.Create(raw);
    }

    public async Task<PlayerResponseExtractor> GetPlayerResponseAsync(
        VideoId videoId,
        CancellationToken cancellationToken = default) =>
        await GetPlayerResponseAsync(videoId, false, cancellationToken);
}
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
                contentCheckOk = true,
                racyCheckOk = true,
                context = new
                {
                    client = new
                    {
                        clientName = "ANDROID",
                        clientVersion = "17.29.35",
                        androidSdkVersion = 30,
                        hl = "en",
                        gl = "US",
                        utcOffsetMinutes = 0
                    }
                }
            })
        };

        var raw = await SendHttpRequestAsync(request, cancellationToken);
        var playerResponse = PlayerResponseExtractor.Create(raw);

        if (!playerResponse.IsVideoAvailable())
        {
            throw new VideoUnavailableException($"Video '{videoId}' is not available.");
        }

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
                contentCheckOk = true,
                racyCheckOk = true,
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
        {
            throw new VideoUnavailableException($"Video '{videoId}' is not available.");
        }

        return playerResponse;
    }
}
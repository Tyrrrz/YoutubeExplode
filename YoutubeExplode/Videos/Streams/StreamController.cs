using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;

namespace YoutubeExplode.Videos.Streams;

internal class StreamController : VideoController
{
    public StreamController(HttpClient http)
        : base(http)
    {
    }

    public async ValueTask<PlayerSourceExtractor?> TryGetPlayerSourceAsync(
        CancellationToken cancellationToken = default)
    {
        var iframeContent = await SendHttpRequestAsync(
            "https://www.youtube.com/iframe_api",
            cancellationToken
        );

        var version = Regex.Match(iframeContent, @"player\\?/([0-9a-fA-F]{8})\\?/").Groups[1].Value;
        if (string.IsNullOrWhiteSpace(version))
            return null;

        var source = await SendHttpRequestAsync(
            $"https://www.youtube.com/s/player/{version}/player_ias.vflset/en_US/base.js",
            cancellationToken
        );

        return PlayerSourceExtractor.Create(source);
    }

    public async ValueTask<DashManifestExtractor> GetDashManifestAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        var raw = await SendHttpRequestAsync(url, cancellationToken);
        return DashManifestExtractor.Create(raw);
    }
}
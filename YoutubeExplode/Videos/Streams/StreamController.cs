using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Videos.Streams;

internal class StreamController : VideoController
{
    public StreamController(HttpClient http)
        : base(http)
    {
    }

    public async ValueTask<PlayerSourceExtractor> GetPlayerSourceAsync(
        CancellationToken cancellationToken = default)
    {
        var iframe = await GetStringAsync("/iframe_api", cancellationToken);

        var version = Regex.Match(iframe, @"player\\?/([0-9a-fA-F]{8})\\?/").Groups[1].Value;
        if (string.IsNullOrWhiteSpace(version))
            throw new YoutubeExplodeException("Could not extract player version.");

        var source = await GetStringAsync(
            $"/s/player/{version}/player_ias.vflset/en_US/base.js",
            cancellationToken
        );

        return PlayerSourceExtractor.Create(source);
    }

    public async ValueTask<DashManifestExtractor> GetDashManifestAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        var raw = await GetStringAsync(url, cancellationToken);
        return DashManifestExtractor.Create(raw);
    }
}
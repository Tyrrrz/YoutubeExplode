using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Bridge;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Videos.ClosedCaptions;

internal class ClosedCaptionController : VideoController
{
    public ClosedCaptionController(HttpClient http)
        : base(http)
    {
    }

    public async ValueTask<ClosedCaptionTrackExtractor> GetClosedCaptionTrackAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        // Enforce known format
        var urlWithFormat = url
            .Pipe(s => Url.SetQueryParameter(s, "format", "3"))
            .Pipe(s => Url.SetQueryParameter(s, "fmt", "3"));

        var raw = await SendHttpRequestAsync(urlWithFormat, cancellationToken);

        return ClosedCaptionTrackExtractor.Create(raw);
    }
}
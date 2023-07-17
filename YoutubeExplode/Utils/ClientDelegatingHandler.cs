using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeExplode.Utils;

// Used to extend an externally provided HttpClient with additional behavior
internal abstract class ClientDelegatingHandler : HttpMessageHandler
{
    private readonly HttpClient _http;
    private readonly bool _disposeClient;

    protected ClientDelegatingHandler(HttpClient http, bool disposeClient = false)
    {
        _http = http;
        _disposeClient = disposeClient;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken) =>
        await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

    protected override void Dispose(bool disposing)
    {
        if (disposing && _disposeClient)
            _http.Dispose();

        base.Dispose(disposing);
    }
}
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeExplode.Utils.Extensions;

internal static class HttpExtensions
{
    // NET5.0+ has a better API for this, but it's annoying to target multiple frameworks
    public static HttpStatusCode? TryGetStatusCode(this HttpRequestException exception) => Regex
        .Match(exception.Message, @"\b(\d{3})\b")
        .Groups[1]
        .Value
        .NullIfWhiteSpace()?
        .ParseIntOrNull()?
        .Pipe(s => (HttpStatusCode)s);

    public static async ValueTask<HttpResponseMessage> HeadAsync(
        this HttpClient http,
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, requestUri);
        return await http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );
    }

    public static async ValueTask<Stream> GetStreamAsync(
        this HttpClient http,
        string requestUri,
        long? from = null,
        long? to = null,
        bool ensureSuccess = true,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Range = new RangeHeaderValue(from, to);

        var response = await http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        if (ensureSuccess)
            response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public static async ValueTask<long?> TryGetContentLengthAsync(
        this HttpClient http,
        string requestUri,
        bool ensureSuccess = true,
        CancellationToken cancellationToken = default)
    {
        using var response = await http.HeadAsync(requestUri, cancellationToken);

        if (ensureSuccess)
            response.EnsureSuccessStatusCode();

        return response.Content.Headers.ContentLength;
    }
}
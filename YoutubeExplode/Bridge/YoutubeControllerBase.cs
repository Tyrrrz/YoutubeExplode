using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Bridge;

internal abstract class YoutubeControllerBase
{
    private readonly HttpClient _http;

    protected YoutubeControllerBase(HttpClient http) =>
        _http = http;

    protected async ValueTask<string> GetStringAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        // Root the URL if necessary
        if (request.RequestUri is { IsAbsoluteUri: false })
        {
            request.RequestUri = new Uri(
                new Uri("https://www.youtube.com/", UriKind.Absolute),
                request.RequestUri
            );
        }

        // Set API key if necessary
        if (request.RequestUri is not null &&
            request.RequestUri.AbsolutePath.StartsWith("/youtubei/") &&
            !Url.ContainsQueryParameter(request.RequestUri.Query, "key"))
        {
            request.RequestUri = new Uri(
                Url.SetQueryParameter(
                    request.RequestUri.OriginalString,
                    "key",
                    // This key doesn't appear to change
                    "AIzaSyA8eiZmM1FaDVjRy-df2KTyQ_vz_yYM39w"
                )
            );
        }

        // Set user agent if necessary
        if (!request.Headers.Contains("User-Agent"))
        {
            request.Headers.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36"
            );
        }

        // Set required cookies
        request.Headers.Add("Cookie", "CONSENT=YES+cb; YSC=DwKYllHNwuw");

        using var response = await _http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        // Special case check for rate limiting errors
        if ((int)response.StatusCode == 429)
        {
            throw new RequestLimitExceededException(
                "Exceeded request rate limit. " +
                "Please try again in a few hours. " +
                "Alternatively, inject an instance of HttpClient that includes cookies for a pre-authenticated user."
            );
        }

        if (!response.IsSuccessStatusCode)
        {
            var message =
                $"""
                Response status code does not indicate success: {(int) response.StatusCode} ({response.StatusCode}).

                Request:
                {request}
                """;

#if NET5_0_OR_GREATER
            throw new HttpRequestException(message, null, response.StatusCode);
#else
            throw new HttpRequestException(message);
#endif
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    protected async ValueTask<string> GetStringAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        return await GetStringAsync(request, cancellationToken);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PowerKit;
using PowerKit.Extensions;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;

namespace YoutubeExplode;

internal partial class YoutubeHttpHandler : ClientDelegatingHandler
{
    private readonly CookieContainer _cookieContainer = new();

    public YoutubeHttpHandler(
        HttpClient http,
        IReadOnlyList<Cookie> initialCookies,
        bool disposeClient = false
    )
        : base(http, disposeClient)
    {
        // Consent to the use of cookies on YouTube.
        // This is required to access some personalized content, such as mix playlists.
        // https://github.com/Tyrrrz/YoutubeExplode/issues/730
        // https://github.com/Tyrrrz/YoutubeExplode/issues/732
        // https://github.com/Tyrrrz/YoutubeExplode/issues/907
        // The cookie embeds the time at which consent was given and is invalidated after
        // 13 months, so it needs to be generated with the current date instead of being hardcoded.
        // https://policies.google.com/technologies/cookies/embedded
        _cookieContainer.Add(
            new Cookie("SOCS", GenerateConsentCookieValue(DateTimeOffset.UtcNow))
            {
                Domain = "youtube.com",
            }
        );

        // Add user-supplied cookies
        foreach (var cookie in initialCookies)
            _cookieContainer.Add(cookie);
    }

    private string? TryGenerateAuthHeaderValue(Uri uri)
    {
        var cookies = _cookieContainer.GetCookies(uri).Cast<Cookie>().ToArray();

        var sessionId =
            cookies
                .FirstOrDefault(c =>
                    string.Equals(c.Name, "__Secure-3PAPISID", StringComparison.Ordinal)
                )
                ?.Value
            ?? cookies
                .FirstOrDefault(c => string.Equals(c.Name, "SAPISID", StringComparison.Ordinal))
                ?.Value;

        if (string.IsNullOrWhiteSpace(sessionId))
            return null;

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var token = $"{timestamp} {sessionId} {uri.Domain}";
        var tokenHash = Convert.ToHexString(
            HashAlgorithm.ComputeHash(SHA1.Create(), Encoding.UTF8.GetBytes(token))
        );

        return $"SAPISIDHASH {timestamp}_{tokenHash}";
    }

    private HttpRequestMessage HandleRequest(HttpRequestMessage request)
    {
        // Shouldn't happen?
        if (request.RequestUri is null)
            return request;

        // Set internal API key
        if (
            request.RequestUri.AbsolutePath.StartsWith("/youtubei/", StringComparison.Ordinal)
            && !UrlEx.ContainsQueryParameter(request.RequestUri.Query, "key")
        )
        {
            request.RequestUri = new Uri(
                UrlEx.SetQueryParameter(
                    request.RequestUri.OriginalString,
                    "key",
                    // This key doesn't appear to change
                    "AIzaSyA8eiZmM1FaDVjRy-df2KTyQ_vz_yYM39w"
                )
            );
        }

        // Set localization language
        if (!UrlEx.ContainsQueryParameter(request.RequestUri.Query, "hl"))
        {
            request.RequestUri = new Uri(
                UrlEx.SetQueryParameter(request.RequestUri.OriginalString, "hl", "en")
            );
        }

        // Set origin
        if (!request.Headers.Contains("Origin"))
        {
            request.Headers.Add("Origin", request.RequestUri.Domain);
        }

        // Set user agent
        if (!request.Headers.Contains("User-Agent"))
        {
            request.Headers.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36"
            );
        }

        // Set cookies
        if (!request.Headers.Contains("Cookie") && _cookieContainer.Count > 0)
        {
            var cookieHeaderValue = _cookieContainer.GetCookieHeader(request.RequestUri);
            if (!string.IsNullOrWhiteSpace(cookieHeaderValue))
                request.Headers.Add("Cookie", cookieHeaderValue);
        }

        // Set authorization
        if (
            !request.Headers.Contains("Authorization")
            && TryGenerateAuthHeaderValue(request.RequestUri) is { } authHeaderValue
        )
        {
            request.Headers.Add("Authorization", authHeaderValue);
        }

        return request;
    }

    private HttpResponseMessage HandleResponse(HttpResponseMessage response)
    {
        if (response.RequestMessage?.RequestUri is null)
            return response;

        // Custom exception for rate limit errors
        if ((int)response.StatusCode == 429)
        {
            throw new RequestLimitExceededException(
                "Exceeded request rate limit. "
                    + "Please try again in a few hours. "
                    + "Alternatively, inject cookies corresponding to a pre-authenticated user when initializing an instance of `YoutubeClient`."
            );
        }

        // Set cookies
        if (response.Headers.TryGetValues("Set-Cookie", out var cookieHeaderValues))
        {
            foreach (var cookieHeaderValue in cookieHeaderValues)
            {
                try
                {
                    _cookieContainer.SetCookies(
                        response.RequestMessage.RequestUri,
                        cookieHeaderValue
                    );
                }
                catch (CookieException)
                {
                    // YouTube may send cookies for other domains, ignore them
                    // https://github.com/Tyrrrz/YoutubeExplode/issues/762
                }
            }
        }

        return response;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        for (var retriesRemaining = 5; ; retriesRemaining--)
        {
            var response = HandleResponse(
                await base.SendAsync(
                    // Request will be cloned by the base handler
                    HandleRequest(request),
                    cancellationToken
                )
            );

            // Retry on 5XX errors
            if ((int)response.StatusCode >= 500 && retriesRemaining > 0)
            {
                response.Dispose();
                continue;
            }

            return response;
        }
    }
}

internal partial class YoutubeHttpHandler
{
    // Protobuf-encoded consent record: { 1: 2, 2: { 1: 3, 2: "813836352", 3: "en", 4: 1 }, 3: { 1: <timestamp> } }
    private static string GenerateConsentCookieValue(DateTimeOffset timestamp)
    {
        var timestampMessage = new List<byte> { 0x08 };

        Protobuf.WriteVarint(
            timestampMessage,
            // Round down to the start of the day (UTC), like the cookies issued by YouTube
            (ulong)
                new DateTimeOffset(timestamp.UtcDateTime.Date, TimeSpan.Zero).ToUnixTimeSeconds()
        );

        var buffer = new List<byte> { 0x08, 0x02, 0x12, 0x13, 0x08, 0x03, 0x12, 0x09 };
        buffer.AddRange(Encoding.ASCII.GetBytes("813836352"));
        buffer.AddRange([0x1A, 0x02, (byte)'e', (byte)'n', 0x20, 0x01]);
        buffer.AddRange([0x1A, (byte)timestampMessage.Count]);
        buffer.AddRange(timestampMessage);

        return Convert
            .ToBase64String(buffer.ToArray())
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}

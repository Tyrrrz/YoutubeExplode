using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace YoutubeExplode;

internal sealed class YoutubeHttpClient : HttpClient
{
    private readonly YoutubeHttpHandler _handler;

    public bool IsAuthenticated => _handler.IsAuthenticated;

    private YoutubeHttpClient(YoutubeHttpHandler handler)
        : base(handler, true) => _handler = handler;

    public YoutubeHttpClient(HttpClient http, IReadOnlyList<Cookie> initialCookies)
        : this(new YoutubeHttpHandler(http, initialCookies)) { }
}

using System;
using System.Net.Http;

#if NETCOREAPP2_1_OR_GREATER
using HttpClientHandler = System.Net.Http.SocketsHttpHandler;
#endif

namespace YoutubeExplode.Utils;

internal static class Http
{
    private static readonly Lazy<HttpClient> HttpClientLazy = new(() =>
        new HttpClient(new HttpClientHandler
        {
            // https://github.com/Tyrrrz/YoutubeExplode/issues/530
            UseCookies = false
        }, true)
    );

    public static HttpClient Client => HttpClientLazy.Value;
}
using System;
using System.Net.Http;
using YoutubeExplode.Tests.Utils;

namespace YoutubeExplode.Tests;

public abstract class SpecsBase
{
    // Static handler to apply rate limiting to all tests
    public static HttpMessageHandler HttpHandler { get; } =
        new RateLimitedHttpHandler(TimeSpan.FromSeconds(1));

    public YoutubeClient Youtube { get; } = new(new HttpClient(HttpHandler, false));
}

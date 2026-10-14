using System.Net.Http;

namespace YoutubeExplode;

internal sealed class YoutubeHttpRequest(HttpMethod method, string url)
    : HttpRequestMessage(method, url)
{
    public string? UserSessionId { get; init; }

    public bool IncludeApiKey { get; init; } = true;

    public bool RetryOnServerErrors { get; init; }
}

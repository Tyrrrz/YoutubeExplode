using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PowerKit.Extensions;
using YoutubeExplode.Bridge;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Videos.ClosedCaptions;

internal class ClosedCaptionController(HttpClient http) : VideoController(http)
{
    private const string EmbeddedUserAgent =
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/15.5 Safari/605.1.15";
    private const string EmbedOrigin = "https://www.google.com/";

    private sealed record TimestampCache(string PlayerUrl, int Value);

    private TimestampCache? _signatureTimestamp;

    private async ValueTask<int> GetSignatureTimestampAsync(
        EmbeddedPlayerConfig config,
        CancellationToken cancellationToken
    )
    {
        if (config.SignatureTimestamp is { } inlineTimestamp)
            return inlineTimestamp;
        var playerUrl =
            config.PlayerSourceUrl
            ?? throw new YoutubeExplodeException("Missing embedded player script URL.");
        if (_signatureTimestamp is { } cachedTimestamp && playerUrl == cachedTimestamp.PlayerUrl)
            return cachedTimestamp.Value;

        using var request = new YoutubeHttpRequest(
            HttpMethod.Get,
            new Uri(new Uri("https://www.youtube.com"), playerUrl).AbsoluteUri
        );
        using var response = await Http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var source = PlayerSource.Parse(
            await response.Content.ReadAsStringAsync(cancellationToken)
        );
        if (
            !int.TryParse(
                source.SignatureTimestamp,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var timestamp
            )
        )
            throw new YoutubeExplodeException(
                "Failed to extract embedded player signature timestamp."
            );
        _signatureTimestamp = new TimestampCache(playerUrl, timestamp);
        return timestamp;
    }

    public async ValueTask<PlayerResponse> GetCaptionPlayerResponseAsync(
        VideoId videoId,
        CancellationToken cancellationToken
    )
    {
        if (Http is not YoutubeHttpClient { IsAuthenticated: true })
            return await GetPlayerResponseAsync(videoId, cancellationToken);

        // Cookie authentication requires a web client. Bootstrap it once, without loading
        // the watch page or media manifests. Only read the player script's timestamp;
        // caption downloads do not need JavaScript execution or media URL deciphering.
        using var configRequest = new YoutubeHttpRequest(
            HttpMethod.Get,
            $"https://www.youtube.com/embed/{videoId}?html5=1"
        );
        configRequest.Headers.Add("User-Agent", EmbeddedUserAgent);
        configRequest.Headers.Add("Referer", EmbedOrigin);
        using var configResponse = await Http.SendAsync(configRequest, cancellationToken);
        configResponse.EnsureSuccessStatusCode();
        var config = EmbeddedPlayerConfig.Parse(
            await configResponse.Content.ReadAsStringAsync(cancellationToken)
        );
        var signatureTimestamp = await GetSignatureTimestampAsync(config, cancellationToken);

        using var buffer = new MemoryStream();
        using (var json = new Utf8JsonWriter(buffer))
        {
            json.WriteStartObject();
            json.WriteString("videoId", videoId.ToString());
            json.WriteBoolean("contentCheckOk", true);
            json.WriteBoolean("racyCheckOk", true);
            json.WriteStartObject("context");
            foreach (var property in config.Context.EnumerateObject())
            {
                if (property.Name is not "client" and not "thirdParty")
                    property.WriteTo(json);
            }
            json.WriteStartObject("client");
            foreach (var property in config.Context.GetProperty("client").EnumerateObject())
            {
                if (property.Name is not "hl" and not "timeZone" and not "utcOffsetMinutes")
                    property.WriteTo(json);
            }
            json.WriteString("hl", "en");
            json.WriteString("timeZone", "UTC");
            json.WriteNumber("utcOffsetMinutes", 0);
            json.WriteEndObject();
            json.WriteStartObject("thirdParty");
            if (config.Context.TryGetProperty("thirdParty", out var thirdParty))
            {
                foreach (var property in thirdParty.EnumerateObject())
                {
                    if (property.Name != "embedUrl")
                        property.WriteTo(json);
                }
            }
            json.WriteString("embedUrl", EmbedOrigin);
            json.WriteEndObject();
            json.WriteEndObject();
            json.WriteStartObject("playbackContext");
            json.WriteStartObject("contentPlaybackContext");
            json.WriteString("html5Preference", "HTML5_PREF_WANTS");
            json.WriteNumber("signatureTimestamp", signatureTimestamp);
            if (config.EncryptedHostFlags is { } flags)
                json.WriteString("encryptedHostFlags", flags);
            json.WriteEndObject();
            json.WriteEndObject();
            json.WriteEndObject();
        }

        using var request = new YoutubeHttpRequest(
            HttpMethod.Post,
            "https://www.youtube.com/youtubei/v1/player?prettyPrint=false"
        )
        {
            UserSessionId = config.UserSessionId,
            IncludeApiKey = false,
            Content = new ByteArrayContent(buffer.GetBuffer(), 0, checked((int)buffer.Length)),
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        request.Headers.TryAddWithoutValidation(
            "User-Agent",
            config.UserAgent ?? EmbeddedUserAgent
        );
        request.Headers.Add("X-YouTube-Client-Name", "56");
        request.Headers.Add("X-YouTube-Client-Version", config.ClientVersion);
        if (config.VisitorData is { } visitorData)
            request.Headers.Add("X-Goog-Visitor-Id", visitorData);
        if (config.SessionIndex is { } sessionIndex)
            request.Headers.Add("X-Goog-AuthUser", sessionIndex);
        if (config.DelegatedSessionId is { } delegatedSessionId)
        {
            request.Headers.Add("X-Goog-PageId", delegatedSessionId);
            if (config.SessionIndex is null)
                request.Headers.Add("X-Goog-AuthUser", "0");
        }
        if (config.IsLoggedIn)
            request.Headers.Add("X-Youtube-Bootstrap-Logged-In", "true");

        using var response = await Http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var player = PlayerResponse.Parse(
            await response.Content.ReadAsStringAsync(cancellationToken)
        );
        if (!player.IsAvailable || !player.IsPlayable)
            throw new VideoUnplayableException(
                $"Video '{videoId}' captions could not be accessed: {player.PlayabilityError ?? "embedded player rejected the request"}."
            );
        return player;
    }

    public async ValueTask<ClosedCaptionTrackResponse> GetClosedCaptionTrackResponseAsync(
        string url,
        CancellationToken cancellationToken = default
    )
    {
        // Enforce known format
        var urlWithFormat = url.Pipe(s => UrlEx.RemoveQueryParameter(s, "xosf"))
            .Pipe(s => UrlEx.SetQueryParameter(s, "format", "3"))
            .Pipe(s => UrlEx.SetQueryParameter(s, "fmt", "3"));
        using var request = new YoutubeHttpRequest(HttpMethod.Get, urlWithFormat);
        using var response = await Http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return ClosedCaptionTrackResponse.Parse(
            await response.Content.ReadAsStringAsync(cancellationToken)
        );
    }
}

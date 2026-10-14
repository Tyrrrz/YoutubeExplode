using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Tests;

public class AuthenticatedCaptionSpecs
{
    private const string VideoId = "4Ff0xc9M8kA";
    private const string EmbedPath = "/embed/" + VideoId;
    private const string PlayerPath = "/youtubei/v1/player";
    private const string CaptionPath = "/api/timedtext";
    private const string ScriptPath = "/s/player/current/base.js";
    private const string UpdatedScriptPath = "/s/player/updated/base.js";

    private const string ConfigHtml = """
        <script>ytcfg.set({"unrelated":true});</script>
        <script>ytcfg.set({
          "INNERTUBE_CONTEXT": {
            "client": {"clientName":"WEB_EMBEDDED_PLAYER","clientVersion":"test-current-version","hl":"de","visitorData":"visitor","userAgent":"Safari/605.1.15,gzip(gfe)"},
            "request": {"useSsl":true},
            "thirdParty": {"embedUrl":"https://www.youtube.com/","embeddedPlayerContext":{"embeddedPlayerEncryptedContext":"encrypted-context"}}
          },
          "SESSION_INDEX": 2,
          "DATASYNC_ID": "channel-id||user-id",
          "LOGGED_IN": true,
          "STS": 12345,
          "WEB_PLAYER_CONTEXT_CONFIGS": {
            "WEB_PLAYER_CONTEXT_CONFIG_ID_EMBEDDED_PLAYER": {"encryptedHostFlags":"host-flags"}
          }
        });</script>
        """;

    private const string PlayerJson = """
        {
          "playabilityStatus":{"status":"OK"},
          "videoDetails":{"videoId":"4Ff0xc9M8kA"},
          "captions":{"playerCaptionsTracklistRenderer":{"captionTracks":[{
            "baseUrl":"https://www.youtube.com/api/timedtext?lang=en&xosf=1",
            "languageCode":"en","name":{"simpleText":"English"},"vssId":"a.en"
          }]}}
        }
        """;

    private static IReadOnlyList<Cookie> BrowserCookies()
    {
        var cookies = new List<Cookie>
        {
            new("LOGIN_INFO", "login", "/", ".youtube.com"),
            new("SAPISID", "sid", "/", ".youtube.com"),
            new("__Secure-1PAPISID", "first-party-sid", "/", ".youtube.com") { Secure = true },
            new("__Secure-3PAPISID", "third-party-sid", "/", ".youtube.com") { Secure = true },
        };
        // Exceed the old 20-cookie limit after the authentication cookies were inserted.
        for (var i = 0; i < 30; i++)
            cookies.Add(new Cookie($"browser-{i}", "value", "/", ".youtube.com"));
        return cookies;
    }

    [Fact]
    public async Task Authenticated_captions_keep_browser_cookies_and_need_only_three_requests()
    {
        using var handler = new CaptionHandler(
            async (request, cancellationToken) =>
            {
                request.Headers.GetValues("Cookie").Single().Should().Contain("LOGIN_INFO=login");
                request.Headers.GetValues("Cookie").Single().Should().Contain("browser-29=value");
                if (request.RequestUri!.AbsolutePath != PlayerPath)
                    return;

                request
                    .Headers.GetValues("X-YouTube-Client-Version")
                    .Single()
                    .Should()
                    .Be("test-current-version");
                request.Headers.GetValues("X-YouTube-Client-Name").Single().Should().Be("56");
                request.Headers.GetValues("X-Goog-Visitor-Id").Single().Should().Be("visitor");
                request.Headers.GetValues("X-Goog-PageId").Single().Should().Be("channel-id");
                request.Headers.GetValues("X-Goog-AuthUser").Single().Should().Be("2");
                request
                    .Headers.GetValues("X-Youtube-Bootstrap-Logged-In")
                    .Single()
                    .Should()
                    .Be("true");
                request.Content!.Headers.ContentType!.MediaType.Should().Be("application/json");
                request.RequestUri.Query.Should().NotContain("key=");

                using var json = JsonDocument.Parse(
                    await request.Content.ReadAsStringAsync(cancellationToken)
                );
                var context = json.RootElement.GetProperty("context");
                context
                    .GetProperty("thirdParty")
                    .GetProperty("embeddedPlayerContext")
                    .GetProperty("embeddedPlayerEncryptedContext")
                    .GetString()
                    .Should()
                    .Be("encrypted-context");
                context
                    .GetProperty("client")
                    .GetProperty("clientName")
                    .GetString()
                    .Should()
                    .Be("WEB_EMBEDDED_PLAYER");
                context.GetProperty("client").GetProperty("hl").GetString().Should().Be("en");
                context.GetProperty("request").GetProperty("useSsl").GetBoolean().Should().BeTrue();
                context
                    .GetProperty("thirdParty")
                    .GetProperty("embedUrl")
                    .GetString()
                    .Should()
                    .NotContain("youtube.com");
                var playback = json
                    .RootElement.GetProperty("playbackContext")
                    .GetProperty("contentPlaybackContext");
                playback.GetProperty("encryptedHostFlags").GetString().Should().Be("host-flags");
                playback.GetProperty("signatureTimestamp").GetInt32().Should().Be(12345);

                var authorization = request.Headers.GetValues("Authorization").Single().Split(' ');
                authorization.Should().HaveCount(6);
                foreach (
                    var (index, scheme, cookie) in new[]
                    {
                        (0, "SAPISIDHASH", "sid"),
                        (2, "SAPISID1PHASH", "first-party-sid"),
                        (4, "SAPISID3PHASH", "third-party-sid"),
                    }
                )
                {
                    authorization[index].Should().Be(scheme);
                    var parts = authorization[index + 1].Split('_');
                    parts.Should().HaveCount(3);
                    parts[2].Should().Be("u");
                    var expectedHash = Convert
                        .ToHexString(
                            SHA1.HashData(
                                Encoding.UTF8.GetBytes(
                                    $"user-id {parts[0]} {cookie} https://www.youtube.com"
                                )
                            )
                        )
                        .ToLowerInvariant();
                    parts[1].Should().Be(expectedHash);
                }
            }
        );
        using var http = new HttpClient(handler);
        using var youtube = new YoutubeClient(http, BrowserCookies());

        var manifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(VideoId);
        var captions = await youtube.Videos.ClosedCaptions.GetAsync(manifest.GetByLanguage("en"));

        captions.Captions.Should().ContainSingle().Which.Text.Should().Be("Hello & world");
        handler.Paths.Should().Equal(EmbedPath, PlayerPath, CaptionPath);
    }

    [Fact]
    public async Task The_player_timestamp_is_read_without_running_javascript_and_cached_by_script_url()
    {
        using var handler = new CaptionHandler
        {
            ConfigResponse = ConfigHtml.Replace(
                "\"STS\": 12345,",
                $"\"PLAYER_JS_URL\": \"{ScriptPath}\","
            ),
        };
        using var http = new HttpClient(handler);
        using var youtube = new YoutubeClient(http, BrowserCookies());

        await youtube.Videos.ClosedCaptions.GetManifestAsync(VideoId);
        await youtube.Videos.ClosedCaptions.GetManifestAsync(VideoId);
        handler.ConfigResponse = handler.ConfigResponse.Replace(ScriptPath, UpdatedScriptPath);
        await youtube.Videos.ClosedCaptions.GetManifestAsync(VideoId);

        handler
            .Paths.Should()
            .Equal(
                EmbedPath,
                ScriptPath,
                PlayerPath,
                EmbedPath,
                PlayerPath,
                EmbedPath,
                UpdatedScriptPath,
                PlayerPath
            );
        handler.SignatureTimestamps.Should().Equal(12345, 12345, 12346);
    }

    [Fact]
    public async Task A_missing_player_timestamp_fails_before_requesting_a_player_response()
    {
        using var handler = new CaptionHandler
        {
            ConfigResponse = ConfigHtml.Replace(
                "\"STS\": 12345,",
                $"\"PLAYER_JS_URL\": \"{ScriptPath}\","
            ),
            ScriptResponse = "var unrelated = 1;",
        };
        using var http = new HttpClient(handler);
        using var youtube = new YoutubeClient(http, BrowserCookies());

        var act = async () => await youtube.Videos.ClosedCaptions.GetManifestAsync(VideoId);
        await act.Should()
            .ThrowAsync<YoutubeExplodeException>()
            .WithMessage("*signature timestamp*");
        handler.Paths.Should().Equal(EmbedPath, ScriptPath);
    }

    [Fact]
    public async Task A_player_denial_preserves_the_reason_and_does_not_retry_mobile_clients()
    {
        using var handler = new CaptionHandler
        {
            PlayerResponse =
                """{"playabilityStatus":{"status":"LOGIN_REQUIRED","reason":"Sign in to confirm you’re not a bot"}}""",
        };
        using var http = new HttpClient(handler);
        using var youtube = new YoutubeClient(http, BrowserCookies());

        var act = async () => await youtube.Videos.ClosedCaptions.GetManifestAsync(VideoId);
        await act.Should()
            .ThrowAsync<VideoUnplayableException>()
            .WithMessage("*Sign in to confirm*");
        handler.Paths.Should().Equal(EmbedPath, PlayerPath);
    }

    [Theory]
    [InlineData(EmbedPath, 1)]
    [InlineData(PlayerPath, 2)]
    [InlineData(CaptionPath, 3)]
    public async Task Caption_server_errors_fail_without_repeated_requests(
        string failedPath,
        int expectedRequests
    )
    {
        using var handler = new CaptionHandler { FailedPath = failedPath };
        using var http = new HttpClient(handler);
        using var youtube = new YoutubeClient(http, BrowserCookies());

        var act = async () =>
        {
            var manifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(VideoId);
            await youtube.Videos.ClosedCaptions.GetAsync(manifest.GetByLanguage("en"));
        };
        await act.Should().ThrowAsync<HttpRequestException>();
        handler.Paths.Should().HaveCount(expectedRequests);
        handler.Paths.Count(path => path == failedPath).Should().Be(1);
    }

    [Fact]
    public async Task Canceling_the_bootstrap_stops_before_the_player_request()
    {
        using var cancellation = new CancellationTokenSource();
        using var handler = new CaptionHandler(
            (_, token) =>
            {
                cancellation.Cancel();
                token.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }
        );
        using var http = new HttpClient(handler);
        using var youtube = new YoutubeClient(http, BrowserCookies());

        var act = async () =>
            await youtube.Videos.ClosedCaptions.GetManifestAsync(VideoId, cancellation.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
        handler.Paths.Should().Equal(EmbedPath);
    }

    [Fact]
    public async Task Anonymous_captions_preserve_the_existing_fast_client_path()
    {
        using var handler = new CaptionHandler(
            async (request, token) =>
            {
                if (request.RequestUri!.AbsolutePath != PlayerPath)
                    return;
                using var json = JsonDocument.Parse(
                    await request.Content!.ReadAsStringAsync(token)
                );
                json.RootElement.GetProperty("context")
                    .GetProperty("client")
                    .GetProperty("clientName")
                    .GetString()
                    .Should()
                    .Be("VISIONOS");
            }
        );
        using var http = new HttpClient(handler);
        using var youtube = new YoutubeClient(http);

        var manifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(VideoId);
        await youtube.Videos.ClosedCaptions.GetAsync(manifest.GetByLanguage("en"));

        handler.Paths.Should().Equal("/sw.js_data", PlayerPath, CaptionPath);
    }

    private sealed class CaptionHandler(
        Func<HttpRequestMessage, CancellationToken, Task>? inspect = null
    ) : HttpMessageHandler
    {
        public List<string> Paths { get; } = [];
        public List<int> SignatureTimestamps { get; } = [];
        public string ConfigResponse { get; set; } = ConfigHtml;
        public string ScriptResponse { get; init; } = "var player={signatureTimestamp:12345};";
        public string PlayerResponse { get; init; } = PlayerJson;
        public string? FailedPath { get; init; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var path = request.RequestUri!.AbsolutePath;
            Paths.Add(path);
            if (inspect is not null)
                await inspect(request, cancellationToken);
            if (path == FailedPath)
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                {
                    RequestMessage = request,
                };

            if (path == PlayerPath)
            {
                using var json = JsonDocument.Parse(
                    await request.Content!.ReadAsStringAsync(cancellationToken)
                );
                if (json.RootElement.TryGetProperty("playbackContext", out var playback))
                    SignatureTimestamps.Add(
                        playback
                            .GetProperty("contentPlaybackContext")
                            .GetProperty("signatureTimestamp")
                            .GetInt32()
                    );
            }

            var body = path switch
            {
                EmbedPath => ConfigResponse,
                ScriptPath => ScriptResponse,
                UpdatedScriptPath => "var player={sts:12346};",
                PlayerPath => PlayerResponse,
                CaptionPath =>
                    "<timedtext><body><p t=\"0\" d=\"1000\">Hello &amp; world</p></body></timedtext>",
                "/sw.js_data" =>
                    "[[null,null,[[[null,null,null,null,null,null,null,null,null,null,null,null,null,\"visitor\"]]]]]",
                _ => throw new InvalidOperationException($"Unexpected network request: {path}"),
            };
            if (path == CaptionPath)
            {
                request.RequestUri.Query.Should().Contain("fmt=3");
                request.RequestUri.Query.Should().NotContain("xosf");
            }
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
                Content = new StringContent(body),
            };
        }
    }
}

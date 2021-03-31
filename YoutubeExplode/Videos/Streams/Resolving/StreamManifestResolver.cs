using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Videos.Streams.Resolving
{
    internal class StreamManifestResolver
    {
        private readonly HttpClient _httpClient;
        private readonly VideoId _videoId;
        private readonly Cache _cache = new();

        public StreamManifestResolver(HttpClient httpClient, VideoId videoId)
        {
            _httpClient = httpClient;
            _videoId = videoId;
        }

        private ValueTask<IHtmlDocument> GetEmbedPageAsync() => _cache.WrapAsync(async () =>
        {
            var url = $"https://youtube.com/embed/{_videoId}?hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            return Html.Parse(raw);
        });

        private ValueTask<JsonElement?> TryGetEmbedPagePlayerConfigAsync() =>
            _cache.WrapAsync(async () =>
            {
                var embedPage = await GetEmbedPageAsync();

                return

                    // Current
                    embedPage
                        .GetElementsByTagName("script")
                        .Select(e => e.Text())
                        .Select(s => Regex.Match(s, @"['""]PLAYER_CONFIG['""]\s*:\s*(\{.*\})").Groups[1].Value)
                        .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))?
                        .NullIfWhiteSpace()?
                        .Pipe(Json.Extract)
                        .Pipe(Json.Parse) ??

                    // Legacy
                    embedPage
                        .GetElementsByTagName("script")
                        .Select(e => e.Text())
                        .Select(s => Regex.Match(s, @"yt.setConfig\((\{.*\})").Groups[1].Value)
                        .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))?
                        .NullIfWhiteSpace()?
                        .Pipe(Json.Extract)
                        .Pipe(Json.Parse);
            });

        private ValueTask<IHtmlDocument> GetWatchPageAsync() => _cache.WrapAsync(async () =>
        {
            var url = $"https://youtube.com/watch?v={_videoId}&bpctr=9999999999&hl=en";
            var raw = await _httpClient.GetStringAsync(url);

            return Html.Parse(raw);
        });

        private ValueTask<JsonElement?> TryGetWatchPagePlayerConfigAsync() =>
            _cache.WrapAsync(async () =>
            {
                var watchPage = await GetWatchPageAsync();

                return watchPage
                    .GetElementsByTagName("script")
                    .Select(e => e.Text())
                    .Select(s => Regex.Match(s, @"ytplayer\.config\s*=\s*(\{.*\})").Groups[1].Value)
                    .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))?
                    .NullIfWhiteSpace()?
                    .Pipe(Json.Extract)
                    .Pipe(Json.TryParse);
            });

        private ValueTask<string> GetPlayerSourceUrl() => _cache.WrapAsync(async () =>
        {

        });

        private ValueTask<IReadOnlyList<IStreamInfo>> GetStreamInfosFromVideoInfoAsync() =>
            _cache.WrapAsync(async () =>
            {
                var embedPage = await GetEmbedPageAsync();
            });

        public ValueTask<IReadOnlyList<IStreamInfo>> GetStreamInfosAsync() => _cache.WrapAsync(async () =>
        {

        });
    }
}
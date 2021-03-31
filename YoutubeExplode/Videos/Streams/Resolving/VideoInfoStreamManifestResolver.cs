using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;
using YoutubeExplode.Videos.Streams.Resolving.Cipher;

namespace YoutubeExplode.Videos.Streams.Resolving
{
    internal class VideoInfoStreamManifestResolver
    {
        private readonly HttpClient _httpClient;
        private readonly VideoId _videoId;
        private readonly Cache _cache = new();

        public VideoInfoStreamManifestResolver(HttpClient httpClient, VideoId videoId)
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

        private ValueTask<JsonElement> GetPlayerConfigAsync() => _cache.WrapAsync(async () =>
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
                    .Pipe(Json.Parse) ??

                // TODO: add exceptions for all similar cases where result cannot be obtained
                throw FatalFailureException.Generic("Could not find player config.");
        });

        private ValueTask<string> GetPlayerSourceUrl() => _cache.WrapAsync(async () =>
        {
            var embedPage = await GetEmbedPageAsync();

            var sourceUrlFromEmbedPage = embedPage
                .GetElementsByTagName("script")
                .Select(e => e.GetAttribute("src"))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .FirstOrDefault(s =>
                    s.Contains("player_ias", StringComparison.OrdinalIgnoreCase) &&
                    s.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
                )?
                .Pipe(s => "https://youtube.com" + s);

            if (!string.IsNullOrWhiteSpace(sourceUrlFromEmbedPage))
                return sourceUrlFromEmbedPage;

            var playerConfig = await GetPlayerConfigAsync();

            var sourceUrlFromPlayerConfig = playerConfig
                .GetPropertyOrNull("assets")?
                .GetPropertyOrNull("js")?
                .GetString()
                .Pipe(s => "https://youtube.com" + s);

            if (!string.IsNullOrWhiteSpace(sourceUrlFromPlayerConfig))
                return sourceUrlFromPlayerConfig;

            throw FatalFailureException.Generic("Could not find player source URL.");
        });

        private ValueTask<string> GetPlayerSourceAsync() => _cache.WrapAsync(async () =>
        {
            var playerSourceUrl = await GetPlayerSourceUrl();
            return await _httpClient.GetStringAsync(playerSourceUrl);
        });

        private ValueTask<string> GetSignatureTimestampAsync() => _cache.WrapAsync(async () =>
        {
            var playerSource = await GetPlayerSourceAsync();

            return

                // Current
                playerSource
                    .Pipe(s => Regex.Match(s, @"(?<=invalid namespace.*?;[\w\s]+=)\d+").Value)
                    .NullIfWhiteSpace() ??

                // Legacy
                playerSource
                    .Pipe(s => Regex.Match(s, @"(?<=signatureTimestamp[=\:])\d+").Value)
                    .NullIfWhiteSpace() ??

                throw FatalFailureException.Generic("Could not find signature timestamp in player source.");
        });

        private ValueTask<IReadOnlyList<ICipherOperation>> GetCipherOperationsAsync() => _cache.WrapAsync(async () =>
        {
            var playerSource = await GetPlayerSourceAsync();

            var result = new List<ICipherOperation>();

            string? TryGetDeciphererFuncBody()
            {
                var funcName = Regex.Match(playerSource,
                        @"(\w+)=function\(\w+\){(\w+)=\2\.split\(\x22{2}\);.*?return\s+\2\.join\(\x22{2}\)}")
                    .Groups[0]
                    .Value;

                return funcName;
            }

            string? TryGetDeciphererDefinitionBody(string body)
            {
                var objName = Regex.Match(body, "([\\$_\\w]+).\\w+\\(\\w+,\\d+\\);")
                    .Groups[1]
                    .Value;

                var escapedObjName = Regex.Escape(objName);

                return Regex.Match(playerSource,
                        $@"var\s+{escapedObjName}=\{{(\w+:function\(\w+(,\w+)?\)\{{(.*?)\}}),?\}};",
                        RegexOptions.Singleline)
                    .Groups[0]
                    .Value
                    .NullIfWhiteSpace();
            }

            var deciphererFuncBody =
                TryGetDeciphererFuncBody() ??
                throw FatalFailureException.Generic("Could not find signature decipherer function body.");

            var deciphererDefinitionBody =
                TryGetDeciphererDefinitionBody(deciphererFuncBody) ??
                throw FatalFailureException.Generic("Could not find signature decipherer definition body.");

            // Analyze statements to determine cipher function names
            foreach (var statement in deciphererFuncBody.Split(";"))
            {
                // Get the name of the function called in this statement
                var calledFuncName = Regex.Match(statement, @"\w+(?:.|\[)(\""?\w+(?:\"")?)\]?\(").Groups[1].Value;
                if (string.IsNullOrWhiteSpace(calledFuncName))
                {
                    continue;
                }

                // Slice
                if (Regex.IsMatch(deciphererDefinitionBody,
                    $@"{Regex.Escape(calledFuncName)}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
                {
                    var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    result.Add(new SliceCipherOperation(index));
                }

                // Swap
                else if (Regex.IsMatch(deciphererDefinitionBody,
                    $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"))
                {
                    var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    result.Add(new SwapCipherOperation(index));
                }

                // Reverse
                else if (Regex.IsMatch(deciphererDefinitionBody,
                    $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\)"))
                {
                    result.Add(new ReverseCipherOperation());
                }
            }

            return (IReadOnlyList<ICipherOperation>) result;
        });

        private ValueTask<IReadOnlyDictionary<string, string>> GetVideoInfoAsync() => _cache.WrapAsync(async () =>
        {
            var signatureTimestamp = await GetSignatureTimestampAsync();

            var eurl = WebUtility.HtmlEncode($"https://youtube.googleapis.com/v/{_videoId}");

            var url =
                $"https://youtube.com/get_video_info?video_id={_videoId}&el=embedded&eurl={eurl}&hl=en&sts={signatureTimestamp}";

            var raw = await _httpClient.GetStringAsync(url);

            return (IReadOnlyDictionary<string, string>) Url.SplitQuery(raw);
        });

        private ValueTask<JsonElement> GetPlayerResponseAsync() => _cache.WrapAsync(async () =>
        {
            var videoInfo = await GetVideoInfoAsync();

            return Json.Parse(videoInfo["player_response"]);
        });

        public ValueTask<IReadOnlyList<IStreamInfo>> GetStreamInfosAsync() => _cache.WrapAsync(async () =>
        {

        });
    }
}
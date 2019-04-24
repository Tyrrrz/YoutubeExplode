using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class VideoWatchPageParser : Cached
    {
        private readonly IHtmlDocument _root;

        public VideoWatchPageParser(IHtmlDocument root)
        {
            _root = root;
        }

        private JToken GetPlayerConfig() => Cache(() =>
        {
            var raw = Regex.Match(_root.Source.Text,
                    @"ytplayer\.config = (?<Json>\{[^\{\}]*(((?<Open>\{)[^\{\}]*)+((?<Close-Open>\})[^\{\}]*)+)*(?(Open)(?!))\})")
                .Groups["Json"].Value;
            return JToken.Parse(raw);
        });

        private JToken GetPlayerResponse() => Cache(() =>
        {
            // Player response is a json, which is stored as a string, inside json
            var raw = GetPlayerConfig().SelectToken("args.player_response").Value<string>();
            return JToken.Parse(raw);
        });

        public string TryGetErrorReason() => Cache(() => _root.QuerySelector("div#unavailable-submessage")?.TextContent);

        public string TryGetPreviewVideoId() => Cache(() => GetPlayerConfig().SelectToken("args.ypc_vid")?.Value<string>());

        public DateTimeOffset GetVideoUploadDate() => Cache(() =>
            _root.QuerySelector("meta[itemprop=\"datePublished\"]").GetAttribute("content").ParseDateTimeOffset("yyyy-MM-dd"));

        public string GetVideoDescription() => Cache(() =>
        {
            var buffer = new StringBuilder();

            foreach (var childNode in _root.QuerySelector("p#eow-description").ChildNodes)
            {
                // If it's a text node - display text content
                if (childNode.NodeType == NodeType.Text)
                {
                    buffer.Append(childNode.TextContent);
                }
                // If it's an anchor node - perform some special transformation
                else if (childNode is IHtmlAnchorElement anchorNode)
                {
                    // If the link appears shortened - get full link
                    if (anchorNode.TextContent.EndsWith("...", StringComparison.OrdinalIgnoreCase))
                    {
                        // Get href
                        var href = anchorNode.GetAttribute("href");

                        // If it's a relative link that goes through YouTube redirect - extract the actual link
                        if (href.StartsWith("/redirect", StringComparison.OrdinalIgnoreCase))
                        {
                            // Get query parameters
                            var queryParams = UrlEx.SplitQuery(anchorNode.Search);

                            // Get the actual href
                            href = queryParams["q"];
                        }
                        // If it's a relative link - prepend YouTube's host
                        else if (href.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                        {
                            // Prepend host to the link to make it absolute
                            href = "https://youtube.com" + anchorNode.GetAttribute("href");
                        }

                        buffer.Append(href);
                    }
                    // Otherwise - just use its inner text
                    else
                    {
                        buffer.Append(anchorNode.TextContent);
                    }
                }
                // If it's a break row node - append new line
                else if (childNode is IHtmlBreakRowElement)
                {
                    buffer.AppendLine();
                }
            }

            return buffer.ToString();
        });

        public long? TryGetVideoViewCount() => Cache(() =>
            _root.QuerySelector("meta[itemprop=\"interactionCount\"]")?.GetAttribute("content").ParseLongOrDefault());

        public long? TryGetVideoLikeCount() => Cache(() =>
            _root.QuerySelector("button.like-button-renderer-like-button")?.Text().StripNonDigit().ParseLongOrDefault());

        public long? TryGetVideoDislikeCount() => Cache(() =>
            _root.QuerySelector("button.like-button-renderer-dislike-button")?.Text().StripNonDigit().ParseLongOrDefault());

        public TimeSpan GetExpiresIn() => Cache(() =>
            TimeSpan.FromSeconds(GetPlayerResponse().SelectToken("streamingData.expiresInSeconds").Value<double>()));

        public string TryGetDashManifestUrl() =>
            Cache(() => GetPlayerResponse().SelectToken("streamingData.dashManifestUrl")?.Value<string>());

        public string TryGetHlsManifestUrl() =>
            Cache(() => GetPlayerResponse().SelectToken("streamingData.hlsManifestUrl")?.Value<string>());

        public IReadOnlyList<UrlEncodedStreamInfoParser> GetMuxedStreamInfos() => Cache(() =>
        {
            var streamInfosEncoded = GetPlayerConfig().SelectToken("args.url_encoded_fmt_stream_map").Value<string>();

            if (streamInfosEncoded.IsNullOrWhiteSpace())
                return new UrlEncodedStreamInfoParser[0];

            return streamInfosEncoded.Split(",")
                .Select(UrlEx.SplitQuery)
                .Select(d => new UrlEncodedStreamInfoParser(d))
                .ToArray();
        });

        public IReadOnlyList<UrlEncodedStreamInfoParser> GetAdaptiveStreamInfos() => Cache(() =>
        {
            var streamInfosEncoded = GetPlayerConfig().SelectToken("args.adaptive_fmts").Value<string>();

            if (streamInfosEncoded.IsNullOrWhiteSpace())
                return new UrlEncodedStreamInfoParser[0];

            return streamInfosEncoded.Split(",")
                .Select(UrlEx.SplitQuery)
                .Select(d => new UrlEncodedStreamInfoParser(d))
                .ToArray();
        });
    }

    internal partial class VideoWatchPageParser
    {
        public static VideoWatchPageParser Initialize(string raw)
        {
            var root = new HtmlParser().Parse(raw);
            return new VideoWatchPageParser(root);
        }
    }
}
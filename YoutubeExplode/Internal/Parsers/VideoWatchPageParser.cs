﻿using System;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class VideoWatchPageParser
    {
        private readonly IHtmlDocument _root;

        public VideoWatchPageParser(IHtmlDocument root)
        {
            _root = root;
        }

        public DateTimeOffset ParseUploadDate() => _root.QuerySelector("meta[itemprop=\"datePublished\"]")
            .GetAttribute("content").ParseDateTimeOffset("yyyy-MM-dd");

        public string ParseDescription()
        {
            var buffer = new StringBuilder();

            var descriptionNode = _root.QuerySelector("p#eow-description");
            var childNodes = descriptionNode.ChildNodes;

            foreach (var childNode in childNodes)
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
        }

        public long ParseViewCount() => _root.QuerySelector("meta[itemprop=\"interactionCount\"]")
            ?.GetAttribute("content").ParseLongOrDefault() ?? 0;

        public long ParseLikeCount() => _root.QuerySelector("button.like-button-renderer-like-button")?.Text()
            .StripNonDigit().ParseLongOrDefault() ?? 0;

        public long ParseDislikeCount() => _root.QuerySelector("button.like-button-renderer-dislike-button")?.Text()
            .StripNonDigit().ParseLongOrDefault() ?? 0;

        private string ParseConfigRaw() => Regex.Match(_root.Source.Text,
                @"ytplayer\.config = (?<Json>\{[^\{\}]*(((?<Open>\{)[^\{\}]*)+((?<Close-Open>\})[^\{\}]*)+)*(?(Open)(?!))\})")
            .Groups["Json"].Value;

        public bool ParseIsConfigAvailable() => ParseConfigRaw().IsNotBlank();

        public ConfigParser GetConfig()
        {
            var configRaw = ParseConfigRaw();
            var configJson = JToken.Parse(configRaw);

            return new ConfigParser(configJson);
        }
    }

    internal partial class VideoWatchPageParser
    {
        public class ConfigParser
        {
            private readonly JToken _root;

            public ConfigParser(JToken root)
            {
                _root = root;
            }

            public string ParsePreviewVideoId() => _root.SelectToken("args.ypc_vid")?.Value<string>();

            public PlayerResponseParser GetPlayerResponse()
            {
                // Player response is a json, which is stored as a string, inside json
                var playerResponseRaw = _root.SelectToken("args.player_response").Value<string>();
                var playerResponseJson = JToken.Parse(playerResponseRaw);

                return new PlayerResponseParser(playerResponseJson);
            }
        }
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
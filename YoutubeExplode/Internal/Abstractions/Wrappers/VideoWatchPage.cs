using System;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using Newtonsoft.Json.Linq;
using YoutubeExplode.Internal.Abstractions.Wrappers.Shared;

namespace YoutubeExplode.Internal.Abstractions.Wrappers
{
    internal partial class VideoWatchPage
    {
        private readonly IHtmlDocument _root;

        public VideoWatchPage(IHtmlDocument root)
        {
            _root = root;
        }

        public string TryGetErrorReason() => _root.QuerySelector("div#unavailable-submessage")?.TextContent;

        public DateTimeOffset GetVideoUploadDate() => _root.QuerySelector("meta[itemprop=\"datePublished\"]").GetAttribute("content")
            .ParseDateTimeOffset("yyyy-MM-dd");

        public string GetVideoDescription()
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
        }

        public long? TryGetVideoViewCount() =>
            _root.QuerySelector("meta[itemprop=\"interactionCount\"]")?.GetAttribute("content").ParseLongOrDefault();

        public long? TryGetVideoLikeCount() => _root.QuerySelector("button.like-button-renderer-like-button")?.Text()
            .StripNonDigit().ParseLongOrDefault();

        public long? TryGetVideoDislikeCount() => _root.QuerySelector("button.like-button-renderer-dislike-button")?.Text()
            .StripNonDigit().ParseLongOrDefault();

        public PlayerConfig GetPlayerConfig()
        {
            var configRaw = Regex.Match(_root.Source.Text,
                    @"ytplayer\.config = (?<Json>\{[^\{\}]*(((?<Open>\{)[^\{\}]*)+((?<Close-Open>\})[^\{\}]*)+)*(?(Open)(?!))\})")
                .Groups["Json"].Value;
            var configJson = JToken.Parse(configRaw);

            return new PlayerConfig(configJson);
        }
    }

    internal partial class VideoWatchPage
    {
        public static VideoWatchPage Initialize(string raw)
        {
            var root = new HtmlParser().Parse(raw);
            return new VideoWatchPage(root);
        }
    }
}
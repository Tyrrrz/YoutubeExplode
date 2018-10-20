using System;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;

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
                if (childNode.NodeType == NodeType.Text)
                {
                    buffer.Append(childNode.TextContent);
                }
                else if (childNode is IHtmlAnchorElement anchorNode)
                {
                    // If it uses YouTube redirect - get the actual link
                    if (anchorNode.PathName.Equals("/redirect", StringComparison.OrdinalIgnoreCase))
                    {
                        // Get query parameters
                        var queryParams = UrlEx.SplitQuery(anchorNode.Search);

                        // Get the actual href
                        var actualHref = queryParams["q"].UrlDecode();

                        buffer.Append(actualHref);
                    }
                    else
                    {
                        buffer.Append(anchorNode.TextContent);
                    }
                }
                else if (childNode is IHtmlBreakRowElement)
                {
                    buffer.AppendLine();
                }
            }

            return buffer.ToString();
        }

        public long ParseLikeCount() => _root.QuerySelector("button.like-button-renderer-like-button").Text()
            .StripNonDigit().ParseLongOrDefault();

        public long ParseDislikeCount() => _root.QuerySelector("button.like-button-renderer-dislike-button").Text()
            .StripNonDigit().ParseLongOrDefault();
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
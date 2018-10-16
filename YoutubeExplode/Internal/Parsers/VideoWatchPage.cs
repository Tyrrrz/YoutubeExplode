using System;
using System.Text;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class VideoWatchPage
    {
        private readonly IHtmlDocument _root;

        public VideoWatchPage(IHtmlDocument root)
        {
            _root = root;
        }

        public DateTimeOffset GetUploadDate() => _root.QuerySelector("meta[itemprop=\"datePublished\"]")
            .GetAttribute("content").ParseDateTimeOffset("yyyy-MM-dd");

        public long GetLikeCount() => _root.QuerySelector("button.like-button-renderer-like-button").Text()
            .StripNonDigit().ParseLongOrDefault();

        public long GetDislikeCount() => _root.QuerySelector("button.like-button-renderer-dislike-button").Text()
            .StripNonDigit().ParseLongOrDefault();

        public string GetDescription()
        {
            var buffer = new StringBuilder();

            var descriptionNode = _root.QuerySelector("p#eow-description");
            var childNodes = descriptionNode.ChildNodes;

            foreach (var childNode in childNodes)
            {
                if (childNode.NodeType == NodeType.Text)
                    buffer.Append(childNode.TextContent);

                else if (childNode is IHtmlAnchorElement anchorNode)
                    buffer.Append(anchorNode.Href);

                else if (childNode is IHtmlBreakRowElement)
                    buffer.AppendLine();
            }

            return buffer.ToString();
        }
    }

    internal partial class VideoWatchPage
    {
        public static VideoWatchPage Parse(string raw)
        {
            var root = new HtmlParser().Parse(raw);
            return new VideoWatchPage(root);
        }
    }
}
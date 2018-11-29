using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class ChannelPageParser
    {
        private readonly IHtmlDocument _root;

        public ChannelPageParser(IHtmlDocument root)
        {
            _root = root;
        }

        public bool ParseIsAvailable() => _root.QuerySelector("meta[property=\"og:url\"]") != null;

        public string ParseChannelUrl() => _root.QuerySelector("meta[property=\"og:url\"]")
            .GetAttribute("content");

        public string ParseChannelId() => ParseChannelUrl().SubstringAfter("channel/");

        public string ParseChannelTitle() => _root.QuerySelector("meta[property=\"og:title\"]")
            .GetAttribute("content");

        public string ParseChannelLogoUrl() => _root.QuerySelector("meta[property=\"og:image\"]")
            .GetAttribute("content");
    }

    internal partial class ChannelPageParser
    {
        public static ChannelPageParser Initialize(string raw)
        {
            var root = new HtmlParser().Parse(raw);
            return new ChannelPageParser(root);
        }
    }
}
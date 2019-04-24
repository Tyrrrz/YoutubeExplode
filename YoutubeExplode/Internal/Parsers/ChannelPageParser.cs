using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class ChannelPageParser : Cached
    {
        private readonly IHtmlDocument _root;

        public ChannelPageParser(IHtmlDocument root)
        {
            _root = root;
        }

        public bool Validate() => Cache(() => _root.QuerySelector("meta[property=\"og:url\"]") != null);

        public string GetChannelUrl() => Cache(() => _root.QuerySelector("meta[property=\"og:url\"]").GetAttribute("content"));

        public string GetChannelId() => Cache(() => GetChannelUrl().SubstringAfter("channel/"));

        public string GetChannelTitle() => Cache(() => _root.QuerySelector("meta[property=\"og:title\"]").GetAttribute("content"));

        public string GetChannelLogoUrl() => Cache(() => _root.QuerySelector("meta[property=\"og:image\"]").GetAttribute("content"));
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
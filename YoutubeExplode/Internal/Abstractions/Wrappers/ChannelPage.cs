using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;

namespace YoutubeExplode.Internal.Abstractions.Wrappers
{
    internal partial class ChannelPage
    {
        private readonly IHtmlDocument _root;

        public ChannelPage(IHtmlDocument root)
        {
            _root = root;
        }

        public bool Validate() => _root.QuerySelector("meta[property=\"og:url\"]") != null;

        public string GetChannelUrl() => _root.QuerySelector("meta[property=\"og:url\"]").GetAttribute("content");

        public string GetChannelId() => GetChannelUrl().SubstringAfter("channel/");

        public string GetChannelTitle() => _root.QuerySelector("meta[property=\"og:title\"]").GetAttribute("content");

        public string GetChannelLogoUrl() => _root.QuerySelector("meta[property=\"og:image\"]").GetAttribute("content");
    }

    internal partial class ChannelPage
    {
        public static ChannelPage Initialize(string raw)
        {
            var root = new HtmlParser().Parse(raw);
            return new ChannelPage(root);
        }
    }
}
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace YoutubeExplode.Internal
{
    internal static class Html
    {
        private static readonly HtmlParser HtmlParser = new HtmlParser();

        public static IHtmlDocument Parse(string source) => HtmlParser.ParseDocument(source);
    }
}
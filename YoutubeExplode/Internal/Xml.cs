using System.Xml.Linq;
using YoutubeExplode.Internal.Extensions;

namespace YoutubeExplode.Internal
{
    internal static class Xml
    {
        public static XElement Parse(string source) => XElement.Parse(source, LoadOptions.PreserveWhitespace).StripNamespaces();
    }
}
using System.Xml.Linq;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Utils;

internal static class Xml
{
    public static XElement Parse(string source) =>
        XElement.Parse(source, LoadOptions.PreserveWhitespace).StripNamespaces();
}
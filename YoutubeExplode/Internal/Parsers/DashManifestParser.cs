using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class DashManifestParser : Cached
    {
        private readonly XElement _root;

        public DashManifestParser(XElement root)
        {
            _root = root;
        }

        public IReadOnlyList<DashStreamInfoParser> GetStreamInfos() => Cache(() =>
        {
            // Get all stream info nodes
            var streamInfosXml = _root.Descendants("Representation");

            // Filter out partial streams
            streamInfosXml = streamInfosXml.Where(s =>
                s.Descendants("Initialization").FirstOrDefault()?.Attribute("sourceURL")?.Value.Contains("sq/") != true);

            return streamInfosXml.Select(x => new DashStreamInfoParser(x)).ToArray();
        });

    }

    internal partial class DashManifestParser
    {
        public static DashManifestParser Initialize(string raw)
        {
            var root = XElement.Parse(raw).StripNamespaces();
            return new DashManifestParser(root);
        }
    }
}
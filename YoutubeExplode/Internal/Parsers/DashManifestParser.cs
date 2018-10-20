using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class DashManifestParser
    {
        private readonly XElement _root;

        public DashManifestParser(XElement root)
        {
            _root = root;
        }

        public IEnumerable<DashStreamInfoParser> DashStreamInfos()
        {
            var streamInfosXml = _root.Descendants("Representation");

            // Filter out partial streams
            streamInfosXml = streamInfosXml.Where(s =>
                s.Descendants("Initialization").FirstOrDefault()?.Attribute("sourceURL")?.Value.Contains("sq/") != true);

            return streamInfosXml.Select(x => new DashStreamInfoParser(x));
        }
    }

    internal partial class DashManifestParser
    {
        public class DashStreamInfoParser
        {
            private readonly XElement _root;

            public DashStreamInfoParser(XElement root)
            {
                _root = root;
            }

            public string GetUrl() => (string) _root.Element("BaseURL");

            public int GetItag() => (int) _root.Attribute("id");

            public long GetBitrate() => (long) _root.Attribute("bandwidth");

            public long GetContentLength() => Regex.Match(GetUrl(), @"clen[/=](\d+)").Groups[1].Value.ParseLong();

            public bool GetIsAudioOnly() => _root.Element("AudioChannelConfiguration") != null;

            public int GetWidth() => (int) _root.Attribute("width");

            public int GetHeight() => (int) _root.Attribute("height");

            public int GetFramerate() => (int) _root.Attribute("frameRate");
        }
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
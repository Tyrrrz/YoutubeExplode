using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace YoutubeExplode.Internal.Abstractions.Wrappers
{
    internal partial class DashManifest
    {
        private readonly XElement _root;

        public DashManifest(XElement root)
        {
            _root = root;
        }

        public IEnumerable<StreamInfo> GetStreamInfos()
        {
            var streamInfosXml = _root.Descendants("Representation");

            // Filter out partial streams
            streamInfosXml = streamInfosXml.Where(s =>
                s.Descendants("Initialization").FirstOrDefault()?.Attribute("sourceURL")?.Value.Contains("sq/") != true);

            return streamInfosXml.Select(x => new StreamInfo(x));
        }
    }

    internal partial class DashManifest
    {
        public class StreamInfo
        {
            private readonly XElement _root;

            public StreamInfo(XElement root)
            {
                _root = root;
            }

            public int GetItag() => (int) _root.Attribute("id");

            public string GetUrl() => (string) _root.Element("BaseURL");

            public long GetContentLength() => Regex.Match(GetUrl(), @"clen[/=](\d+)").Groups[1].Value.ParseLong();

            public long GetBitrate() => (long) _root.Attribute("bandwidth");

            public string GetContainer() => Regex.Match(GetUrl(), @"mime[/=]\w*%2F([\w\d]*)").Groups[1].Value.UrlDecode();

            public string GetEncoding() => (string) _root.Attribute("codecs");

            public bool GetIsAudioOnly() => _root.Element("AudioChannelConfiguration") != null;

            public int GetWidth() => (int) _root.Attribute("width");

            public int GetHeight() => (int) _root.Attribute("height");

            public int GetFramerate() => (int) _root.Attribute("frameRate");
        }
    }

    internal partial class DashManifest
    {
        public static DashManifest Initialize(string raw)
        {
            var root = XElement.Parse(raw).StripNamespaces();
            return new DashManifest(root);
        }
    }
}
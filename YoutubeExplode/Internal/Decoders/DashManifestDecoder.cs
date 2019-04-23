using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace YoutubeExplode.Internal.Decoders
{
    internal partial class DashManifestDecoder : DecoderBase
    {
        private readonly XElement _root;

        public DashManifestDecoder(XElement root)
        {
            _root = root;
        }

        public IReadOnlyList<StreamInfoDecoder> GetStreamInfos() => Cache(() =>
        {
            var streamInfosXml = _root.Descendants("Representation");

            // Filter out partial streams
            streamInfosXml = streamInfosXml.Where(s =>
                s.Descendants("Initialization").FirstOrDefault()?.Attribute("sourceURL")?.Value.Contains("sq/") != true);

            return streamInfosXml.Select(x => new StreamInfoDecoder(x)).ToArray();
        });

    }

    internal partial class DashManifestDecoder
    {
        public class StreamInfoDecoder : DecoderBase
        {
            private readonly XElement _root;

            public StreamInfoDecoder(XElement root)
            {
                _root = root;
            }

            public int GetItag() => Cache(() => (int) _root.Attribute("id"));

            public string GetUrl() => Cache(() => (string) _root.Element("BaseURL"));

            public long GetContentLength() => Cache(() => Regex.Match(GetUrl(), @"clen[/=](\d+)").Groups[1].Value.ParseLong());

            public long GetBitrate() => Cache(() => (long) _root.Attribute("bandwidth"));

            public string GetContainer() => Cache(() => Regex.Match(GetUrl(), @"mime[/=]\w*%2F([\w\d]*)").Groups[1].Value.UrlDecode());

            public string GetEncoding() => Cache(() => (string) _root.Attribute("codecs"));

            public bool GetIsAudioOnly() => Cache(() => _root.Element("AudioChannelConfiguration") != null);

            public int GetWidth() => Cache(() => (int) _root.Attribute("width"));

            public int GetHeight() => Cache(() => (int) _root.Attribute("height"));

            public int GetFramerate() => Cache(() => (int) _root.Attribute("frameRate"));
        }
    }

    internal partial class DashManifestDecoder
    {
        public static DashManifestDecoder Initialize(string raw)
        {
            var root = XElement.Parse(raw).StripNamespaces();
            return new DashManifestDecoder(root);
        }
    }
}
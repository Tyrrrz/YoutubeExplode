using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal class DashStreamInfoParser : Cached
    {
        private readonly XElement _root;

        public DashStreamInfoParser(XElement root)
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
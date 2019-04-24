using System;
using System.Xml.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal class ClosedCaptionParser : Cached
    {
        private readonly XElement _root;

        public ClosedCaptionParser(XElement root)
        {
            _root = root;
        }

        public string GetText() => Cache(() => (string) _root);

        public TimeSpan GetOffset() => Cache(() => TimeSpan.FromMilliseconds((double) _root.Attribute("t")));

        public TimeSpan GetDuration() => Cache(() => TimeSpan.FromMilliseconds((double) _root.Attribute("d")));
    }
}
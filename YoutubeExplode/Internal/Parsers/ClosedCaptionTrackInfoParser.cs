using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class ClosedCaptionTrackInfoParser
    {
        private readonly XElement _root;

        public ClosedCaptionTrackInfoParser(XElement root)
        {
            _root = root;
        }

        public IEnumerable<TrackParser> Tracks()
        {
            return _root.Descendants("p").Select(x => new TrackParser(x));
        }
    }

    internal partial class ClosedCaptionTrackInfoParser
    {
        public class TrackParser
        {
            private readonly XElement _root;

            public TrackParser(XElement root)
            {
                _root = root;
            }

            public string GetText() => (string) _root;

            public TimeSpan GetOffset() => TimeSpan.FromMilliseconds((double) _root.Attribute("t"));

            public TimeSpan GetDuration() => TimeSpan.FromMilliseconds((double) _root.Attribute("d"));
        }
    }

    internal partial class ClosedCaptionTrackInfoParser
    {
        public static ClosedCaptionTrackInfoParser Initialize(string raw)
        {
            var root = XElement.Parse(raw).StripNamespaces();
            return new ClosedCaptionTrackInfoParser(root);
        }
    }
}
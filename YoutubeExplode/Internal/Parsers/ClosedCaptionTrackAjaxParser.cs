using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class ClosedCaptionTrackAjaxParser
    {
        private readonly XElement _root;

        public ClosedCaptionTrackAjaxParser(XElement root)
        {
            _root = root;
        }

        public IEnumerable<ClosedCaptionParser> GetClosedCaptions()
        {
            return _root.Descendants("p").Select(x => new ClosedCaptionParser(x));
        }
    }

    internal partial class ClosedCaptionTrackAjaxParser
    {
        public class ClosedCaptionParser
        {
            private readonly XElement _root;

            public ClosedCaptionParser(XElement root)
            {
                _root = root;
            }

            public string ParseText() => (string) _root;

            public TimeSpan ParseOffset() => TimeSpan.FromMilliseconds((double) _root.Attribute("t"));

            public TimeSpan ParseDuration() => TimeSpan.FromMilliseconds((double) _root.Attribute("d"));
        }
    }

    internal partial class ClosedCaptionTrackAjaxParser
    {
        public static ClosedCaptionTrackAjaxParser Initialize(string raw)
        {
            // Parse with whitespace because we're interested in inner text
            var root = XElement.Parse(raw, LoadOptions.PreserveWhitespace).StripNamespaces();
            return new ClosedCaptionTrackAjaxParser(root);
        }
    }
}
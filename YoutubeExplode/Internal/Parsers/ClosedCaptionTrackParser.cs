using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class ClosedCaptionTrackParser : Cached
    {
        private readonly XElement _root;

        public ClosedCaptionTrackParser(XElement root)
        {
            _root = root;
        }

        public IReadOnlyList<ClosedCaptionParser> GetClosedCaptions() =>
            Cache(() => _root.Descendants("p").Select(x => new ClosedCaptionParser(x)).ToArray());
    }

    internal partial class ClosedCaptionTrackParser
    {
        public static ClosedCaptionTrackParser Initialize(string raw)
        {
            // Parse while preserving whitespace in inner text of nodes
            var root = XElement.Parse(raw, LoadOptions.PreserveWhitespace).StripNamespaces();
            return new ClosedCaptionTrackParser(root);
        }
    }
}
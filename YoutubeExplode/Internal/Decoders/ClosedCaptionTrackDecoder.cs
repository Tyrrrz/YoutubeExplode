using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace YoutubeExplode.Internal.Decoders
{
    internal partial class ClosedCaptionTrackDecoder : DecoderBase
    {
        private readonly XElement _root;

        public ClosedCaptionTrackDecoder(XElement root)
        {
            _root = root;
        }

        public IReadOnlyList<ClosedCaptionDecoder> GetClosedCaptions() =>
            Cache(() => _root.Descendants("p").Select(x => new ClosedCaptionDecoder(x)).ToArray());
    }

    internal partial class ClosedCaptionTrackDecoder
    {
        public class ClosedCaptionDecoder : DecoderBase
        {
            private readonly XElement _root;

            public ClosedCaptionDecoder(XElement root)
            {
                _root = root;
            }

            public string GetText() => Cache(() => (string) _root);

            public TimeSpan GetOffset() => Cache(() => TimeSpan.FromMilliseconds((double) _root.Attribute("t")));

            public TimeSpan GetDuration() => Cache(() => TimeSpan.FromMilliseconds((double) _root.Attribute("d")));
        }
    }

    internal partial class ClosedCaptionTrackDecoder
    {
        public static ClosedCaptionTrackDecoder Initialize(string raw)
        {
            // Parse with whitespace because we're interested in inner text
            var root = XElement.Parse(raw, LoadOptions.PreserveWhitespace).StripNamespaces();
            return new ClosedCaptionTrackDecoder(root);
        }
    }
}
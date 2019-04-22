using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace YoutubeExplode.Internal.Abstractions.Wrappers
{
    internal partial class ClosedCaptionTrackAjax
    {
        private readonly XElement _root;

        public ClosedCaptionTrackAjax(XElement root)
        {
            _root = root;
        }

        public IEnumerable<ClosedCaptionWrapper> GetClosedCaptions() => _root.Descendants("p").Select(x => new ClosedCaptionWrapper(x));
    }

    internal partial class ClosedCaptionTrackAjax
    {
        public class ClosedCaptionWrapper
        {
            private readonly XElement _root;

            public ClosedCaptionWrapper(XElement root)
            {
                _root = root;
            }

            public string GetText() => (string)_root;

            public TimeSpan GetOffset() => TimeSpan.FromMilliseconds((double)_root.Attribute("t"));

            public TimeSpan GetDuration() => TimeSpan.FromMilliseconds((double)_root.Attribute("d"));
        }
    }

    internal partial class ClosedCaptionTrackAjax
    {
        public static ClosedCaptionTrackAjax Initialize(string raw)
        {
            // Parse with whitespace because we're interested in inner text
            var root = XElement.Parse(raw, LoadOptions.PreserveWhitespace).StripNamespaces();
            return new ClosedCaptionTrackAjax(root);
        }
    }
}
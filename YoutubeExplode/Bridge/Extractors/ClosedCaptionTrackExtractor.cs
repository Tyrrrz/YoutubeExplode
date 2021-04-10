using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Bridge.Extractors
{
    internal partial class ClosedCaptionTrackExtractor
    {
        private readonly XElement _content;
        private readonly Memo _memo = new();

        public ClosedCaptionTrackExtractor(XElement content) => _content = content;

        public IReadOnlyList<ClosedCaptionExtractor> GetClosedCaptions() => _memo.Wrap(() =>
            _content
                .Descendants("p")
                .Select(x => new ClosedCaptionExtractor(x))
                .ToArray()
        );
    }

    internal partial class ClosedCaptionTrackExtractor
    {
        public static ClosedCaptionTrackExtractor Create(string raw)
        {
            var content = Xml.Parse(raw);
            return new ClosedCaptionTrackExtractor(content);
        }
    }
}
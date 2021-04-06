using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Extraction.Responses
{
    internal class ClosedCaptionTrackResponse
    {
        private readonly XElement _root;
        private readonly Memo _memo = new();

        public ClosedCaptionTrackResponse(XElement root) => _root = root;

        public IReadOnlyList<ClosedCaptionResponse> GetClosedCaptions() => _memo.Wrap(() =>
            _root
                .Descendants("p")
                .Select(x => new ClosedCaptionResponse(x))
                .ToArray()
        );
    }
}
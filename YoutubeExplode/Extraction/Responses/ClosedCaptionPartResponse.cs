using System;
using System.Xml.Linq;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Extraction.Responses
{
    internal class ClosedCaptionPartResponse
    {
        private readonly XElement _root;
        private readonly Memo _memo = new();

        public ClosedCaptionPartResponse(XElement root) => _root = root;

        public string? TryGetText() => _memo.Wrap(() =>
            (string?) _root
        );

        public TimeSpan? TryGetOffset() => _memo.Wrap(() =>
            ((double?) _root.Attribute("t"))?.Pipe(TimeSpan.FromMilliseconds)
        );
    }
}
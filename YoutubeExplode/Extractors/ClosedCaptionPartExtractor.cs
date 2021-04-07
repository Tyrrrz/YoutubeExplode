using System;
using System.Xml.Linq;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Extractors
{
    internal class ClosedCaptionPartExtractor
    {
        private readonly XElement _content;
        private readonly Memo _memo = new();

        public ClosedCaptionPartExtractor(XElement content) => _content = content;

        public string? TryGetText() => _memo.Wrap(() =>
            (string?) _content
        );

        public TimeSpan? TryGetOffset() => _memo.Wrap(() =>
            ((double?) _content.Attribute("t"))?.Pipe(TimeSpan.FromMilliseconds)
        );
    }
}
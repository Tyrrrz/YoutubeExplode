using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Extraction.Responses
{
    internal class ClosedCaptionResponse
    {
        private readonly XElement _root;
        private readonly Memo _memo = new();

        public ClosedCaptionResponse(XElement root) => _root = root;

        public string? TryGetText() => _memo.Wrap(() =>
            (string?) _root
        );

        public TimeSpan? TryGetOffset() => _memo.Wrap(() =>
            ((double?) _root.Attribute("t"))?.Pipe(TimeSpan.FromMilliseconds)
        );

        public TimeSpan? TryGetDuration() => _memo.Wrap(() =>
            ((double?) _root.Attribute("d"))?.Pipe(TimeSpan.FromMilliseconds)
        );

        public IReadOnlyList<ClosedCaptionPartResponse> GetParts() => _memo.Wrap(() =>
            _root
                .Elements("s")
                .Select(x => new ClosedCaptionPartResponse(x))
                .ToArray()
        );
    }
}
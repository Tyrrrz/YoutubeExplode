using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge.Extractors;

internal class ClosedCaptionExtractor
{
    private readonly XElement _content;
    private readonly Memo _memo = new();

    public ClosedCaptionExtractor(XElement content) => _content = content;

    public string? TryGetText() => _memo.Wrap(() =>
        (string?) _content
    );

    public TimeSpan? TryGetOffset() => _memo.Wrap(() =>
        ((double?) _content.Attribute("t"))?.Pipe(TimeSpan.FromMilliseconds)
    );

    public TimeSpan? TryGetDuration() => _memo.Wrap(() =>
        ((double?) _content.Attribute("d"))?.Pipe(TimeSpan.FromMilliseconds)
    );

    public IReadOnlyList<ClosedCaptionPartExtractor> GetParts() => _memo.Wrap(() =>
        _content
            .Elements("s")
            .Select(x => new ClosedCaptionPartExtractor(x))
            .ToArray()
    );
}
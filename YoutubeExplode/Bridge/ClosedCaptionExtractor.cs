using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal class ClosedCaptionExtractor
{
    private readonly XElement _content;

    public ClosedCaptionExtractor(XElement content) => _content = content;

    public string? TryGetText() => Memo.Cache(this, () =>
        (string?) _content
    );

    public TimeSpan? TryGetOffset() => Memo.Cache(this, () =>
        ((double?) _content.Attribute("t"))?.Pipe(TimeSpan.FromMilliseconds)
    );

    public TimeSpan? TryGetDuration() => Memo.Cache(this, () =>
        ((double?) _content.Attribute("d"))?.Pipe(TimeSpan.FromMilliseconds)
    );

    public IReadOnlyList<ClosedCaptionPartExtractor> GetParts() => Memo.Cache(this, () =>
        _content
            .Elements("s")
            .Select(x => new ClosedCaptionPartExtractor(x))
            .ToArray()
    );
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class ClosedCaptionTrackResponse
{
    private readonly XElement _content;

    public IReadOnlyList<CaptionData> Captions => Memo.Cache(this, () =>
        _content
            .Descendants("p")
            .Select(x => new CaptionData(x))
            .ToArray()
    );

    public ClosedCaptionTrackResponse(XElement content) => _content = content;
}

internal partial class ClosedCaptionTrackResponse
{
    public class CaptionData
    {
        private readonly XElement _content;

        public string? Text => Memo.Cache(this, () =>
            (string?)_content
        );

        public TimeSpan? Offset => Memo.Cache(this, () =>
            ((double?)_content.Attribute("t"))?.Pipe(TimeSpan.FromMilliseconds)
        );

        public TimeSpan? Duration => Memo.Cache(this, () =>
            ((double?)_content.Attribute("d"))?.Pipe(TimeSpan.FromMilliseconds)
        );

        public IReadOnlyList<PartData> Parts => Memo.Cache(this, () =>
            _content
                .Elements("s")
                .Select(x => new PartData(x))
                .ToArray()
        );

        public CaptionData(XElement content) => _content = content;
    }
}

internal partial class ClosedCaptionTrackResponse
{
    public class PartData
    {
        private readonly XElement _content;

        public string? Text => Memo.Cache(this, () =>
            (string?)_content
        );

        public TimeSpan? Offset => Memo.Cache(this, () =>
            ((double?)_content.Attribute("t"))?.Pipe(TimeSpan.FromMilliseconds) ??
            ((double?)_content.Attribute("ac"))?.Pipe(TimeSpan.FromMilliseconds) ??
            TimeSpan.Zero
        );

        public PartData(XElement content) => _content = content;
    }
}

internal partial class ClosedCaptionTrackResponse
{
    public static ClosedCaptionTrackResponse Parse(string raw) => new(Xml.Parse(raw));
}
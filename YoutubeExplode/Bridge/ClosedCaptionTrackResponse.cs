using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Lazy;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class ClosedCaptionTrackResponse
{
    private readonly XElement _content;

    [Lazy]
    public IReadOnlyList<CaptionData> Captions => _content
        .Descendants("p")
        .Select(x => new CaptionData(x))
        .ToArray();

    public ClosedCaptionTrackResponse(XElement content) => _content = content;
}

internal partial class ClosedCaptionTrackResponse
{
    public class CaptionData
    {
        private readonly XElement _content;

        [Lazy]
        public string? Text => (string?)_content;

        [Lazy]
        public TimeSpan? Offset => ((double?)_content.Attribute("t"))?.Pipe(TimeSpan.FromMilliseconds);

        [Lazy]
        public TimeSpan? Duration => ((double?)_content.Attribute("d"))?.Pipe(TimeSpan.FromMilliseconds);

        [Lazy]
        public IReadOnlyList<PartData> Parts => _content
            .Elements("s")
            .Select(x => new PartData(x))
            .ToArray();

        public CaptionData(XElement content) => _content = content;
    }
}

internal partial class ClosedCaptionTrackResponse
{
    public class PartData
    {
        private readonly XElement _content;

        [Lazy]
        public string? Text => (string?)_content;

        [Lazy]
        public TimeSpan? Offset =>
            ((double?)_content.Attribute("t"))?.Pipe(TimeSpan.FromMilliseconds) ??
            ((double?)_content.Attribute("ac"))?.Pipe(TimeSpan.FromMilliseconds) ??
            TimeSpan.Zero;

        public PartData(XElement content) => _content = content;
    }
}

internal partial class ClosedCaptionTrackResponse
{
    public static ClosedCaptionTrackResponse Parse(string raw) => new(Xml.Parse(raw));
}
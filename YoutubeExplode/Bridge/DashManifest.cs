using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class DashManifest
{
    private readonly XElement _content;

    public IReadOnlyList<IStreamData> Streams => Memo.Cache(this, () =>
        _content
            .Descendants("Representation")
            // Skip non-media representations (like "rawcc")
            // https://github.com/Tyrrrz/YoutubeExplode/issues/546
            .Where(x => x
                .Attribute("id")?
                .Value
                .All(char.IsDigit) == true
            )
            // Skip segmented streams
            // https://github.com/Tyrrrz/YoutubeExplode/issues/159
            .Where(x => x
                .Descendants("Initialization")
                .FirstOrDefault()?
                .Attribute("sourceURL")?
                .Value
                .Contains("sq/") != true
            )
            // Skip streams without codecs
            .Where(x => !string.IsNullOrWhiteSpace(x.Attribute("codecs")?.Value))
            .Select(x => new StreamData(x))
            .ToArray()
    );

    public DashManifest(XElement content) => _content = content;
}

internal partial class DashManifest
{
    public class StreamData : IStreamData
    {
        private readonly XElement _content;

        public int? Itag => Memo.Cache(this, () =>
            (int?)_content.Attribute("id")
        );

        public string? Url => Memo.Cache(this, () =>
            (string?)_content.Element("BaseURL")
        );

        // DASH streams don't have signatures
        public string? Signature => null;

        // DASH streams don't have signatures
        public string? SignatureParameter => null;

        public long? ContentLength => Memo.Cache(this, () =>
            (long?)_content.Attribute("contentLength") ??

            Url?
                .Pipe(s => Regex.Match(s, @"[/\?]clen[/=](\d+)").Groups[1].Value)
                .NullIfWhiteSpace()?
                .ParseLongOrNull()
        );

        public long? Bitrate => Memo.Cache(this, () =>
            (long?)_content.Attribute("bandwidth")
        );

        public string? Container => Memo.Cache(this, () =>
            Url?
                .Pipe(s => Regex.Match(s, @"mime[/=]\w*%2F([\w\d]*)").Groups[1].Value)
                .Pipe(WebUtility.UrlDecode)
        );

        private bool IsAudioOnly => Memo.Cache(this, () =>
            _content.Element("AudioChannelConfiguration") is not null
        );

        public string? AudioCodec => Memo.Cache(this, () =>
            IsAudioOnly
                ? (string?)_content.Attribute("codecs")
                : null
        );

        public string? VideoCodec => Memo.Cache(this, () =>
            IsAudioOnly
                ? null
                : (string?)_content.Attribute("codecs")
        );

        public string? VideoQualityLabel => null;

        public int? VideoWidth => Memo.Cache(this, () =>
            (int?)_content.Attribute("width")
        );

        public int? VideoHeight => Memo.Cache(this, () =>
            (int?)_content.Attribute("height")
        );

        public int? VideoFramerate => Memo.Cache(this, () =>
            (int?)_content.Attribute("frameRate")
        );

        public StreamData(XElement content) => _content = content;
    }
}

internal partial class DashManifest
{
    public static DashManifest Parse(string raw) => new(Xml.Parse(raw));
}
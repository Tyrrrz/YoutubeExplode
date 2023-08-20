using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Lazy;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class DashManifest
{
    private readonly XElement _content;

    [Lazy]
    public IReadOnlyList<IStreamData> Streams => _content
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
        .ToArray();

    public DashManifest(XElement content) => _content = content;
}

internal partial class DashManifest
{
    public class StreamData : IStreamData
    {
        private readonly XElement _content;

        [Lazy]
        public int? Itag => (int?)_content.Attribute("id");

        [Lazy]
        public string? Url => (string?)_content.Element("BaseURL");

        // DASH streams don't have signatures
        public string? Signature => null;

        // DASH streams don't have signatures
        public string? SignatureParameter => null;

        [Lazy]
        public long? ContentLength =>
            (long?)_content.Attribute("contentLength") ??

            Url?
                .Pipe(s => Regex.Match(s, @"[/\?]clen[/=](\d+)").Groups[1].Value)
                .NullIfWhiteSpace()?
                .ParseLongOrNull();

        [Lazy]
        public long? Bitrate => (long?)_content.Attribute("bandwidth");

        [Lazy]
        public string? Container => Url?
            .Pipe(s => Regex.Match(s, @"mime[/=]\w*%2F([\w\d]*)").Groups[1].Value)
            .Pipe(WebUtility.UrlDecode);

        [Lazy]
        private bool IsAudioOnly => _content.Element("AudioChannelConfiguration") is not null;

        [Lazy]
        public string? AudioCodec => IsAudioOnly
            ? (string?)_content.Attribute("codecs")
            : null;

        [Lazy]
        public string? VideoCodec => IsAudioOnly
            ? null
            : (string?)_content.Attribute("codecs");

        public string? VideoQualityLabel => null;

        [Lazy]
        public int? VideoWidth => (int?)_content.Attribute("width");

        [Lazy]
        public int? VideoHeight => (int?)_content.Attribute("height");

        [Lazy]
        public int? VideoFramerate => (int?)_content.Attribute("frameRate");

        public StreamData(XElement content) => _content = content;
    }
}

internal partial class DashManifest
{
    public static DashManifest Parse(string raw) => new(Xml.Parse(raw));
}
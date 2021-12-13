using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal class DashStreamInfoExtractor : IStreamInfoExtractor
{
    private readonly XElement _content;

    public DashStreamInfoExtractor(XElement content) => _content = content;

    public int? TryGetItag() => Memo.Cache(this, () =>
        (int?) _content.Attribute("id")
    );

    public string? TryGetUrl() => Memo.Cache(this, () =>
        (string?) _content.Element("BaseURL")
    );

    // DASH streams don't have signatures
    public string? TryGetSignature() => null;

    // DASH streams don't have signatures
    public string? TryGetSignatureParameter() => null;

    public long? TryGetContentLength() => Memo.Cache(this, () =>
        (long?) _content.Attribute("contentLength") ??

        TryGetUrl()?
            .Pipe(s => Regex.Match(s, @"[/\?]clen[/=](\d+)").Groups[1].Value)
            .NullIfWhiteSpace()?
            .ParseLongOrNull()
    );

    public long? TryGetBitrate() => Memo.Cache(this, () =>
        (long?) _content.Attribute("bandwidth")
    );

    public string? TryGetContainer() => Memo.Cache(this, () =>
        TryGetUrl()?
            .Pipe(s => Regex.Match(s, @"mime[/=]\w*%2F([\w\d]*)").Groups[1].Value)
            .Pipe(WebUtility.UrlDecode)
    );

    private bool IsAudioOnly() => Memo.Cache(this, () =>
        _content.Element("AudioChannelConfiguration") is not null
    );

    public string? TryGetAudioCodec() => Memo.Cache(this, () =>
        IsAudioOnly()
            ? (string?) _content.Attribute("codecs")
            : null
    );

    public string? TryGetVideoCodec() => Memo.Cache(this, () =>
        IsAudioOnly()
            ? null
            : (string?) _content.Attribute("codecs")
    );

    public string? TryGetVideoQualityLabel() => null;

    public int? TryGetVideoWidth() => Memo.Cache(this, () =>
        (int?) _content.Attribute("width")
    );

    public int? TryGetVideoHeight() => Memo.Cache(this, () =>
        (int?) _content.Attribute("height")
    );

    public int? TryGetFramerate() => Memo.Cache(this, () =>
        (int?) _content.Attribute("frameRate")
    );
}
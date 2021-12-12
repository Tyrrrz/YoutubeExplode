using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal class DashStreamInfoExtractor : IStreamInfoExtractor
{
    private readonly XElement _content;
    private readonly Memo _memo = new();

    public DashStreamInfoExtractor(XElement content) => _content = content;

    public int? TryGetItag() => _memo.Wrap(() =>
        (int?) _content.Attribute("id")
    );

    public string? TryGetUrl() => _memo.Wrap(() =>
        (string?) _content.Element("BaseURL")
    );

    // DASH streams don't have signatures
    public string? TryGetSignature() => null;

    // DASH streams don't have signatures
    public string? TryGetSignatureParameter() => null;

    public long? TryGetContentLength() => _memo.Wrap(() =>
        (long?) _content.Attribute("contentLength") ??

        TryGetUrl()?
            .Pipe(s => Regex.Match(s, @"[/\?]clen[/=](\d+)").Groups[1].Value)
            .NullIfWhiteSpace()?
            .ParseLongOrNull()
    );

    public long? TryGetBitrate() => _memo.Wrap(() =>
        (long?) _content.Attribute("bandwidth")
    );

    public string? TryGetContainer() => _memo.Wrap(() =>
        TryGetUrl()?
            .Pipe(s => Regex.Match(s, @"mime[/=]\w*%2F([\w\d]*)").Groups[1].Value)
            .Pipe(WebUtility.UrlDecode)
    );

    private bool IsAudioOnly() => _memo.Wrap(() =>
        _content.Element("AudioChannelConfiguration") is not null
    );

    public string? TryGetAudioCodec() => _memo.Wrap(() =>
        IsAudioOnly()
            ? (string?) _content.Attribute("codecs")
            : null
    );

    public string? TryGetVideoCodec() => _memo.Wrap(() =>
        IsAudioOnly()
            ? null
            : (string?) _content.Attribute("codecs")
    );

    public string? TryGetVideoQualityLabel() => null;

    public int? TryGetVideoWidth() => _memo.Wrap(() =>
        (int?) _content.Attribute("width")
    );

    public int? TryGetVideoHeight() => _memo.Wrap(() =>
        (int?) _content.Attribute("height")
    );

    public int? TryGetFramerate() => _memo.Wrap(() =>
        (int?) _content.Attribute("frameRate")
    );
}
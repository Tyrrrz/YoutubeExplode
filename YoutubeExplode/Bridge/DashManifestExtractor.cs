using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Bridge;

internal partial class DashManifestExtractor
{
    private readonly XElement _content;

    public DashManifestExtractor(XElement content) => _content = content;

    public IReadOnlyList<IStreamInfoExtractor> GetStreams() => Memo.Cache(this, () =>
        _content
            .Descendants("Representation")
            // Skip non-media representations (like "rawcc")
            // https://github.com/Tyrrrz/YoutubeExplode/issues/546
            .Where(x => x
                .Attribute("id")?
                .Value
                .All(char.IsDigit) == true)
            // Skip segmented streams
            // https://github.com/Tyrrrz/YoutubeExplode/issues/159
            .Where(x => x
                .Descendants("Initialization")
                .FirstOrDefault()?
                .Attribute("sourceURL")?
                .Value
                .Contains("sq/") != true)
            // Skip streams without codecs
            .Where(x => !string.IsNullOrWhiteSpace(x.Attribute("codecs")?.Value))
            .Select(x => new DashStreamInfoExtractor(x))
            .ToArray()
    );
}

internal partial class DashManifestExtractor
{
    public static DashManifestExtractor Create(string raw) => new(Xml.Parse(raw));
}
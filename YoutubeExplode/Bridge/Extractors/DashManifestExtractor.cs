using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Bridge.Extractors
{
    internal partial class DashManifestExtractor
    {
        private readonly XElement _content;
        private readonly Memo _memo = new();

        public DashManifestExtractor(XElement content) => _content = content;

        public IReadOnlyList<IStreamInfoExtractor> GetStreams() => _memo.Wrap(() =>
            _content
                .Descendants("Representation")
                .Where(x => x
                    .Descendants("Initialization")
                    .FirstOrDefault()?
                    .Attribute("sourceURL")?
                    .Value
                    .Contains("sq/") != true)
                .Where(x => !string.IsNullOrWhiteSpace(x.Attribute("codecs")?.Value))
                .Select(x => new DashStreamInfoExtractor(x))
                .ToArray()
        );
    }

    internal partial class DashManifestExtractor
    {
        public static DashManifestExtractor Create(string raw) => new(Xml.Parse(raw));
    }
}
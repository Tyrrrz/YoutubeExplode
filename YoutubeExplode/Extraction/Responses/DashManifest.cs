using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Extraction.Responses
{
    internal class DashManifest
    {
        private readonly XElement _root;
        private readonly Memo _memo = new();

        public DashManifest(XElement root) => _root = root;

        public IReadOnlyList<IStreamInfoResponse> GetStreams() => _memo.Wrap(() =>
            _root
                .Descendants("Representation")
                .Where(x => x
                    .Descendants("Initialization")
                    .FirstOrDefault()?
                    .Attribute("sourceURL")?
                    .Value
                    .Contains("sq/") != true)
                .Where(x => !string.IsNullOrWhiteSpace(x.Attribute("codecs")?.Value))
                .Select(x => new DashStreamInfoResponse(x))
                .ToArray()
        );
    }
}
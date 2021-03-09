using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Extensions;

namespace YoutubeExplode.ReverseEngineering.Responses
{
    internal partial class DashManifest
    {
        private readonly XElement _root;

        public DashManifest(XElement root) => _root = root;

        public IEnumerable<StreamInfo> GetStreams() => _root
            .Descendants("Representation")
            .Where(x => x
                .Descendants("Initialization")
                .FirstOrDefault()?
                .Attribute("sourceURL")?
                .Value
                .Contains("sq/") != true)
            .Where(x => !string.IsNullOrWhiteSpace(x.Attribute("codecs")?.Value))
            .Select(x => new StreamInfo(x));
    }

    internal partial class DashManifest
    {
        public class StreamInfo : IStreamInfoProvider
        {
            private readonly XElement _root;

            public StreamInfo(XElement root) => _root = root;

            public int GetTag() => (int) _root.Attribute("id");

            public string GetUrl() => (string) _root.Element("BaseURL");

            // DASH streams don't have signatures
            public string? TryGetSignature() => null;

            // DASH streams don't have signatures
            public string? TryGetSignatureParameter() => null;

            public long? TryGetContentLength() =>
                (long?) _root.Attribute("contentLength") ??
                GetUrl().Pipe(s => Regex.Match(s, @"[/\?]clen[/=](\d+)").Groups[1].Value).NullIfWhiteSpace()?.ParseLong();

            public long GetBitrate() => (long) _root.Attribute("bandwidth");

            public string GetContainer() => GetUrl()
                .Pipe(s => Regex.Match(s, @"mime[/=]\w*%2F([\w\d]*)").Groups[1].Value)
                .Pipe(WebUtility.UrlDecode)!;

            private bool IsAudioOnly() => _root
                .Element("AudioChannelConfiguration") is not null;

            public string? TryGetAudioCodec() =>
                IsAudioOnly()
                    ? (string) _root.Attribute("codecs")
                    : null;

            public string? TryGetVideoCodec() =>
                IsAudioOnly()
                    ? null
                    : (string) _root.Attribute("codecs");

            public string? TryGetVideoQualityLabel() => null;

            public int? TryGetVideoWidth() => (int?) _root.Attribute("width");

            public int? TryGetVideoHeight() => (int?) _root.Attribute("height");

            public int? TryGetFramerate() => (int?) _root.Attribute("frameRate");
        }
    }

    internal partial class DashManifest
    {
        public static DashManifest Parse(string raw) => new(Xml.Parse(raw));

        public static async Task<DashManifest> GetAsync(YoutubeHttpClient httpClient, string url) =>
            await Retry.WrapAsync(async () =>
            {
                var raw = await httpClient.GetStringAsync(url);
                return Parse(raw);
            });

        public static string? TryGetSignatureFromUrl(string url) =>
            Regex.Match(url, "/s/(.*?)(?:/|$)").Groups[1].Value.NullIfWhiteSpace();
    }
}
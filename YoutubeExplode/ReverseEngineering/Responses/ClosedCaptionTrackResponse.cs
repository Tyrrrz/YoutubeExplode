using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using YoutubeExplode.Internal;

namespace YoutubeExplode.ReverseEngineering.Responses
{
    internal partial class ClosedCaptionTrackResponse
    {
        private readonly XElement _root;

        public ClosedCaptionTrackResponse(XElement root)
        {
            _root = root;
        }

        public IEnumerable<ClosedCaption> GetClosedCaptions() => _root
            .Descendants("p")
            .Select(x => new ClosedCaption(x));
    }

    internal partial class ClosedCaptionTrackResponse
    {
        public class ClosedCaption
        {
            private readonly XElement _root;

            public ClosedCaption(XElement root)
            {
                _root = root;
            }

            public string GetText() => (string) _root;

            public TimeSpan GetOffset() => TimeSpan.FromMilliseconds((double) _root.Attribute("t"));

            public TimeSpan GetDuration() => TimeSpan.FromMilliseconds((double) _root.Attribute("d"));
        }
    }

    internal partial class ClosedCaptionTrackResponse
    {
        public static ClosedCaptionTrackResponse Parse(string raw) => new ClosedCaptionTrackResponse(Xml.Parse(raw));

        public static async Task<ClosedCaptionTrackResponse> GetAsync(HttpClient httpClient, string url) =>
            await Retry.WrapAsync(async () =>
            {
                // Enforce known format
                var urlWithFormat = Url.SetQueryParameter(url, "format", "3");

                var raw = await httpClient.GetStringAsync(urlWithFormat);

                return Parse(raw);
            });
    }
}
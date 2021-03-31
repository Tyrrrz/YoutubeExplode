using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Videos.ClosedCaptions.Resolving
{
    internal class ClosedCaptionTrackResolver
    {
        private readonly HttpClient _httpClient;
        private readonly string _url;
        private readonly Cache _cache = new();

        public ClosedCaptionTrackResolver(HttpClient httpClient, string url)
        {
            _httpClient = httpClient;
            _url = url;
        }

        private ValueTask<XElement> GetTrackDataAsync() => _cache.WrapAsync(async () =>
        {
            // Enforce known format
            var urlWithFormat = Url.SetQueryParameter(_url, "format", "3");

            var raw = await _httpClient.GetStringAsync(urlWithFormat);

            return Xml.Parse(raw);
        });

        public ValueTask<IReadOnlyList<ClosedCaption>> GetClosedCaptionsAsync() => _cache.WrapAsync(async () =>
        {
            var trackData = await GetTrackDataAsync();

            var result = new List<ClosedCaption>();

            foreach (var captionXml in trackData.Descendants("p"))
            {
                var text = (string) captionXml;

                var offset = TimeSpan.FromMilliseconds(
                    (double?) captionXml.Attribute("t") ?? 0
                );

                var duration = TimeSpan.FromMilliseconds(
                    (double?) captionXml.Attribute("d") ?? 0
                );

                var parts = new List<ClosedCaptionPart>();

                foreach (var partXml in captionXml.Elements("s"))
                {
                    var partText = (string) partXml;

                    var partOffset = TimeSpan.FromMilliseconds(
                        (double?) partXml.Attribute("t") ?? 0
                    );

                    var part = new ClosedCaptionPart(partText, partOffset);

                    parts.Add(part);
                }

                var caption = new ClosedCaption(
                    text,
                    offset,
                    duration,
                    parts
                );

                result.Add(caption);
            }

            return (IReadOnlyList<ClosedCaption>) result;
        });
    }
}
using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class SectionListRenderer
    {
        [JsonProperty("contents")]
        public List<SectionListRendererContent> Contents { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("subMenu")]
        public SubMenu SubMenu { get; set; }
    }
}
using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class MultiPageMenuSectionRenderer
    {
        [JsonProperty("items")]
        public List<MultiPageMenuSectionRendererItem> Items { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }
    }
}
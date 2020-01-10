using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ItemSectionRenderer
    {
        [JsonProperty("contents")]
        public List<ItemSectionRendererContent> Contents { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }
    }
}
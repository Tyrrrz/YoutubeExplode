using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class GridRenderer
    {
        [JsonProperty("items")]
        public List<GridRendererItem> Items { get; set; }

        [JsonProperty("continuations")]
        public List<Continuation> Continuations { get; set; }

        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }
    }
}
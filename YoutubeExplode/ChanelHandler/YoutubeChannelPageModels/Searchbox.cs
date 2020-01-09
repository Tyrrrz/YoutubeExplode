using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Searchbox
    {
        [JsonProperty("fusionSearchboxRenderer")]
        public FusionSearchboxRenderer FusionSearchboxRenderer { get; set; }
    }
}
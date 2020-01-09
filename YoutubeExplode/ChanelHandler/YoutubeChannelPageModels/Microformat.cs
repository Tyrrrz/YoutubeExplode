using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Microformat
    {
        [JsonProperty("microformatDataRenderer")]
        public MicroformatDataRenderer MicroformatDataRenderer { get; set; }
    }
}
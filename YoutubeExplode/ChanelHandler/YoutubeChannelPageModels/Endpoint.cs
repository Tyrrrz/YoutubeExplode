using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Endpoint
    {
        [JsonProperty("params")]
        public string Params { get; set; }
    }
}
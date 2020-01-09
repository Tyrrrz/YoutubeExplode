using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class PurpleUrlEndpoint
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("nofollow")]
        public bool Nofollow { get; set; }
    }
}
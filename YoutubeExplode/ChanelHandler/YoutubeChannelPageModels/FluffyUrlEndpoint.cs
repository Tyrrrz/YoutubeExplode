using System;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class FluffyUrlEndpoint
    {
        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }
    }
}
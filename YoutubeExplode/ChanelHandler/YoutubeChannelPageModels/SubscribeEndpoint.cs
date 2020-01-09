using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class SubscribeEndpoint
    {
        [JsonProperty("channelIds")]
        public List<string> ChannelIds { get; set; }

        [JsonProperty("params")]
        public string Params { get; set; }
    }
}
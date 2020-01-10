using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ServiceTrackingParam
    {
        [JsonProperty("service")]
        public string Service { get; set; }

        [JsonProperty("params")]
        public List<Param> Params { get; set; }
    }
}
using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class SubscriberCountText
    {
        [JsonProperty("runs")]
        public List<PurpleRun> Runs { get; set; }
    }
}
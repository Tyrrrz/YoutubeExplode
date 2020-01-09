using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ThumbnailText
    {
        [JsonProperty("runs")]
        public List<ThumbnailTextRun> Runs { get; set; }
    }
}
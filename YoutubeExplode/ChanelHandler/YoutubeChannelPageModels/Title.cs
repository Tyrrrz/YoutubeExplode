using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Title
    {
        [JsonProperty("runs")]
        public List<TitleRun> Runs { get; set; }
    }
}
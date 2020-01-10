using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ViewPlaylistText
    {
        [JsonProperty("runs")]
        public List<ViewPlaylistTextRun> Runs { get; set; }
    }
}
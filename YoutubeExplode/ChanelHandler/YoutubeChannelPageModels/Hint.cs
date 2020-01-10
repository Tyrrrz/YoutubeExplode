using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Hint
    {
        [JsonProperty("hintRenderer")]
        public HintRenderer HintRenderer { get; set; }
    }
}
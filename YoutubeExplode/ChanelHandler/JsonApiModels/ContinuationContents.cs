using Newtonsoft.Json;
using YoutubeExplode.ChanelHandler.ChannelPageModels;

namespace YoutubeExplode.ChanelHandler.JsonApiModels
{
    internal class ContinuationContents
    {
        [JsonProperty("gridContinuation")]
        public GridRenderer GridContinuation { get; set; }
    }
}
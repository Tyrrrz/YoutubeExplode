using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Continuation
    {
        [JsonProperty("nextContinuationData")]
        public NextContinuationData NextContinuationData { get; set; }
    }
}
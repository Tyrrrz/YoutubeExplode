using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class FrameworkUpdates
    {
        [JsonProperty("entityBatchUpdate")]
        public EntityBatchUpdate EntityBatchUpdate { get; set; }
    }
}
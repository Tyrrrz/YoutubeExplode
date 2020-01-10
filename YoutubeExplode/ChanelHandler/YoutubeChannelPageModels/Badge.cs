using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Badge
    {
        [JsonProperty("metadataBadgeRenderer")]
        public MetadataBadgeRenderer MetadataBadgeRenderer { get; set; }
    }
}
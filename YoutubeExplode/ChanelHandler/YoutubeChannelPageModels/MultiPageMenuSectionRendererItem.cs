using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class MultiPageMenuSectionRendererItem
    {
        [JsonProperty("compactLinkRenderer")]
        public CompactLinkRenderer CompactLinkRenderer { get; set; }
    }
}
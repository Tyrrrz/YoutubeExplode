using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class MultiPageMenuRendererSection
    {
        [JsonProperty("multiPageMenuSectionRenderer")]
        public MultiPageMenuSectionRenderer MultiPageMenuSectionRenderer { get; set; }
    }
}
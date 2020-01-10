using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class IconThumbnail
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
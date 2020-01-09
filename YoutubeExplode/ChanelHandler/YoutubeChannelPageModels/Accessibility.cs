using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Accessibility
    {
        [JsonProperty("label")]
        public string Label { get; set; }
    }
}
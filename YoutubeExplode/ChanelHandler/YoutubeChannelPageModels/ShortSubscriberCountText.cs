using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ShortSubscriberCountText
    {
        [JsonProperty("simpleText", NullValueHandling = NullValueHandling.Ignore)]
        public string SimpleText { get; set; }
    }
}
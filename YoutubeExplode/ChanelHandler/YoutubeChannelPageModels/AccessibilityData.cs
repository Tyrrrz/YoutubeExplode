using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class AccessibilityData
    {
        [JsonProperty("accessibilityData")]
        public Accessibility AccessibilityDataAccessibilityData { get; set; }
    }
}
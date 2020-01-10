using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class HotkeyDialogSectionOptionRenderer
    {
        [JsonProperty("label")]
        public SubscriberCountText Label { get; set; }

        [JsonProperty("hotkey")]
        public string Hotkey { get; set; }

        [JsonProperty("hotkeyAccessibilityLabel", NullValueHandling = NullValueHandling.Ignore)]
        public AccessibilityData HotkeyAccessibilityLabel { get; set; }
    }
}
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Option
    {
        [JsonProperty("hotkeyDialogSectionOptionRenderer")]
        public HotkeyDialogSectionOptionRenderer HotkeyDialogSectionOptionRenderer { get; set; }
    }
}
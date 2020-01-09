using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class HotkeyDialogRendererSection
    {
        [JsonProperty("hotkeyDialogSectionRenderer")]
        public HotkeyDialogSectionRenderer HotkeyDialogSectionRenderer { get; set; }
    }
}
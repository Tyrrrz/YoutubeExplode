using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class HotkeyDialog
    {
        [JsonProperty("hotkeyDialogRenderer")]
        public HotkeyDialogRenderer HotkeyDialogRenderer { get; set; }
    }
}
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class FluffyPopup
    {
        [JsonProperty("confirmDialogRenderer")]
        public ConfirmDialogRenderer ConfirmDialogRenderer { get; set; }
    }
}
using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ConfirmDialogRenderer
    {
        [JsonProperty("trackingParams")]
        public string TrackingParams { get; set; }

        [JsonProperty("dialogMessages")]
        public List<SubscriberCountText> DialogMessages { get; set; }

        [JsonProperty("confirmButton")]
        public CancelButtonClass ConfirmButton { get; set; }

        [JsonProperty("cancelButton")]
        public CancelButtonClass CancelButton { get; set; }

        [JsonProperty("primaryIsCancel")]
        public bool PrimaryIsCancel { get; set; }
    }
}
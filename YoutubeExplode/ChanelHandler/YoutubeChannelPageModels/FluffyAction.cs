using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class FluffyAction
    {
        [JsonProperty("signalAction")]
        public Signal SignalAction { get; set; }
    }
}
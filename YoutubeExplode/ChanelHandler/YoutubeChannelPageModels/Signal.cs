using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Signal
    {
        [JsonProperty("signal")]
        public string SignalSignal { get; set; }
    }
}
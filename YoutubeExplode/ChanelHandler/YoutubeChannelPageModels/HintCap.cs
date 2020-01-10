using Newtonsoft.Json;
using YoutubeExplode.ChanelHandler.Converters;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class HintCap
    {
        [JsonProperty("impressionCap")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long ImpressionCap { get; set; }
    }
}
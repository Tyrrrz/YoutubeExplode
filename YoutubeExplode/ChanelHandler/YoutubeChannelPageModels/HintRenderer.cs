using Newtonsoft.Json;
using YoutubeExplode.ChanelHandler.Converters;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class HintRenderer
    {
        [JsonProperty("hintId")]
        public string HintId { get; set; }

        [JsonProperty("dwellTimeMs")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long DwellTimeMs { get; set; }

        [JsonProperty("hintCap")]
        public HintCap HintCap { get; set; }
    }
}
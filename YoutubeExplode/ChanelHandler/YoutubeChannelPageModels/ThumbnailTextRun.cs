using Newtonsoft.Json;
using YoutubeExplode.ChanelHandler.Converters;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ThumbnailTextRun
    {
        [JsonProperty("text")]
        public TextUnion Text { get; set; }

        [JsonProperty("bold", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Bold { get; set; }
    }
}
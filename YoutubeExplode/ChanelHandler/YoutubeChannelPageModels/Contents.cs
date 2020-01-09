using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class Contents
    {
        [JsonProperty("twoColumnBrowseResultsRenderer")]
        public TwoColumnBrowseResultsRenderer TwoColumnBrowseResultsRenderer { get; set; }
    }
}
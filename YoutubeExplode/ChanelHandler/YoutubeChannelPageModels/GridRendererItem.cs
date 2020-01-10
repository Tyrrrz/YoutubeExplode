using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class GridRendererItem
    {
        [JsonProperty("gridPlaylistRenderer")]
        public GridPlaylistRenderer GridPlaylistRenderer { get; set; }
    }
}
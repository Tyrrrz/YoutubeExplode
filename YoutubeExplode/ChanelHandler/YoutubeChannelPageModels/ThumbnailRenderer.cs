using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class ThumbnailRenderer
    {
        [JsonProperty("playlistVideoThumbnailRenderer")]
        public PlaylistVideoThumbnailRenderer PlaylistVideoThumbnailRenderer { get; set; }
    }
}
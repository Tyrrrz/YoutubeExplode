using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class WebResponseContextPreloadData
    {
        [JsonProperty("preloadThumbnailUrls")]
        public List<string> PreloadThumbnailUrls { get; set; }
    }
}
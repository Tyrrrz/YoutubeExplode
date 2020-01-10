using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.JsonApiModels
{
    internal class WebResponseContextPreloadData
    {
        [JsonProperty("preloadThumbnailUrls")]
        public List<Uri> PreloadThumbnailUrls { get; set; }
    }
}
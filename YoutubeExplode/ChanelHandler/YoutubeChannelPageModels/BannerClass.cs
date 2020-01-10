using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class BannerClass
    {
        [JsonProperty("thumbnails")]
        public List<AvatarThumbnail> Thumbnails { get; set; }

        [JsonProperty("webThumbnailDetailsExtensionData", NullValueHandling = NullValueHandling.Ignore)]
        public BannerWebThumbnailDetailsExtensionData WebThumbnailDetailsExtensionData { get; set; }
    }
}
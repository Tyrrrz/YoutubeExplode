using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class TopbarMenuButtonRendererAvatar
    {
        [JsonProperty("thumbnails")]
        public List<AvatarThumbnail> Thumbnails { get; set; }

        [JsonProperty("accessibility")]
        public AccessibilityData Accessibility { get; set; }

        [JsonProperty("webThumbnailDetailsExtensionData")]
        public PurpleWebThumbnailDetailsExtensionData WebThumbnailDetailsExtensionData { get; set; }
    }
}
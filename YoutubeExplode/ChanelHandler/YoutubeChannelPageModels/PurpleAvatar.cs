using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class PurpleAvatar
    {
        [JsonProperty("thumbnails")]
        public List<AvatarThumbnail> Thumbnails { get; set; }
    }
}
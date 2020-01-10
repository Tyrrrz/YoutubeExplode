using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class PrimaryLinkIcon
    {
        [JsonProperty("thumbnails")]
        public List<IconThumbnail> Thumbnails { get; set; }
    }
}
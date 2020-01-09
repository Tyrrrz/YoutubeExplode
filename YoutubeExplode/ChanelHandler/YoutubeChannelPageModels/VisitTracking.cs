using System;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class VisitTracking
    {
        [JsonProperty("remarketingPing")]
        public Uri RemarketingPing { get; set; }
    }
}
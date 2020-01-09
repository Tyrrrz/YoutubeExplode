using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class EntityBatchUpdate
    {
        [JsonProperty("mutations")]
        public List<Mutation> Mutations { get; set; }
    }
}
using System.Collections.Generic;
using Newtonsoft.Json;

namespace YoutubeExplode.ChanelHandler.ChannelPageModels
{
    internal class CommandExecutorCommand
    {
        [JsonProperty("commands")]
        public List<CommandElement> Commands { get; set; }
    }
}
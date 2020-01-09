using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeExplode.ChanelHandler.Exceptions
{
    /// <summary>
    /// Thrown when the call to youtube json api fails
    /// </summary>
    public class YoutubeApiCallException : Exception
    {
        /// <summary>
        /// ChannelId or ChannelName which failed
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelId"></param>
        public YoutubeApiCallException(string channelId)
        {
            ChannelId = channelId;
        }
    }
}

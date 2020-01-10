using System;
using System.Collections.Generic;
using System.Text;
using YoutubeExplode.ChanelHandler.ChannelPageModels;

namespace YoutubeExplode.ChanelHandler.Exceptions
{
    /// <summary>
    /// Thrown when the youtube api version couldn't be extracted from the embedded json.
    /// </summary>
    public class YoutubeApiVersionException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public YoutubeApiVersionException() { }
    }
}

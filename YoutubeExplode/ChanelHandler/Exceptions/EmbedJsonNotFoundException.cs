using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeExplode.ChanelHandler.Exceptions
{
    /// <summary>
    /// Thrown when the embedded json from the channel page is not found.
    /// </summary>
    public class EmbedJsonNotFoundException : Exception
    {
        /// <summary>
        /// Channel that was attempted to look up
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel">Channel that was attempted to load</param>
        public EmbedJsonNotFoundException(string channel)
        {
            Channel = channel;
        }
    }
}

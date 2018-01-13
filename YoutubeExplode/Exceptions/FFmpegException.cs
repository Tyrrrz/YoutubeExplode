using System;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// /// Thrown when there was an error muxing files.
    /// </summary>
    public class FFmpegException : Exception
    {
        /// <summary />
        public FFmpegException(string message)
            : base(message)
        {

        }
    }
}

using System;

namespace YoutubeExplode.Exceptions
{
    public abstract class YoutubeExplodeException : Exception
    {
        public YoutubeExplodeException(string message)
            : base(message)
        {
        }
    }
}
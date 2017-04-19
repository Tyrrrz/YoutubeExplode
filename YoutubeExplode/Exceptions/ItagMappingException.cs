using System;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Thrown when a mapping between an itag and stream metadata could not be resolved
    /// </summary>
    public class ItagMappingException : Exception
    {
        /// <summary>
        /// Itag
        /// </summary>
        public int Itag { get; }

        internal ItagMappingException(int itag)
            : base($"Could not resolve mapping for itag [{itag}]")
        {
            Itag = itag;
        }
    }
}

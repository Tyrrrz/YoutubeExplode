using System;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Model extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Get file extension based on container type
        /// </summary>
        public static string GetFileExtension(this Container container)
        {
            switch (container)
            {
                case Container.Mp4:
                    return "mp4";
                case Container.M4A:
                    return "m4a";
                case Container.WebM:
                    return "webm";
                case Container.Tgpp:
                    return "3gpp";
                case Container.Flv:
                    return "flv";
                case Container.Ts:
                    return "ts";
                default:
                    throw new ArgumentOutOfRangeException(nameof(container), "Unknown container type");
            }
        }
    }
}
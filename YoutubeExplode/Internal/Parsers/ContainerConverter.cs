using System;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Internal.Parsers
{
    internal static class ContainerConverter
    {
        public static Container ContainerFromString(string str)
        {
            if (str.Equals("mp4", StringComparison.OrdinalIgnoreCase))
                return Container.Mp4;

            if (str.Equals("webm", StringComparison.OrdinalIgnoreCase))
                return Container.WebM;

            if (str.Equals("3gpp", StringComparison.OrdinalIgnoreCase))
                return Container.Tgpp;

            // Unknown
            throw new ArgumentOutOfRangeException(nameof(str), $"Unknown container [{str}].");
        }
    }
}
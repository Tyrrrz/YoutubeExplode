using System;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Internal.Helpers
{
    internal static class ContainerHelper
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

        public static string ContainerToFileExtension(Container container)
        {
            // Tgpp gets special treatment
            if (container == Container.Tgpp)
                return "3gpp";

            // Convert to lower case string
            return container.ToString().ToLowerInvariant();
        }
    }
}
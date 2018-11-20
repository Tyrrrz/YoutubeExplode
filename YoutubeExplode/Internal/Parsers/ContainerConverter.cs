using System;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Internal.Parsers
{
    internal static class ContainerConverter
    {
        public static Container ContainerFromMimeType(string mimeType)
        {
            // Unknown
            throw new ArgumentOutOfRangeException(nameof(mimeType), $"Unknown mime type [{mimeType}].");
        }
    }
}
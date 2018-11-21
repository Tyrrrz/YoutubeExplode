using System;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Internal.Helpers
{
    internal static class AudioEncodingHelper
    {
        public static AudioEncoding AudioEncodingFromString(string str)
        {
            if (str.StartsWith("mp4a", StringComparison.OrdinalIgnoreCase))
                return AudioEncoding.Aac;

            if (str.StartsWith("vorbis", StringComparison.OrdinalIgnoreCase))
                return AudioEncoding.Vorbis;

            if (str.StartsWith("opus", StringComparison.OrdinalIgnoreCase))
                return AudioEncoding.Opus;

            // Unknown
            throw new ArgumentOutOfRangeException(nameof(str), $"Unknown encoding [{str}].");
        }
    }
}
using System;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Internal.Parsers
{
    internal static class AudioEncodingConverter
    {
        public static AudioEncoding AudioEncodingFromCodec(string codec)
        {
            if (codec.StartsWith("mp4a.", StringComparison.OrdinalIgnoreCase))
                return AudioEncoding.Aac;

            if (codec.Equals("vorbis", StringComparison.OrdinalIgnoreCase))
                return AudioEncoding.Vorbis;

            if (codec.Equals("opus", StringComparison.OrdinalIgnoreCase))
                return AudioEncoding.Opus;

            // Unknown
            throw new ArgumentOutOfRangeException(nameof(codec), $"Unknown codec [{codec}].");
        }
    }
}
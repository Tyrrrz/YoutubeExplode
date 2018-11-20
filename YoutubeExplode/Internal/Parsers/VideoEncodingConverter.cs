using System;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Internal.Parsers
{
    internal static class VideoEncodingConverter
    {
        public static VideoEncoding VideoEncodingFromCodec(string codec)
        {
            if (codec.StartsWith("mp4v.", StringComparison.OrdinalIgnoreCase))
                return VideoEncoding.Mp4V;

            if (codec.StartsWith("avc1.", StringComparison.OrdinalIgnoreCase))
                return VideoEncoding.H264;

            if (codec.Equals("vp8", StringComparison.OrdinalIgnoreCase))
                return VideoEncoding.Vp8;

            if (codec.Equals("vp9", StringComparison.OrdinalIgnoreCase))
                return VideoEncoding.Vp9;

            if (codec.StartsWith("av01.", StringComparison.OrdinalIgnoreCase))
                return VideoEncoding.Av1;

            // Unknown
            throw new ArgumentOutOfRangeException(nameof(codec), $"Unknown codec [{codec}].");
        }
    }
}
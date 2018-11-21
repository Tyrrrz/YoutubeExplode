using System;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Internal.Helpers
{
    internal static class VideoEncodingHelper
    {
        public static VideoEncoding VideoEncodingFromString(string str)
        {
            if (str.StartsWith("mp4v", StringComparison.OrdinalIgnoreCase))
                return VideoEncoding.Mp4V;

            if (str.StartsWith("avc1", StringComparison.OrdinalIgnoreCase))
                return VideoEncoding.H264;

            if (str.StartsWith("vp8", StringComparison.OrdinalIgnoreCase))
                return VideoEncoding.Vp8;

            if (str.StartsWith("vp9", StringComparison.OrdinalIgnoreCase))
                return VideoEncoding.Vp9;

            if (str.StartsWith("av01", StringComparison.OrdinalIgnoreCase))
                return VideoEncoding.Av1;

            // Unknown
            throw new ArgumentOutOfRangeException(nameof(str), $"Unknown encoding [{str}].");
        }
    }
}
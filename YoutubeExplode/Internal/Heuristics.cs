using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Internal
{
    internal static class Heuristics
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

        private static readonly Dictionary<int, VideoQuality> HeightToVideoQualityMap =
            Enum.GetValues(typeof(VideoQuality)).Cast<VideoQuality>().ToDictionary(
                v => v.ToString().StripNonDigit().ParseInt(), // High1080 => 1080
                v => v);

        public static VideoQuality VideoQualityFromItag(int itag)
        {
            if (itag == 5)
                return VideoQuality.Low144;

            if (itag == 6)
                return VideoQuality.Low240;

            if (itag == 13)
                return VideoQuality.Low144;

            if (itag == 17)
                return VideoQuality.Low144;

            if (itag == 18)
                return VideoQuality.Medium360;

            if (itag == 22)
                return VideoQuality.High720;

            if (itag == 34)
                return VideoQuality.Medium360;

            if (itag == 35)
                return VideoQuality.Medium480;

            if (itag == 36)
                return VideoQuality.Low240;

            if (itag == 37)
                return VideoQuality.High1080;

            if (itag == 38)
                return VideoQuality.High3072;

            if (itag == 43)
                return VideoQuality.Medium360;

            if (itag == 44)
                return VideoQuality.Medium480;

            if (itag == 45)
                return VideoQuality.High720;

            if (itag == 46)
                return VideoQuality.High1080;

            if (itag == 59)
                return VideoQuality.Medium480;

            if (itag == 78)
                return VideoQuality.Medium480;

            if (itag == 82)
                return VideoQuality.Medium360;

            if (itag == 83)
                return VideoQuality.Medium480;

            if (itag == 84)
                return VideoQuality.High720;

            if (itag == 85)
                return VideoQuality.High1080;

            if (itag == 91)
                return VideoQuality.Low144;

            if (itag == 92)
                return VideoQuality.Low240;

            if (itag == 93)
                return VideoQuality.Medium360;

            if (itag == 94)
                return VideoQuality.Medium480;

            if (itag == 95)
                return VideoQuality.High720;

            if (itag == 96)
                return VideoQuality.High1080;

            if (itag == 100)
                return VideoQuality.Medium360;

            if (itag == 101)
                return VideoQuality.Medium480;

            if (itag == 102)
                return VideoQuality.High720;

            if (itag == 132)
                return VideoQuality.Low240;

            if (itag == 151)
                return VideoQuality.Low144;

            // Unknown
            throw new ArgumentException($"Unknown itag [{itag}].", nameof(itag));
        }

        public static VideoQuality VideoQualityFromHeight(int height)
        {
            // Find the video quality by height (highest video quality that has height below or equal to given)
            var matchingHeight = HeightToVideoQualityMap.Keys.LastOrDefault(h => h <= height);

            // Return video quality
            return matchingHeight > 0
                ? HeightToVideoQualityMap[matchingHeight] // if found - return matching quality
                : HeightToVideoQualityMap.Values.First(); // otherwise return lowest available quality
        }

        public static VideoQuality VideoQualityFromLabel(string label)
        {
            // Strip "p" and framerate to get height (e.g. 1080p60 => 1080)
            var height = label.SubstringUntil("p").ParseInt();

            return VideoQualityFromHeight(height);
        }

        public static string VideoQualityToLabel(VideoQuality quality)
        {
            // Convert to string, strip non-digits and add "p"
            return quality.ToString().StripNonDigit() + "p";
        }

        public static string VideoQualityToLabel(VideoQuality quality, int framerate)
        {
            // Framerate appears only if it's above 30
            if (framerate <= 30)
                return VideoQualityToLabel(quality);

            // YouTube rounds framerate to nearest next ten
            var framerateRounded = (int) Math.Ceiling(framerate / 10.0) * 10;
            return VideoQualityToLabel(quality) + framerateRounded;
        }

        public static VideoResolution VideoQualityToResolution(VideoQuality quality)
        {
            if (quality == VideoQuality.Low144)
                return new VideoResolution(256, 144);

            if (quality == VideoQuality.Low240)
                return new VideoResolution(426, 240);

            if (quality == VideoQuality.Medium360)
                return new VideoResolution(640, 360);

            if (quality == VideoQuality.Medium480)
                return new VideoResolution(854, 480);

            if (quality == VideoQuality.High720)
                return new VideoResolution(1280, 720);

            if (quality == VideoQuality.High1080)
                return new VideoResolution(1920, 1080);

            if (quality == VideoQuality.High1440)
                return new VideoResolution(2560, 1440);

            if (quality == VideoQuality.High2160)
                return new VideoResolution(3840, 2160);

            if (quality == VideoQuality.High2880)
                return new VideoResolution(5120, 2880);

            if (quality == VideoQuality.High3072)
                return new VideoResolution(4096, 3072);

            if (quality == VideoQuality.High4320)
                return new VideoResolution(7680, 4320);

            // Unknown
            throw new ArgumentOutOfRangeException(nameof(quality), $"Unknown video quality [{quality}].");
        }
    }
}
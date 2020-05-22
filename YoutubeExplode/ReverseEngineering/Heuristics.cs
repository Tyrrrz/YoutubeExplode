using System;
using YoutubeExplode.Internal.Extensions;
using YoutubeExplode.Videos.Streams;

namespace YoutubeExplode.ReverseEngineering
{
    internal static class Heuristics
    {
        public static VideoQuality GetVideoQuality(int tag) => tag switch
        {
            5 => VideoQuality.Low144,
            6 => VideoQuality.Low240,
            13 => VideoQuality.Low144,
            17 => VideoQuality.Low144,
            18 => VideoQuality.Medium360,
            22 => VideoQuality.High720,
            34 => VideoQuality.Medium360,
            35 => VideoQuality.Medium480,
            36 => VideoQuality.Low240,
            37 => VideoQuality.High1080,
            38 => VideoQuality.High3072,
            43 => VideoQuality.Medium360,
            44 => VideoQuality.Medium480,
            45 => VideoQuality.High720,
            46 => VideoQuality.High1080,
            59 => VideoQuality.Medium480,
            78 => VideoQuality.Medium480,
            82 => VideoQuality.Medium360,
            83 => VideoQuality.Medium480,
            84 => VideoQuality.High720,
            85 => VideoQuality.High1080,
            91 => VideoQuality.Low144,
            92 => VideoQuality.Low240,
            93 => VideoQuality.Medium360,
            94 => VideoQuality.Medium480,
            95 => VideoQuality.High720,
            96 => VideoQuality.High1080,
            100 => VideoQuality.Medium360,
            101 => VideoQuality.Medium480,
            102 => VideoQuality.High720,
            132 => VideoQuality.Low240,
            151 => VideoQuality.Low144,
            133 => VideoQuality.Low240,
            134 => VideoQuality.Medium360,
            135 => VideoQuality.Medium480,
            136 => VideoQuality.High720,
            137 => VideoQuality.High1080,
            138 => VideoQuality.High4320,
            160 => VideoQuality.Low144,
            212 => VideoQuality.Medium480,
            213 => VideoQuality.Medium480,
            214 => VideoQuality.High720,
            215 => VideoQuality.High720,
            216 => VideoQuality.High1080,
            217 => VideoQuality.High1080,
            264 => VideoQuality.High1440,
            266 => VideoQuality.High2160,
            298 => VideoQuality.High720,
            299 => VideoQuality.High1080,
            399 => VideoQuality.High1080,
            398 => VideoQuality.High720,
            397 => VideoQuality.Medium480,
            396 => VideoQuality.Medium360,
            395 => VideoQuality.Low240,
            394 => VideoQuality.Low144,
            167 => VideoQuality.Medium360,
            168 => VideoQuality.Medium480,
            169 => VideoQuality.High720,
            170 => VideoQuality.High1080,
            218 => VideoQuality.Medium480,
            219 => VideoQuality.Medium480,
            242 => VideoQuality.Low240,
            243 => VideoQuality.Medium360,
            244 => VideoQuality.Medium480,
            245 => VideoQuality.Medium480,
            246 => VideoQuality.Medium480,
            247 => VideoQuality.High720,
            248 => VideoQuality.High1080,
            271 => VideoQuality.High1440,
            272 => VideoQuality.High2160,
            278 => VideoQuality.Low144,
            302 => VideoQuality.High720,
            303 => VideoQuality.High1080,
            308 => VideoQuality.High1440,
            313 => VideoQuality.High2160,
            315 => VideoQuality.High2160,
            330 => VideoQuality.Low144,
            331 => VideoQuality.Low240,
            332 => VideoQuality.Medium360,
            333 => VideoQuality.Medium480,
            334 => VideoQuality.High720,
            335 => VideoQuality.High1080,
            336 => VideoQuality.High1440,
            337 => VideoQuality.High2160,
            _ => throw new ArgumentException($"Unrecognized tag '{tag}'.", nameof(tag))
        };

        public static VideoQuality GetVideoQuality(string label) => label switch
        {
            _ when label.StartsWith("4320", StringComparison.Ordinal) => VideoQuality.High4320,
            _ when label.StartsWith("3072", StringComparison.Ordinal) => VideoQuality.High3072,
            _ when label.StartsWith("2880", StringComparison.Ordinal) => VideoQuality.High2880,
            _ when label.StartsWith("2160", StringComparison.Ordinal) => VideoQuality.High2160,
            _ when label.StartsWith("1440", StringComparison.Ordinal) => VideoQuality.High1440,
            _ when label.StartsWith("1080", StringComparison.Ordinal) => VideoQuality.High1080,
            _ when label.StartsWith("720", StringComparison.Ordinal) => VideoQuality.High720,
            _ when label.StartsWith("480", StringComparison.Ordinal) => VideoQuality.Medium480,
            _ when label.StartsWith("360", StringComparison.Ordinal) => VideoQuality.Medium360,
            _ when label.StartsWith("240", StringComparison.Ordinal) => VideoQuality.Low240,
            _ when label.StartsWith("144", StringComparison.Ordinal) => VideoQuality.Low144,
            _ => throw new ArgumentException($"Unrecognized video quality label '{label}'.", nameof(label))
        };

        public static string GetVideoQualityLabel(VideoQuality quality)
        {
            // Convert to string, strip non-digits and add "p"
            return quality.ToString().StripNonDigit() + 'p';
        }

        public static string GetVideoQualityLabel(VideoQuality quality, double framerate)
        {
            // Framerate appears only if it's above 30
            if (framerate <= 30)
                return GetVideoQualityLabel(quality);

            // YouTube rounds framerate to nearest next decimal
            var framerateRounded = (int) Math.Ceiling(framerate / 10.0) * 10;
            return GetVideoQualityLabel(quality) + framerateRounded;
        }

        public static string GetVideoQualityLabel(int tag, double framerate)
        {
            var videoQuality = GetVideoQuality(tag);
            return GetVideoQualityLabel(videoQuality, framerate);
        }

        public static VideoResolution GetVideoResolution(VideoQuality quality) => quality switch
        {
            VideoQuality.Low144 => new VideoResolution(256, 144),
            VideoQuality.Low240 => new VideoResolution(426, 240),
            VideoQuality.Medium360 => new VideoResolution(640, 360),
            VideoQuality.Medium480 => new VideoResolution(854, 480),
            VideoQuality.High720 => new VideoResolution(1280, 720),
            VideoQuality.High1080 => new VideoResolution(1920, 1080),
            VideoQuality.High1440 => new VideoResolution(2560, 1440),
            VideoQuality.High2160 => new VideoResolution(3840, 2160),
            VideoQuality.High2880 => new VideoResolution(5120, 2880),
            VideoQuality.High3072 => new VideoResolution(4096, 3072),
            VideoQuality.High4320 => new VideoResolution(7680, 4320),
            _ => throw new ArgumentException($"Unrecognized video quality '{quality}'.", nameof(quality))
        };
    }
}
using System;
using System.Collections.Generic;
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

            // Unrecognized
            throw new ArgumentException($"Unrecognized audio encoding [{str}].", nameof(str));
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

            // Unrecognized
            throw new ArgumentException($"Unrecognized video encoding [{str}].", nameof(str));
        }

        public static Container ContainerFromString(string str)
        {
            if (str.Equals("mp4", StringComparison.OrdinalIgnoreCase))
                return Container.Mp4;

            if (str.Equals("webm", StringComparison.OrdinalIgnoreCase))
                return Container.WebM;

            if (str.Equals("3gpp", StringComparison.OrdinalIgnoreCase))
                return Container.Tgpp;

            // Unrecognized
            throw new ArgumentException($"Unrecognized container [{str}].", nameof(str));
        }

        private static readonly Dictionary<Container, string> ContainerToFileExtensionMap =
            new Dictionary<Container, string>
            {
                {Container.Mp4, "mp4"},
                {Container.WebM, "webm"},
                {Container.Tgpp, "3gpp"}
            };

        public static string ContainerToFileExtension(Container container)
        {
            return ContainerToFileExtensionMap.TryGetValue(container, out var extension)
                ? extension
                : throw new ArgumentException($"Unrecognized container [{container}].", nameof(container));
        }

        private static readonly Dictionary<int, VideoQuality> ItagToVideoQualityMap =
            new Dictionary<int, VideoQuality>
            {
                {5, VideoQuality.Low144},
                {6, VideoQuality.Low240},
                {13, VideoQuality.Low144},
                {17, VideoQuality.Low144},
                {18, VideoQuality.Medium360},
                {22, VideoQuality.High720},
                {34, VideoQuality.Medium360},
                {35, VideoQuality.Medium480},
                {36, VideoQuality.Low240},
                {37, VideoQuality.High1080},
                {38, VideoQuality.High3072},
                {43, VideoQuality.Medium360},
                {44, VideoQuality.Medium480},
                {45, VideoQuality.High720},
                {46, VideoQuality.High1080},
                {59, VideoQuality.Medium480},
                {78, VideoQuality.Medium480},
                {82, VideoQuality.Medium360},
                {83, VideoQuality.Medium480},
                {84, VideoQuality.High720},
                {85, VideoQuality.High1080},
                {91, VideoQuality.Low144},
                {92, VideoQuality.Low240},
                {93, VideoQuality.Medium360},
                {94, VideoQuality.Medium480},
                {95, VideoQuality.High720},
                {96, VideoQuality.High1080},
                {100, VideoQuality.Medium360},
                {101, VideoQuality.Medium480},
                {102, VideoQuality.High720},
                {132, VideoQuality.Low240},
                {151, VideoQuality.Low144},
                {133, VideoQuality.Low240},
                {134, VideoQuality.Medium360},
                {135, VideoQuality.Medium480},
                {136, VideoQuality.High720},
                {137, VideoQuality.High1080},
                {138, VideoQuality.High4320},
                {160, VideoQuality.Low144},
                {212, VideoQuality.Medium480},
                {213, VideoQuality.Medium480},
                {214, VideoQuality.High720},
                {215, VideoQuality.High720},
                {216, VideoQuality.High1080},
                {217, VideoQuality.High1080},
                {264, VideoQuality.High1440},
                {266, VideoQuality.High2160},
                {298, VideoQuality.High720},
                {299, VideoQuality.High1080},
                {399, VideoQuality.High1080},
                {398, VideoQuality.High720},
                {397, VideoQuality.Medium480},
                {396, VideoQuality.Medium360},
                {395, VideoQuality.Low240},
                {394, VideoQuality.Low144},
                {167, VideoQuality.Medium360},
                {168, VideoQuality.Medium480},
                {169, VideoQuality.High720},
                {170, VideoQuality.High1080},
                {218, VideoQuality.Medium480},
                {219, VideoQuality.Medium480},
                {242, VideoQuality.Low240},
                {243, VideoQuality.Medium360},
                {244, VideoQuality.Medium480},
                {245, VideoQuality.Medium480},
                {246, VideoQuality.Medium480},
                {247, VideoQuality.High720},
                {248, VideoQuality.High1080},
                {271, VideoQuality.High1440},
                {272, VideoQuality.High2160},
                {278, VideoQuality.Low144},
                {302, VideoQuality.High720},
                {303, VideoQuality.High1080},
                {308, VideoQuality.High1440},
                {313, VideoQuality.High2160},
                {315, VideoQuality.High2160},
                {330, VideoQuality.Low144},
                {331, VideoQuality.Low240},
                {332, VideoQuality.Medium360},
                {333, VideoQuality.Medium480},
                {334, VideoQuality.High720},
                {335, VideoQuality.High1080},
                {336, VideoQuality.High1440},
                {337, VideoQuality.High2160}
            };

        public static VideoQuality VideoQualityFromItag(int itag)
        {
            return ItagToVideoQualityMap.TryGetValue(itag, out var quality)
                ? quality
                : throw new ArgumentException($"Unrecognized itag [{itag}].", nameof(itag));
        }

        public static VideoQuality VideoQualityFromLabel(string label)
        {
            if (label.StartsWith("144p", StringComparison.OrdinalIgnoreCase))
                return VideoQuality.Low144;

            if (label.StartsWith("240p", StringComparison.OrdinalIgnoreCase))
                return VideoQuality.Low240;

            if (label.StartsWith("360p", StringComparison.OrdinalIgnoreCase))
                return VideoQuality.Medium360;

            if (label.StartsWith("480p", StringComparison.OrdinalIgnoreCase))
                return VideoQuality.Medium480;

            if (label.StartsWith("720p", StringComparison.OrdinalIgnoreCase))
                return VideoQuality.High720;

            if (label.StartsWith("1080p", StringComparison.OrdinalIgnoreCase))
                return VideoQuality.High1080;

            if (label.StartsWith("1440p", StringComparison.OrdinalIgnoreCase))
                return VideoQuality.High1440;

            if (label.StartsWith("2160p", StringComparison.OrdinalIgnoreCase))
                return VideoQuality.High2160;

            if (label.StartsWith("2880p", StringComparison.OrdinalIgnoreCase))
                return VideoQuality.High2880;

            if (label.StartsWith("3072p", StringComparison.OrdinalIgnoreCase))
                return VideoQuality.High3072;

            if (label.StartsWith("4320p", StringComparison.OrdinalIgnoreCase))
                return VideoQuality.High4320;

            // Unrecognized
            throw new ArgumentException($"Unrecognized video quality label [{label}].", nameof(label));
        }

        public static string VideoQualityToLabel(VideoQuality quality)
        {
            // Convert to string, strip non-digits and add "p"
            return quality.ToString().StripNonDigit() + 'p';
        }

        public static string VideoQualityToLabel(VideoQuality quality, int framerate)
        {
            // Framerate appears only if it's above 30
            if (framerate <= 30)
                return VideoQualityToLabel(quality);

            // YouTube rounds framerate to nearest next decimal
            var framerateRounded = (int) Math.Ceiling(framerate / 10.0) * 10;
            return VideoQualityToLabel(quality) + framerateRounded;
        }

        private static readonly Dictionary<VideoQuality, VideoResolution> VideoQualityToResolutionMap =
            new Dictionary<VideoQuality, VideoResolution>
            {
                {VideoQuality.Low144, new VideoResolution(256, 144)},
                {VideoQuality.Low240, new VideoResolution(426, 240)},
                {VideoQuality.Medium360, new VideoResolution(640, 360)},
                {VideoQuality.Medium480, new VideoResolution(854, 480)},
                {VideoQuality.High720, new VideoResolution(1280, 720)},
                {VideoQuality.High1080, new VideoResolution(1920, 1080)},
                {VideoQuality.High1440, new VideoResolution(2560, 1440)},
                {VideoQuality.High2160, new VideoResolution(3840, 2160)},
                {VideoQuality.High2880, new VideoResolution(5120, 2880)},
                {VideoQuality.High3072, new VideoResolution(4096, 3072)},
                {VideoQuality.High4320, new VideoResolution(7680, 4320)}
            };

        public static VideoResolution VideoQualityToResolution(VideoQuality quality)
        {
            return VideoQualityToResolutionMap.TryGetValue(quality, out var resolution)
                ? resolution
                : throw new ArgumentException($"Unrecognized video quality [{quality}].", nameof(quality));
        }
    }
}
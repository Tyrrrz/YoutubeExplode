using YoutubeExplode.Models;

namespace YoutubeExplode.Internal
{
    internal static class ItagHelper
    {
        public static ContentType GetAdaptiveMode(int itag)
        {
            if (itag.IsEither(160, 133, 134, 135, 136, 298, 137, 299, 264, 266, 138, 278, 242, 243, 244, 247, 248, 271,
                313, 272, 302, 303, 308, 315, 330, 331, 332, 333, 334, 335, 336, 337, 212, 213, 214, 215, 216, 217))
                return ContentType.Video;

            if (itag.IsEither(140, 141, 171, 249, 250, 251))
                return ContentType.Audio;

            return ContentType.Mixed;
        }

        public static bool GetIsVideo3D(int itag)
        {
            return itag.IsEither(82, 83, 84, 85, 100, 101, 102);
        }

        public static bool GetIsLiveStream(int itag)
        {
            return itag.IsEither(91, 92, 93, 94, 95, 96, 127, 128);
        }

        public static ContainerType GetContainerType(int itag)
        {
            if (itag.IsEither(18, 22, 82, 83, 84, 85, 160, 133, 134, 135, 136, 298, 137, 299, 264, 266, 138, 212, 213,
                214, 215, 216, 217))
                return ContainerType.MP4;

            if (itag.IsEither(140, 141))
                return ContainerType.M4A;

            if (itag.IsEither(43, 100, 278, 242, 243, 244, 247, 248, 271, 313, 272, 302, 303, 308, 315, 330, 331, 332,
                333, 334, 335, 336, 337, 171, 249, 250, 251))
                return ContainerType.WebM;

            if (itag.IsEither(13, 17, 36))
                return ContainerType.TGPP;

            if (itag.IsEither(5, 6, 34, 35))
                return ContainerType.FLV;

            if (itag.IsEither(91, 92, 93, 94, 95, 96, 127, 128))
                return ContainerType.TS;

            return ContainerType.Unknown;
        }

        public static VideoQuality GetVideoQuality(int itag)
        {
            if (itag.IsEither(140, 141, 171, 249, 250, 251))
                return VideoQuality.NoVideo;

            if (itag.IsEither(17, 91, 160, 219, 278, 330))
                return VideoQuality.Low144;

            if (itag.IsEither(5, 36, 83, 92, 132, 133, 242, 331))
                return VideoQuality.Low240;

            if (itag.IsEither(18, 34, 43, 82, 93, 100, 134, 167, 243, 332))
                return VideoQuality.Medium360;

            if (itag.IsEither(35, 44, 83, 101, 94, 135, 168, 218, 244, 245, 246, 212, 213))
                return VideoQuality.Medium480;

            if (itag.IsEither(22, 45, 84, 102, 95, 136, 169, 244, 247, 298, 302, 334, 214, 215))
                return VideoQuality.High720;

            if (itag.IsEither(37, 46, 85, 96, 137, 299, 170, 248, 303, 335, 216, 217))
                return VideoQuality.High1080;

            if (itag.IsEither(264, 271, 308, 336))
                return VideoQuality.High1440;

            if (itag.IsEither(138, 266, 272, 313, 315, 337))
                return VideoQuality.High2160;

            if (itag.IsEither(38))
                return VideoQuality.High3072;

            return VideoQuality.Unknown;
        }

        public static string GetFileExtension(int itag)
        {
            var type = GetContainerType(itag);

            if (type == ContainerType.MP4)
                return "mp4";

            if (type == ContainerType.M4A)
                return "m4a";

            if (type == ContainerType.WebM)
                return "webm";

            if (type == ContainerType.TGPP)
                return "3gpp";

            if (type == ContainerType.FLV)
                return "flv";

            if (type == ContainerType.TS)
                return "ts";

            return null;
        }

        public static Resolution GetDefaultResolution(int itag)
        {
            var quality = GetVideoQuality(itag);

            if (quality == VideoQuality.Low144)
                return new Resolution(256, 144);

            if (quality == VideoQuality.Low240)
                return new Resolution(426, 240);

            if (quality == VideoQuality.Medium360)
                return new Resolution(640, 360);

            if (quality == VideoQuality.Medium480)
                return new Resolution(854, 480);

            if (quality == VideoQuality.High720)
                return new Resolution(1280, 720);

            if (quality == VideoQuality.High1080)
                return new Resolution(1920, 1080);

            if (quality == VideoQuality.High1440)
                return new Resolution(2560, 1440);

            if (quality == VideoQuality.High2160)
                return new Resolution(3840, 2160);

            if (quality == VideoQuality.High3072)
                return new Resolution(4096, 3072);

            return Resolution.Empty;
        }
    }
}
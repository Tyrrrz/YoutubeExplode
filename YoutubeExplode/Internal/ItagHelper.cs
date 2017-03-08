using YoutubeExplode.Models;

namespace YoutubeExplode.Internal
{
    internal static class ItagHelper
    {
        public static VideoStreamAdaptiveMode GetAdaptiveMode(int itag)
        {
            if (itag.IsEither(160, 133, 134, 135, 136, 298, 137, 299, 264, 266, 138, 278, 242, 243, 244, 247, 248, 271,
                313, 272, 302, 303, 308, 315, 330, 331, 332, 333, 334, 335, 336, 337))
                return VideoStreamAdaptiveMode.Video;

            if (itag.IsEither(140, 141, 171, 249, 250, 251))
                return VideoStreamAdaptiveMode.Audio;

            return VideoStreamAdaptiveMode.None;
        }

        public static bool GetIs3D(int itag)
        {
            return itag.IsEither(82, 83, 84, 85, 100, 101, 102);
        }

        public static bool GetIsLiveStream(int itag)
        {
            return itag.IsEither(91, 92, 93, 94, 95, 96, 127, 128);
        }

        public static VideoStreamType GetType(int itag)
        {
            if (itag.IsEither(18, 22, 82, 83, 84, 85, 160, 133, 134, 135, 136, 298, 137, 299, 264, 266, 138))
                return VideoStreamType.MP4;

            if (itag.IsEither(140, 141))
                return VideoStreamType.M4A;

            if (itag.IsEither(43, 100, 278, 242, 243, 244, 247, 248, 271, 313, 272, 302, 303, 308, 315, 330, 331, 332,
                333, 334, 335, 336, 337, 171, 249, 250, 251))
                return VideoStreamType.WebM;

            if (itag.IsEither(13, 17, 36))
                return VideoStreamType.TGPP;

            if (itag.IsEither(5, 6, 34, 35))
                return VideoStreamType.FLV;

            if (itag.IsEither(91, 92, 93, 94, 95, 96, 127, 128))
                return VideoStreamType.TS;

            return VideoStreamType.Unknown;
        }

        public static VideoStreamQuality GetQuality(int itag)
        {
            if (itag.IsEither(17, 91, 160, 219, 278, 330))
                return VideoStreamQuality.Low144;

            if (itag.IsEither(5, 36, 83, 92, 132, 133, 242, 331))
                return VideoStreamQuality.Low240;

            if (itag.IsEither(18, 34, 43, 82, 93, 100, 134, 167, 243, 332))
                return VideoStreamQuality.Medium360;

            if (itag.IsEither(35, 44, 83, 101, 94, 135, 168, 218, 244, 245, 246))
                return VideoStreamQuality.Medium480;

            if (itag.IsEither(22, 45, 84, 102, 95, 136, 169, 244, 247, 298, 302, 334))
                return VideoStreamQuality.High720;

            if (itag.IsEither(37, 46, 85, 96, 137, 299, 170, 248, 303, 335))
                return VideoStreamQuality.High1080;

            if (itag.IsEither(264, 271, 308, 336))
                return VideoStreamQuality.High1440;

            if (itag.IsEither(138, 266, 272, 313, 315, 337))
                return VideoStreamQuality.High2160;

            if (itag.IsEither(38))
                return VideoStreamQuality.High3072;

            return VideoStreamQuality.Unknown;
        }

        public static string GetQualityLabel(int itag)
        {
            var quality = GetQuality(itag);

            if (quality == VideoStreamQuality.Low144) return "144p";
            if (quality == VideoStreamQuality.Low240) return "240p";
            if (quality == VideoStreamQuality.Medium360) return "360p";
            if (quality == VideoStreamQuality.Medium480) return "480p";
            if (quality == VideoStreamQuality.High720) return "720p";
            if (quality == VideoStreamQuality.High1080) return "1080p";
            if (quality == VideoStreamQuality.High1440) return "1440p";
            if (quality == VideoStreamQuality.High2160) return "2160p";
            if (quality == VideoStreamQuality.High3072) return "3072p";

            return null;
        }

        public static string GetExtension(int itag)
        {
            var type = GetType(itag);

            if (type == VideoStreamType.MP4)
                return "mp4";

            if (type == VideoStreamType.M4A)
                return "m4a";

            if (type == VideoStreamType.WebM)
                return "webm";

            if (type == VideoStreamType.TGPP)
                return "3gpp";

            if (type == VideoStreamType.FLV)
                return "flv";

            if (type == VideoStreamType.TS)
                return "ts";

            return null;
        }
    }
}
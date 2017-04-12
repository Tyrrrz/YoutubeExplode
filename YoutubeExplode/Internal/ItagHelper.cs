using YoutubeExplode.Models;

namespace YoutubeExplode.Internal
{
    internal static class ItagHelper
    {
        public static MediaStreamContentType GetContentType(int itag)
        {
            if (itag.IsEither(17, 36, 18, 22, 43, 91, 92, 93, 94, 95, 96))
                return MediaStreamContentType.Mixed;

            if (itag.IsEither(160, 133, 134, 135, 136, 298, 137, 299, 264, 266, 138, 278, 242, 243, 244, 247, 248, 271,
                313, 272, 302, 303, 308, 315, 330, 331, 332, 333, 334, 335, 336, 337, 212, 213, 214, 215, 216, 217))
                return MediaStreamContentType.Video;

            if (itag.IsEither(140, 141, 171, 249, 250, 251))
                return MediaStreamContentType.Audio;

            return MediaStreamContentType.Unknown;
        }

        public static bool GetIsVideo3D(int itag)
        {
            return itag.IsEither(82, 83, 84, 85, 100, 101, 102);
        }

        public static bool GetIsLiveStream(int itag)
        {
            return itag.IsEither(91, 92, 93, 94, 95, 96, 127, 128);
        }

        public static MediaStreamContainerType GetContainerType(int itag)
        {
            if (itag.IsEither(18, 22, 82, 83, 84, 85, 160, 133, 134, 135, 136, 298, 137, 299, 264, 266, 138, 212, 213,
                214, 215, 216, 217))
                return MediaStreamContainerType.MP4;

            if (itag.IsEither(140, 141))
                return MediaStreamContainerType.M4A;

            if (itag.IsEither(43, 100, 278, 242, 243, 244, 247, 248, 271, 313, 272, 302, 303, 308, 315, 330, 331, 332,
                333, 334, 335, 336, 337, 171, 249, 250, 251))
                return MediaStreamContainerType.WebM;

            if (itag.IsEither(13, 17, 36))
                return MediaStreamContainerType.TGPP;

            if (itag.IsEither(5, 6, 34, 35))
                return MediaStreamContainerType.FLV;

            if (itag.IsEither(91, 92, 93, 94, 95, 96, 127, 128))
                return MediaStreamContainerType.TS;

            return MediaStreamContainerType.Unknown;
        }

        public static MediaStreamVideoQuality GetVideoQuality(int itag)
        {
            if (itag.IsEither(140, 141, 171, 249, 250, 251))
                return MediaStreamVideoQuality.NoVideo;

            if (itag.IsEither(17, 91, 160, 219, 278, 330))
                return MediaStreamVideoQuality.Low144;

            if (itag.IsEither(5, 36, 83, 92, 132, 133, 242, 331))
                return MediaStreamVideoQuality.Low240;

            if (itag.IsEither(18, 34, 43, 82, 93, 100, 134, 167, 243, 332))
                return MediaStreamVideoQuality.Medium360;

            if (itag.IsEither(35, 44, 83, 101, 94, 135, 168, 218, 244, 245, 246, 212, 213))
                return MediaStreamVideoQuality.Medium480;

            if (itag.IsEither(22, 45, 84, 102, 95, 136, 169, 244, 247, 298, 302, 334, 214, 215))
                return MediaStreamVideoQuality.High720;

            if (itag.IsEither(37, 46, 85, 96, 137, 299, 170, 248, 303, 335, 216, 217))
                return MediaStreamVideoQuality.High1080;

            if (itag.IsEither(264, 271, 308, 336))
                return MediaStreamVideoQuality.High1440;

            if (itag.IsEither(138, 266, 272, 313, 315, 337))
                return MediaStreamVideoQuality.High2160;

            if (itag.IsEither(38))
                return MediaStreamVideoQuality.High3072;

            return MediaStreamVideoQuality.Unknown;
        }

        public static string GetFileExtension(int itag)
        {
            var type = GetContainerType(itag);

            if (type == MediaStreamContainerType.MP4)
                return "mp4";

            if (type == MediaStreamContainerType.M4A)
                return "m4a";

            if (type == MediaStreamContainerType.WebM)
                return "webm";

            if (type == MediaStreamContainerType.TGPP)
                return "3gpp";

            if (type == MediaStreamContainerType.FLV)
                return "flv";

            if (type == MediaStreamContainerType.TS)
                return "ts";

            return type.ToString();
        }

        public static MediaStreamVideoResolution GetDefaultResolution(int itag)
        {
            var quality = GetVideoQuality(itag);

            if (quality == MediaStreamVideoQuality.Low144)
                return new MediaStreamVideoResolution(256, 144);

            if (quality == MediaStreamVideoQuality.Low240)
                return new MediaStreamVideoResolution(426, 240);

            if (quality == MediaStreamVideoQuality.Medium360)
                return new MediaStreamVideoResolution(640, 360);

            if (quality == MediaStreamVideoQuality.Medium480)
                return new MediaStreamVideoResolution(854, 480);

            if (quality == MediaStreamVideoQuality.High720)
                return new MediaStreamVideoResolution(1280, 720);

            if (quality == MediaStreamVideoQuality.High1080)
                return new MediaStreamVideoResolution(1920, 1080);

            if (quality == MediaStreamVideoQuality.High1440)
                return new MediaStreamVideoResolution(2560, 1440);

            if (quality == MediaStreamVideoQuality.High2160)
                return new MediaStreamVideoResolution(3840, 2160);

            if (quality == MediaStreamVideoQuality.High3072)
                return new MediaStreamVideoResolution(4096, 3072);

            return MediaStreamVideoResolution.Empty;
        }
    }
}
using System;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Model extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets file extension based on container type
        /// </summary>
        public static string GetFileExtension(this Container container)
        {
            switch (container)
            {
                case Container.Mp4:
                    return "mp4";
                case Container.M4A:
                    return "m4a";
                case Container.WebM:
                    return "webm";
                case Container.Tgpp:
                    return "3gpp";
                case Container.Flv:
                    return "flv";
                default:
                    throw new ArgumentOutOfRangeException(nameof(container), "Unknown container type");
            }
        }

        /// <summary>
        /// Gets label for given video quality and framerate
        /// </summary>
        public static string GetVideoQualityLabel(this VideoQuality videoQuality, int framerate = 30)
        {
            // Video quality
            string qualityPart;
            if (videoQuality == VideoQuality.Low144)
                qualityPart = "144p";

            else if (videoQuality == VideoQuality.Low240)
                qualityPart = "240p";

            else if (videoQuality == VideoQuality.Medium360)
                qualityPart = "360p";

            else if (videoQuality == VideoQuality.Medium480)
                qualityPart = "480p";

            else if (videoQuality == VideoQuality.High720)
                qualityPart = "720p";

            else if (videoQuality == VideoQuality.High1080)
                qualityPart = "1080p";

            else if (videoQuality == VideoQuality.High1440)
                qualityPart = "1440p";

            else if (videoQuality == VideoQuality.High2160)
                qualityPart = "2160p";

            else if (videoQuality == VideoQuality.High3072)
                qualityPart = "3072p";

            else if (videoQuality == VideoQuality.High4320)
                qualityPart = "4320p";

            else
                throw new ArgumentOutOfRangeException(nameof(videoQuality),
                    $"Unexpected video quality [{videoQuality}]");

            // Framerate
            var frameratePart = framerate > 30 ? framerate.ToString() : string.Empty;

            return qualityPart + frameratePart;
        }
    }
}
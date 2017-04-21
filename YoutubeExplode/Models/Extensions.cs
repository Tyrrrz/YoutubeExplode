﻿using System;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Model extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Get file extension based on container type
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
                case Container.Ts:
                    return "ts";
                default:
                    throw new ArgumentOutOfRangeException(nameof(container), "Unknown container type");
            }
        }

        /// <summary>
        /// Get video quality label based on video quality and framerate
        /// </summary>
        public static string GetLabel(this VideoQuality videoQuality, double framerate = 0)
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

            else
                throw new ArgumentOutOfRangeException(nameof(videoQuality), "Unknown video quality");

            // Framerate
            var frameratePart = framerate > 30 ? framerate.ToString("F0") : string.Empty;

            return qualityPart + frameratePart;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Internal.Parsers
{
    internal static class VideoQualityConverter
    {
        private static readonly Dictionary<string, VideoQuality> VideoQualityLabelMap =
            Enum.GetValues(typeof(VideoQuality)).Cast<VideoQuality>().ToDictionary(
                v => v.ToString().StripNonDigit(), // High1080 => 1080
                v => v);

        public static VideoQuality FromVideoQualityLabel(string label)
        {
            // Strip "p" and framerate
            var videoQualityStr = label.SubstringUntil("p");

            // Try to find matching video quality
            return VideoQualityLabelMap.TryGetValue(videoQualityStr, out var videoQuality)
                ? videoQuality
                : throw new FormatException($"Could not parse video quality from given string [{label}].");
        }
    }
}
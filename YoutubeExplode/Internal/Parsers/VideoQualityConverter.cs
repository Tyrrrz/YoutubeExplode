using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Models.MediaStreams;

namespace YoutubeExplode.Internal.Parsers
{
    internal static class VideoQualityConverter
    {
        private static readonly Dictionary<int, VideoQuality> HeightToVideoQualityMap =
            Enum.GetValues(typeof(VideoQuality)).Cast<VideoQuality>().ToDictionary(
                v => v.ToString().StripNonDigit().ParseInt(), // High1080 => 1080
                v => v);

        public static VideoQuality VideoQualityFromHeight(int height)
        {
            // Find the video quality by height (lowest video quality that has height above or equal to given)
            var matchingHeight = HeightToVideoQualityMap.Keys.FirstOrDefault(h => h >= height);

            // If found - return video quality
            return matchingHeight > 0
                ? HeightToVideoQualityMap[matchingHeight] // if found - return matching quality
                : HeightToVideoQualityMap.Values.Last(); // otherwise return highest available quality
        }

        public static VideoQuality VideoQualityFromLabel(string label)
        {
            // Strip "p" and framerate to get height (e.g. >1080<p60)
            var heightStr = label.SubstringUntil("p");
            var height = heightStr.ParseInt();

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
    }
}
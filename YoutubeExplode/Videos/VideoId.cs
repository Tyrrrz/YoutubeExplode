using System;
using System.Text.RegularExpressions;

namespace YoutubeExplode.Videos
{
    [Equals(DoNotAddEqualityOperators = true)]
    public readonly partial struct VideoId
    {
        public string Value { get; }

        public VideoId(string idOrUrl) =>
            Value = TryNormalize(idOrUrl) ??
                    throw new ArgumentException($"Invalid YouTube video ID or URL: '{idOrUrl}'.");

        /// <inheritdoc />
        public override string ToString() => Value;
    }

    public partial struct VideoId
    {
        public static implicit operator VideoId(string idOrUrl) => new VideoId(idOrUrl);

        public static implicit operator string(VideoId id) => id.ToString();
    }

    public partial struct VideoId
    {
        private static bool IsValid(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            // Video IDs are always 11 characters
            if (id.Length != 11)
                return false;

            return !Regex.IsMatch(id, @"[^0-9a-zA-Z_\-]");
        }

        private static string? TryNormalize(string idOrUrl)
        {
            if (string.IsNullOrWhiteSpace(idOrUrl))
                return null;

            // Id
            // yIVRs6YSbOM
            if (IsValid(idOrUrl))
                return idOrUrl;

            // Regular URL
            // https://www.youtube.com/watch?v=yIVRs6YSbOM
            var regularMatch = Regex.Match(idOrUrl, @"youtube\..+?/watch.*?v=(.*?)(?:&|/|$)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(regularMatch) && IsValid(regularMatch))
            {
                return regularMatch;
            }

            // Short URL
            // https://youtu.be/yIVRs6YSbOM
            var shortMatch = Regex.Match(idOrUrl, @"youtu\.be/(.*?)(?:\?|&|/|$)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(shortMatch) && IsValid(shortMatch))
            {
                return shortMatch;
            }

            // Embed URL
            // https://www.youtube.com/embed/yIVRs6YSbOM
            var embedMatch = Regex.Match(idOrUrl, @"youtube\..+?/embed/(.*?)(?:\?|&|/|$)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(embedMatch) && IsValid(embedMatch))
            {
                return embedMatch;
            }

            // Invalid input
            return null;
        }
    }
}
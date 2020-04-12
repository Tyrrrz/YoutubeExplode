using System;
using System.Text.RegularExpressions;

namespace YoutubeExplode.Channels
{
    /// <summary>
    /// Encapsulates a valid YouTube channel ID.
    /// </summary>
    [Equals(DoNotAddEqualityOperators = true)]
    public readonly partial struct ChannelId
    {
        /// <summary>
        /// ID as a string.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ChannelId"/>.
        /// </summary>
        public ChannelId(string idOrUrl) =>
            Value = TryNormalize(idOrUrl) ??
                    throw new ArgumentException($"Invalid YouTube channel ID or URL: '{idOrUrl}'.");

        /// <inheritdoc />
        public override string ToString() => Value;
    }

    public partial struct ChannelId
    {
        /// <summary>
        /// Converts string to ID.
        /// </summary>
        public static implicit operator ChannelId(string idOrUrl) => new ChannelId(idOrUrl);

        /// <summary>
        /// Converts ID to string.
        /// </summary>
        public static implicit operator string(ChannelId id) => id.ToString();
    }

    public partial struct ChannelId
    {
        private static bool IsValid(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            // Channel IDs should start with these characters
            if (!id.StartsWith("UC", StringComparison.Ordinal))
                return false;

            // Channel IDs are always 24 characters
            if (id.Length != 24)
                return false;

            return !Regex.IsMatch(id, @"[^0-9a-zA-Z_\-]");
        }

        private static string? TryNormalize(string? idOrUrl)
        {
            if (string.IsNullOrWhiteSpace(idOrUrl))
                return null;

            // Id
            // UC3xnGqlcL3y-GXz5N3wiTJQ
            if (IsValid(idOrUrl))
                return idOrUrl;

            // URL
            // https://www.youtube.com/channel/UC3xnGqlcL3y-GXz5N3wiTJQ
            var regularMatch = Regex.Match(idOrUrl, @"youtube\..+?/channel/(.*?)(?:\?|&|/|$)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(regularMatch) && IsValid(regularMatch))
            {
                return regularMatch;
            }

            // Invalid input
            return null;
        }
    }
}
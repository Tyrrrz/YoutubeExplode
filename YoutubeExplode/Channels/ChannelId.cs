using System;
using System.Text.RegularExpressions;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Channels
{
    /// <summary>
    /// Encapsulates a valid YouTube channel ID.
    /// </summary>
    public readonly partial struct ChannelId
    {
        /// <summary>
        /// ID as a string.
        /// </summary>
        public string Value { get; }

        private ChannelId(string value) => Value = value;

        /// <inheritdoc />
        public override string ToString() => Value;
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
                return regularMatch;

            // Invalid input
            return null;
        }

        /// <summary>
        /// Attempts to parse the specified string as a YouTube channel ID or URL.
        /// Returns null in case of failure.
        /// </summary>
        public static ChannelId? TryParse(string? idOrUrl) =>
            TryNormalize(idOrUrl)?.Pipe(id => new ChannelId(id));

        /// <summary>
        /// Parses the specified string as a YouTube channel ID or URL.
        /// Throws an exception in case of failure.
        /// </summary>
        public static ChannelId Parse(string idOrUrl) =>
            TryParse(idOrUrl) ??
            throw new ArgumentException($"Invalid YouTube channel ID or URL: '{idOrUrl}'.");

        /// <summary>
        /// Converts string to ID.
        /// </summary>
        public static implicit operator ChannelId(string idOrUrl) => Parse(idOrUrl);

        /// <summary>
        /// Converts ID to string.
        /// </summary>
        public static implicit operator string(ChannelId id) => id.ToString();
    }

    public partial struct ChannelId : IEquatable<ChannelId>
    {
        /// <inheritdoc />
        public bool Equals(ChannelId other) => StringComparer.Ordinal.Equals(Value, other.Value);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is ChannelId other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

        /// <summary>
        /// Equality check.
        /// </summary>
        public static bool operator ==(ChannelId left, ChannelId right) => left.Equals(right);

        /// <summary>
        /// Equality check.
        /// </summary>
        public static bool operator !=(ChannelId left, ChannelId right) => !(left == right);
    }
}
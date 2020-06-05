using System;
using System.Text.RegularExpressions;
using YoutubeExplode.Internal.Extensions;

namespace YoutubeExplode.Playlists
{
    /// <summary>
    /// Encapsulates a valid YouTube playlist ID.
    /// </summary>
    public readonly partial struct PlaylistId
    {
        /// <summary>
        /// ID as a string.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Initializes an instance of <see cref="PlaylistId"/>.
        /// </summary>
        public PlaylistId(string idOrUrl) =>
            Value = TryNormalize(idOrUrl) ??
                    throw new ArgumentException($"Invalid YouTube playlist ID or URL: '{idOrUrl}'.");

        /// <inheritdoc />
        public override string ToString() => Value;
    }

    public partial struct PlaylistId : IEquatable<PlaylistId>
    {
        /// <inheritdoc />
        public bool Equals(PlaylistId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is PlaylistId other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Value);

        /// <summary>
        /// Equality check.
        /// </summary>
        public static bool operator ==(PlaylistId left, PlaylistId right) => left.Equals(right);

        /// <summary>
        /// Equality check.
        /// </summary>
        public static bool operator !=(PlaylistId left, PlaylistId right) => !(left == right);
    }

    public partial struct PlaylistId
    {
        /// <summary>
        /// Converts string to ID.
        /// </summary>
        public static implicit operator PlaylistId(string idOrUrl) => new PlaylistId(idOrUrl);

        /// <summary>
        /// Converts ID to string.
        /// </summary>
        public static implicit operator string(PlaylistId id) => id.ToString();
    }

    public partial struct PlaylistId
    {
        private static bool IsValid(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            // Watch later playlist is special
            if (id == "WL")
                return true;

            // My Mix playlist is special
            if (id == "RDMM")
                return true;

            // Other playlist IDs should start with these two characters
            if (!id.StartsWith("PL", StringComparison.Ordinal) &&
                !id.StartsWith("RD", StringComparison.Ordinal) &&
                !id.StartsWith("UL", StringComparison.Ordinal) &&
                !id.StartsWith("UU", StringComparison.Ordinal) &&
                !id.StartsWith("PU", StringComparison.Ordinal) &&
                !id.StartsWith("OL", StringComparison.Ordinal) &&
                !id.StartsWith("LL", StringComparison.Ordinal) &&
                !id.StartsWith("FL", StringComparison.Ordinal))
                return false;

            // Playlist IDs vary a lot in lengths
            if (id.Length < 13)
                return false;

            return !Regex.IsMatch(id, @"[^0-9a-zA-Z_\-]");
        }

        private static string? TryNormalize(string? idOrUrl)
        {
            if (string.IsNullOrWhiteSpace(idOrUrl))
                return null;

            // Id
            // PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H
            if (IsValid(idOrUrl))
                return idOrUrl;

            // Regular URL
            // https://www.youtube.com/playlist?list=PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H
            var regularMatch = Regex.Match(idOrUrl, @"youtube\..+?/playlist.*?list=(.*?)(?:&|/|$)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(regularMatch) && IsValid(regularMatch))
                return regularMatch;

            // Composite URL (video + playlist)
            // https://www.youtube.com/watch?v=b8m9zhNAgKs&list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
            var compositeMatch = Regex.Match(idOrUrl, @"youtube\..+?/watch.*?list=(.*?)(?:&|/|$)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(compositeMatch) && IsValid(compositeMatch))
                return compositeMatch;

            // Short composite URL (video + playlist)
            // https://youtu.be/b8m9zhNAgKs/?list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
            var shortCompositeMatch = Regex.Match(idOrUrl, @"youtu\.be/.*?/.*?list=(.*?)(?:&|/|$)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(shortCompositeMatch) && IsValid(shortCompositeMatch))
                return shortCompositeMatch;

            // Embed URL
            // https://www.youtube.com/embed/b8m9zhNAgKs/?list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
            var embedCompositeMatch = Regex.Match(idOrUrl, @"youtube\..+?/embed/.*?/.*?list=(.*?)(?:&|/|$)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(embedCompositeMatch) && IsValid(embedCompositeMatch))
                return embedCompositeMatch;

            // Invalid input
            return null;
        }

        /// <summary>
        /// Attempts to parse the specified string as a playlist ID or URL.
        /// Returns null in case of failure.
        /// </summary>
        public static PlaylistId? TryParse(string? idOrUrl) =>
            TryNormalize(idOrUrl)?.Pipe(id => new PlaylistId(id));
    }
}
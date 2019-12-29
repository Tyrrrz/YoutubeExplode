using System;
using System.Text.RegularExpressions;

namespace YoutubeExplode
{
    public partial class YoutubeClient
    {
        /// <summary>
        /// Verifies that the given string is syntactically a valid YouTube video ID.
        /// </summary>
        public static bool ValidateVideoId(string videoId)
        {
            if (string.IsNullOrWhiteSpace(videoId))
                return false;

            // Video IDs are always 11 characters
            if (videoId.Length != 11)
                return false;

            return !Regex.IsMatch(videoId, @"[^0-9a-zA-Z_\-]");
        }

        /// <summary>
        /// Tries to parse video ID from a YouTube video URL.
        /// </summary>
        public static bool TryParseVideoId(string videoUrl, out string? videoId)
        {
            videoId = default;

            if (string.IsNullOrWhiteSpace(videoUrl))
                return false;

            // https://www.youtube.com/watch?v=yIVRs6YSbOM
            var regularMatch = Regex.Match(videoUrl, @"youtube\..+?/watch.*?v=(.*?)(?:&|/|$)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(regularMatch) && ValidateVideoId(regularMatch))
            {
                videoId = regularMatch;
                return true;
            }

            // https://youtu.be/yIVRs6YSbOM
            var shortMatch = Regex.Match(videoUrl, @"youtu\.be/(.*?)(?:\?|&|/|$)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(shortMatch) && ValidateVideoId(shortMatch))
            {
                videoId = shortMatch;
                return true;
            }

            // https://www.youtube.com/embed/yIVRs6YSbOM
            var embedMatch = Regex.Match(videoUrl, @"youtube\..+?/embed/(.*?)(?:\?|&|/|$)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(embedMatch) && ValidateVideoId(embedMatch))
            {
                videoId = embedMatch;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parses video ID from a YouTube video URL.
        /// </summary>
        public static string ParseVideoId(string videoUrl) =>
            TryParseVideoId(videoUrl, out var result)
                ? result!
                : throw new FormatException($"Could not parse video ID from given string [{videoUrl}].");

        /// <summary>
        /// Verifies that the given string is syntactically a valid YouTube playlist ID.
        /// </summary>
        public static bool ValidatePlaylistId(string playlistId)
        {
            if (string.IsNullOrWhiteSpace(playlistId))
                return false;

            // Watch later playlist is special
            if (playlistId == "WL")
                return true;

            // My Mix playlist is special
            if (playlistId == "RDMM")
                return true;

            // Other playlist IDs should start with these two characters
            if (!playlistId.StartsWith("PL", StringComparison.Ordinal) &&
                !playlistId.StartsWith("RD", StringComparison.Ordinal) &&
                !playlistId.StartsWith("UL", StringComparison.Ordinal) &&
                !playlistId.StartsWith("UU", StringComparison.Ordinal) &&
                !playlistId.StartsWith("PU", StringComparison.Ordinal) &&
                !playlistId.StartsWith("OL", StringComparison.Ordinal) &&
                !playlistId.StartsWith("LL", StringComparison.Ordinal) &&
                !playlistId.StartsWith("FL", StringComparison.Ordinal))
                return false;

            // Playlist IDs vary a lot in lengths
            if (playlistId.Length < 13)
                return false;

            return !Regex.IsMatch(playlistId, @"[^0-9a-zA-Z_\-]");
        }

        /// <summary>
        /// Tries to parse playlist ID from a YouTube playlist URL.
        /// </summary>
        public static bool TryParsePlaylistId(string playlistUrl, out string? playlistId)
        {
            playlistId = default;

            if (string.IsNullOrWhiteSpace(playlistUrl))
                return false;

            // https://www.youtube.com/playlist?list=PLOU2XLYxmsIJGErt5rrCqaSGTMyyqNt2H
            var regularMatch = Regex.Match(playlistUrl, @"youtube\..+?/playlist.*?list=(.*?)(?:&|/|$)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(regularMatch) && ValidatePlaylistId(regularMatch))
            {
                playlistId = regularMatch;
                return true;
            }

            // https://www.youtube.com/watch?v=b8m9zhNAgKs&list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
            var compositeMatch = Regex.Match(playlistUrl, @"youtube\..+?/watch.*?list=(.*?)(?:&|/|$)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(compositeMatch) && ValidatePlaylistId(compositeMatch))
            {
                playlistId = compositeMatch;
                return true;
            }

            // https://youtu.be/b8m9zhNAgKs/?list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
            var shortCompositeMatch = Regex.Match(playlistUrl, @"youtu\.be/.*?/.*?list=(.*?)(?:&|/|$)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(shortCompositeMatch) && ValidatePlaylistId(shortCompositeMatch))
            {
                playlistId = shortCompositeMatch;
                return true;
            }

            // https://www.youtube.com/embed/b8m9zhNAgKs/?list=PL9tY0BWXOZFuFEG_GtOBZ8-8wbkH-NVAr
            var embedCompositeMatch = Regex.Match(playlistUrl, @"youtube\..+?/embed/.*?/.*?list=(.*?)(?:&|/|$)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(embedCompositeMatch) && ValidatePlaylistId(embedCompositeMatch))
            {
                playlistId = embedCompositeMatch;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parses playlist ID from a YouTube playlist URL.
        /// </summary>
        public static string ParsePlaylistId(string playlistUrl) =>
            TryParsePlaylistId(playlistUrl, out var result)
                ? result!
                : throw new FormatException($"Could not parse playlist ID from given string [{playlistUrl}].");

        /// <summary>
        /// Verifies that the given string is syntactically a valid YouTube username.
        /// </summary>
        public static bool ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            // Usernames can't be longer than 20 characters
            if (username.Length > 20)
                return false;

            return !Regex.IsMatch(username, @"[^0-9a-zA-Z]");
        }

        /// <summary>
        /// Tries to parse username from a YouTube user URL.
        /// </summary>
        public static bool TryParseUsername(string userUrl, out string? username)
        {
            username = default;

            if (string.IsNullOrWhiteSpace(userUrl))
                return false;

            // https://www.youtube.com/user/TheTyrrr
            var regularMatch = Regex.Match(userUrl, @"youtube\..+?/user/(.*?)(?:\?|&|/|$)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(regularMatch) && ValidateUsername(regularMatch))
            {
                username = regularMatch;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parses username from a YouTube user URL.
        /// </summary>
        public static string ParseUsername(string userUrl) =>
            TryParseUsername(userUrl, out var username)
                ? username!
                : throw new FormatException($"Could not parse username from given string [{userUrl}].");

        /// <summary>
        /// Verifies that the given string is syntactically a valid YouTube channel ID.
        /// </summary>
        public static bool ValidateChannelId(string channelId)
        {
            if (string.IsNullOrWhiteSpace(channelId))
                return false;

            // Channel IDs should start with these characters
            if (!channelId.StartsWith("UC", StringComparison.Ordinal))
                return false;

            // Channel IDs are always 24 characters
            if (channelId.Length != 24)
                return false;

            return !Regex.IsMatch(channelId, @"[^0-9a-zA-Z_\-]");
        }

        /// <summary>
        /// Tries to parse channel ID from a YouTube channel URL.
        /// </summary>
        public static bool TryParseChannelId(string channelUrl, out string? channelId)
        {
            channelId = default;

            if (string.IsNullOrWhiteSpace(channelUrl))
                return false;

            // https://www.youtube.com/channel/UC3xnGqlcL3y-GXz5N3wiTJQ
            var regularMatch = Regex.Match(channelUrl, @"youtube\..+?/channel/(.*?)(?:\?|&|/|$)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(regularMatch) && ValidateChannelId(regularMatch))
            {
                channelId = regularMatch;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Parses channel ID from a YouTube channel URL.
        /// </summary>
        public static string ParseChannelId(string channelUrl) =>
            TryParseChannelId(channelUrl, out var result)
                ? result!
                : throw new FormatException($"Could not parse channel ID from given string [{channelUrl}].");
    }
}

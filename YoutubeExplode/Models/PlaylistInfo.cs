using System;
using System.Collections.Generic;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Playlist metadata
    /// </summary>
    public class PlaylistInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Type
        /// </summary>
        public PlaylistType Type
        {
            get
            {
                if (Id.StartsWith("PL", StringComparison.OrdinalIgnoreCase))
                    return PlaylistType.UserMade;

                if (Id.StartsWith("RD", StringComparison.OrdinalIgnoreCase))
                    return PlaylistType.Mix;

                if (Id.StartsWith("LL", StringComparison.OrdinalIgnoreCase))
                    return PlaylistType.Liked;

                if (Id.StartsWith("FL", StringComparison.OrdinalIgnoreCase))
                    return PlaylistType.Favorites;

                if (Id.StartsWith("WL", StringComparison.OrdinalIgnoreCase))
                    return PlaylistType.WatchLater;

                return PlaylistType.Unknown;
            }
        }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// Author's display name
        /// </summary>
        public string Author { get; internal set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// View count
        /// </summary>
        public long ViewCount { get; internal set; }

        /// <summary>
        /// IDs of the videos in the playlist
        /// </summary>
        public IReadOnlyList<string> VideoIds { get; internal set; }

        internal PlaylistInfo()
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Title}";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Playlist info
    /// </summary>
    public partial class PlaylistInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Type
        /// </summary>
        public PlaylistType Type { get; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Author's display name
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// View count
        /// </summary>
        public long ViewCount { get; }

        /// <summary>
        /// IDs of the videos in the playlist
        /// </summary>
        public IReadOnlyList<string> VideoIds { get; }

        /// <inheritdoc />
        public PlaylistInfo(string id, string title, string author, string description, long viewCount,
            IEnumerable<string> videoIds)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Type = GetPlaylistType(id);
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Author = author ?? throw new ArgumentNullException(nameof(author));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            ViewCount = viewCount >= 0 ? viewCount : throw new ArgumentOutOfRangeException(nameof(viewCount));
            VideoIds = videoIds?.ToArray() ?? throw new ArgumentNullException(nameof(videoIds));
        }
    }

    public partial class PlaylistInfo
    {
        /// <summary>
        /// Get playlist type from playlist id
        /// </summary>
        protected static PlaylistType GetPlaylistType(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            if (id.StartsWith("PL", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.UserMade;

            if (id.StartsWith("RD", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.VideoMix;

            if (id.StartsWith("UL", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.ChannelMix;

            if (id.StartsWith("UU", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.ChannelUploads;

            if (id.StartsWith("PU", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.ChannelPopular;

            if (id.StartsWith("LL", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.Liked;

            if (id.StartsWith("FL", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.Favorites;

            if (id.StartsWith("WL", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.WatchLater;

            throw new ArgumentOutOfRangeException(nameof(id), $"Unexpected playlist ID [{id}]");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Exceptions;

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
        /// Like count
        /// </summary>
        public long LikeCount { get; }

        /// <summary>
        /// Dislike count
        /// </summary>
        public long DislikeCount { get; }

        /// <summary>
        /// Average user rating in stars (0* to 5*)
        /// </summary>
        public double AverageRating => 5.0 * LikeCount / (LikeCount + DislikeCount);

        /// <summary>
        /// IDs of the videos in the playlist
        /// </summary>
        public IReadOnlyList<string> VideoIds { get; }

        internal PlaylistInfo(string id, string title, string author, string description, long viewCount,
            long likeCount, long dislikeCount, IEnumerable<string> videoIds)
        {
            Id = id;
            Type = GetPlaylistType(id);
            Title = title;
            Author = author;
            Description = description;
            ViewCount = viewCount;
            LikeCount = likeCount;
            DislikeCount = dislikeCount;
            VideoIds = videoIds.ToArray();
        }
    }

    public partial class PlaylistInfo
    {
        /// <summary>
        /// Get playlist type from playlist id
        /// </summary>
        protected static PlaylistType GetPlaylistType(string id)
        {
            if (id.StartsWith("PL", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.UserMade;

            if (id.StartsWith("RD", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.VideoMix;

            if (id.StartsWith("UL", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.ChannelMix;

            if (id.StartsWith("LL", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.Liked;

            if (id.StartsWith("FL", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.Favorites;

            if (id.StartsWith("WL", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.WatchLater;

            throw new UnexpectedIdentifierException($"Unexpected playlist ID [{id}]");
        }
    }
}
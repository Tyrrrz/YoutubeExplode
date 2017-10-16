using System;
using System.Collections.Generic;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Playlist
    /// </summary>
    public partial class Playlist
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
        /// Description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Author's display name
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// Statistics
        /// </summary>
        public Statistics Statistics { get; }

        /// <summary>
        /// Videos in the playlist
        /// </summary>
        public IReadOnlyList<PlaylistVideo> Videos { get; }

        /// <summary />
        public Playlist(string id, string title, string author, string description, Statistics statistics,
            IReadOnlyList<PlaylistVideo> videos)
        {
            Id = id.GuardNotNull(nameof(id));
            Type = GetPlaylistType(id);
            Title = title.GuardNotNull(nameof(title));
            Author = author.GuardNotNull(nameof(author));
            Description = description.GuardNotNull(nameof(description));
            Statistics = statistics.GuardNotNull(nameof(statistics));
            Videos = videos.GuardNotNull(nameof(videos));
        }

        /// <inheritdoc />
        public override string ToString() => Title;
    }

    public partial class Playlist
    {
        /// <summary>
        /// Get playlist type from playlist id
        /// </summary>
        protected static PlaylistType GetPlaylistType(string id)
        {
            id.GuardNotNull(nameof(id));

            if (id.StartsWith("PL", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.Normal;

            if (id.StartsWith("RD", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.VideoMix;

            if (id.StartsWith("UL", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.ChannelVideoMix;

            if (id.StartsWith("UU", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.ChannelVideos;

            if (id.StartsWith("PU", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.PopularChannelVideos;

            if (id.StartsWith("LL", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.LikedVideos;

            if (id.StartsWith("FL", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.Favorites;

            if (id.StartsWith("WL", StringComparison.OrdinalIgnoreCase))
                return PlaylistType.WatchLater;

            throw new ArgumentOutOfRangeException(nameof(id), $"Unexpected playlist ID [{id}]");
        }
    }
}
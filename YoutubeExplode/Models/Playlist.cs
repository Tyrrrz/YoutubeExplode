using System;
using System.Collections.Generic;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Information about a YouTube playlist.
    /// </summary>
    public partial class Playlist
    {
        /// <summary>
        /// ID of this playlist.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Type of this playlist.
        /// </summary>
        public PlaylistType Type { get; }

        /// <summary>
        /// Author of this playlist.
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// Title of this playlist.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Description of this playlist.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Statistics of this playlist.
        /// </summary>
        public Statistics Statistics { get; }

        /// <summary>
        /// Collection of <see cref="PlaylistVideo"/>s contained in this playlist.
        /// </summary>
        public IReadOnlyList<PlaylistVideo> Videos { get; }

        /// <summary />
        public Playlist(string id, string author, string title, string description, Statistics statistics,
            IReadOnlyList<PlaylistVideo> videos)
        {
            Id = id.GuardNotNull(nameof(id));
            Type = GetPlaylistType(id);
            Author = author.GuardNotNull(nameof(author));
            Title = title.GuardNotNull(nameof(title));
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
        /// Get playlist type by ID.
        /// </summary>
        protected static PlaylistType GetPlaylistType(string id)
        {
            id.GuardNotNull(nameof(id));

            if (id.StartsWith("PL"))
                return PlaylistType.Normal;

            if (id.StartsWith("RD"))
                return PlaylistType.VideoMix;

            if (id.StartsWith("UL"))
                return PlaylistType.ChannelVideoMix;

            if (id.StartsWith("UU"))
                return PlaylistType.ChannelVideos;

            if (id.StartsWith("PU"))
                return PlaylistType.PopularChannelVideos;

            if (id.StartsWith("LL"))
                return PlaylistType.LikedVideos;

            if (id.StartsWith("FL"))
                return PlaylistType.Favorites;

            if (id.StartsWith("WL"))
                return PlaylistType.WatchLater;

            throw new ArgumentOutOfRangeException(nameof(id), $"Unexpected playlist ID [{id}].");
        }
    }
}
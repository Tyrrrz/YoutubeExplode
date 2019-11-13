using System;
using System.Collections.Generic;

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
        /// Collection of videos contained in this playlist.
        /// </summary>
        public IReadOnlyList<Video> Videos { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Playlist"/>.
        /// </summary>
        public Playlist(string id, string author, string title, string description, Statistics statistics,
            IReadOnlyList<Video> videos)
        {
            Id = id;
            Type = GetPlaylistType(id);
            Author = author;
            Title = title;
            Description = description;
            Statistics = statistics;
            Videos = videos;
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
            if (id.StartsWith("PL", StringComparison.Ordinal))
                return PlaylistType.Normal;

            if (id.StartsWith("RD", StringComparison.Ordinal))
                return PlaylistType.VideoMix;

            if (id.StartsWith("UL", StringComparison.Ordinal))
                return PlaylistType.ChannelVideoMix;

            if (id.StartsWith("UU", StringComparison.Ordinal))
                return PlaylistType.ChannelVideos;

            if (id.StartsWith("PU", StringComparison.Ordinal))
                return PlaylistType.PopularChannelVideos;

            if (id.StartsWith("OL", StringComparison.Ordinal))
                return PlaylistType.MusicAlbum;

            if (id.StartsWith("LL", StringComparison.Ordinal))
                return PlaylistType.LikedVideos;

            if (id.StartsWith("FL", StringComparison.Ordinal))
                return PlaylistType.Favorites;

            if (id.StartsWith("WL", StringComparison.Ordinal))
                return PlaylistType.WatchLater;

            throw new ArgumentOutOfRangeException(nameof(id), $"Unexpected playlist ID [{id}].");
        }
    }
}
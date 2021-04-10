using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Common;

namespace YoutubeExplode.Playlists
{
    /// <summary>
    /// Metadata associated with a YouTube playlist.
    /// </summary>
    public class Playlist
    {
        /// <summary>
        /// Playlist ID.
        /// </summary>
        public PlaylistId Id { get; }

        /// <summary>
        /// Playlist URL.
        /// </summary>
        public string Url => $"https://www.youtube.com/playlist?list={Id}";

        /// <summary>
        /// Playlist title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Playlist author.
        /// Can be null if it's a system playlist (e.g. Video Mix, Topics, etc.).
        /// </summary>
        public string? Author { get; }

        /// <summary>
        /// Playlist description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Playlist view count.
        /// </summary>
        public long ViewCount { get; }

        /// <summary>
        /// Available thumbnails for the playlist.
        /// May be empty if the playlist is empty.
        /// </summary>
        public IReadOnlyList<Thumbnail> Thumbnails { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Playlist"/>.
        /// </summary>
        public Playlist(
            PlaylistId id,
            string title,
            string? author,
            string description,
            long viewCount,
            IReadOnlyList<Thumbnail> thumbnails)
        {
            Id = id;
            Title = title;
            Author = author;
            Description = description;
            ViewCount = viewCount;
            Thumbnails = thumbnails;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => $"Playlist ({Title})";
    }
}
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

        /// <remarks>
        /// May be null in case of a system playlist.
        /// System playlist (e.g. mixes, topics, etc) are generated automatically and don't have an author.
        /// </remarks>
        public Author? Author { get; }

        /// <summary>
        /// Playlist description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Playlist thumbnails.
        /// </summary>
        public IReadOnlyList<Thumbnail> Thumbnails { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Playlist"/>.
        /// </summary>
        public Playlist(
            PlaylistId id,
            string title,
            Author? author,
            string description,
            IReadOnlyList<Thumbnail> thumbnails)
        {
            Id = id;
            Title = title;
            Author = author;
            Description = description;
            Thumbnails = thumbnails;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString() => $"Playlist ({Title})";
    }
}
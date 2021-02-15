using YoutubeExplode.Common;

namespace YoutubeExplode.Playlists
{
    /// <summary>
    /// YouTube playlist metadata.
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
        /// View count.
        /// </summary>
        public long ViewCount { get; }

        /// <summary>
        /// Available thumbnails for this playlist.
        /// Can be null if the playlist is empty.
        /// </summary>
        public ThumbnailSet? Thumbnails { get; }


        /// <summary>
        /// Initializes an instance of <see cref="Playlist"/>.
        /// </summary>
        public Playlist(PlaylistId id, string title, string? author, string description, long viewCount, ThumbnailSet? thumbnails)
        {
            Id = id;
            Title = title;
            Author = author;
            Description = description;
            ViewCount = viewCount;
            Thumbnails = thumbnails;
        }

        /// <inheritdoc />
        public override string ToString() => $"Playlist ({Title})";
    }
}
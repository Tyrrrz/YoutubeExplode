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
        /// Playlist author.
        /// Can be null if it's a system playlist (e.g. Video Mix, Topics, etc.).
        /// </summary>
        public string? Author { get; }

        /// <summary>
        /// Playlist title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Playlist description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Engagement statistics.
        /// </summary>
        public Engagement Engagement { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Playlist"/>.
        /// </summary>
        public Playlist(PlaylistId id, string? author, string title, string description, Engagement engagement)
        {
            Id = id;
            Author = author;
            Title = title;
            Description = description;
            Engagement = engagement;
        }

        /// <inheritdoc />
        public override string ToString() => $"Playlist ({Title})";
    }
}
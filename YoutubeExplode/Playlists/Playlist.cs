using YoutubeExplode.Common;

namespace YoutubeExplode.Playlists
{
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

        public string? Author { get; }

        public string Title { get; }

        public string Description { get; }

        public Engagement Engagement { get; }

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
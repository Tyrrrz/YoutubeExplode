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
            Type = PlaylistTypeParser.GetPlaylistType(id);
            Author = author;
            Title = title;
            Description = description;
            Statistics = statistics;
            Videos = videos;
        }

        /// <inheritdoc />
        public override string ToString() => Title;
    }
}
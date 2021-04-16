using System.Collections.Generic;
using YoutubeExplode.Common;

namespace YoutubeExplode.Playlists
{
    /// <summary>
    /// Metadata properties shared by playlists resolved from different sources.
    /// </summary>
    public interface IPlaylist
    {
        /// <summary>
        /// Playlist ID.
        /// </summary>
        PlaylistId Id { get; }

        /// <summary>
        /// Playlist URL.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Playlist title.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Playlist title.
        /// </summary>
        /// <remarks>
        /// May be null in case of system playlists (e.g. mixes, topics, etc).
        /// </remarks>
        Author? Author { get; }

        /// <summary>
        /// Playlist thumbnails.
        /// </summary>
        IReadOnlyList<Thumbnail> Thumbnails { get; }
    }
}
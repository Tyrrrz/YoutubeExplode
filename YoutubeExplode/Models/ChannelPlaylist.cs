using System;
using System.Collections.Generic;
using System.Text;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// A channel playlist is a playlist associated to a specific channel and can be loaded continuously
    /// </summary>
    public class ChannelPlaylist
    {
        /// <summary>
        /// ID of this playlist.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Type of this playlist.
        /// </summary>
        public PlaylistType Type => PlaylistTypeParser.GetPlaylistType(Id);

        /// <summary>
        /// Author of this playlist.
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// Title of this playlist.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// The thumbnail used on youtube.
        /// </summary>
        public ThumbnailSet ThumbnailSet { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Playlist"/>.
        /// </summary>
        public ChannelPlaylist(string id, string author, string title, string thumbnailVideoId)
        {
            Id = id;
            Author = author;
            Title = title;
            ThumbnailSet = new ThumbnailSet(thumbnailVideoId);
        }
    }
}

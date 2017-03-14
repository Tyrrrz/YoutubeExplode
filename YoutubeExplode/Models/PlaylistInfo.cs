using System.Collections.Generic;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Playlist metadata
    /// </summary>
    public class PlaylistInfo
    {
        /// <summary>
        /// IDs of the videos in this playlist
        /// </summary>
        public IReadOnlyList<string> VideoIds { get; internal set; }

        internal PlaylistInfo()
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Videos: {VideoIds.Count}";
        }
    }
}
namespace YoutubeExplode.Models
{
    /// <summary>
    /// Defines possible playlist types
    /// </summary>
    public enum PlaylistType
    {
        /// <summary>
        /// Playlist type could not be identified
        /// </summary>
        Unknown,

        /// <summary>
        /// Playlist created by a user
        /// </summary>
        UserMade,

        /// <summary>
        /// Mix playlist generated for a video
        /// </summary>
        Mix,

        /// <summary>
        /// Liked videos system playlist
        /// </summary>
        Liked,

        /// <summary>
        /// Favorites system playlist
        /// </summary>
        Favorites,

        /// <summary>
        /// Watch later system playlist
        /// </summary>
        WatchLater
    }
}

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
        /// Regular playlist created by a user
        /// </summary>
        UserMade,

        /// <summary>
        /// Mix playlist generated to group similar videos
        /// </summary>
        VideoMix,

        /// <summary>
        /// Mix playlist generated based on channel's uploads
        /// </summary>
        ChannelMix,

        /// <summary>
        /// System playlist for videos liked by a user
        /// </summary>
        Liked,

        /// <summary>
        /// System playlist for videos favorited by a user
        /// </summary>
        Favorites,

        /// <summary>
        /// System playlist for videos user added to watch later
        /// </summary>
        WatchLater
    }
}

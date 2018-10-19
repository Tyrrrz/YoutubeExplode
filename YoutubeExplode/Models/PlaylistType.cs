namespace YoutubeExplode.Models
{
    /// <summary>
    /// Playlist type.
    /// </summary>
    public enum PlaylistType
    {
        /// <summary>
        /// Regular playlist created by a user.
        /// </summary>
        Normal,

        /// <summary>
        /// Mix playlist generated to group similar videos.
        /// </summary>
        VideoMix,

        /// <summary>
        /// Mix playlist generated to group similar videos uploaded by the same channel.
        /// </summary>
        ChannelVideoMix,

        /// <summary>
        /// Playlist generated from channel uploads.
        /// </summary>
        ChannelVideos,

        /// <summary>
        /// Playlist generated from popular channel uploads.
        /// </summary>
        PopularChannelVideos,

        /// <summary>
        /// Playlist generated from automated music videos.
        /// </summary>
        MusicAlbum,

        /// <summary>
        /// System playlist for videos liked by a user.
        /// </summary>
        LikedVideos,

        /// <summary>
        /// System playlist for videos favorited by a user.
        /// </summary>
        Favorites,

        /// <summary>
        /// System playlist for videos user added to watch later.
        /// </summary>
        WatchLater
    }
}
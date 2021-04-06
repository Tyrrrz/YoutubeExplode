using YoutubeExplode.Playlists;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Exception thrown when the requested playlist is unavailable.
    /// </summary>
    public partial class PlaylistUnavailableException : YoutubeExplodeException
    {
        /// <summary>
        /// Initializes an instance of <see cref="PlaylistUnavailableException"/>.
        /// </summary>
        public PlaylistUnavailableException(string message)
            : base(message)
        {
        }
    }

    public partial class PlaylistUnavailableException
    {
        internal static PlaylistUnavailableException Unavailable(PlaylistId playlistId)
        {
            var message = $@"
Playlist '{playlistId}' is unavailable.
In most cases, this error indicates that the playlist doesn't exist, is private, or has been taken down.
If you can however open this video in your browser in incognito mode, it most likely means that YouTube changed something, which broke this library.
Please report this issue on GitHub in that case.";

            return new PlaylistUnavailableException(message.Trim());
        }
    }
}
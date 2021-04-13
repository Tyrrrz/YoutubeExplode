using System.Collections.Generic;

namespace YoutubeExplode.Playlists
{
    /// <summary>
    /// Batch of videos included in a playlist.
    /// </summary>
    public class PlaylistVideoBatch
    {
        /// <summary>
        /// Videos in the batch.
        /// </summary>
        public IReadOnlyList<PlaylistVideo> Videos { get; }

        /// <summary>
        /// Initializes an instance of <see cref="PlaylistVideoBatch"/>.
        /// </summary>
        public PlaylistVideoBatch(IReadOnlyList<PlaylistVideo> videos) =>
            Videos = videos;
    }
}
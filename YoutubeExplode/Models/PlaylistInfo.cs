namespace YoutubeExplode.Models
{
    /// <summary>
    /// Youtube playlist meta data
    /// </summary>
    public class PlaylistInfo
    {
        /// <summary>
        /// IDs of the videos in this playlist
        /// </summary>
        public string[] VideoIds { get; internal set; }

        internal PlaylistInfo()
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Videos: {VideoIds.Length}";
        }
    }
}
namespace YoutubeExplode.Models
{
    /// <summary>
    /// Video extended metadata
    /// </summary>
    public class ExtendedVideoInfo
    {
        /// <summary>
        /// Video author metadata
        /// </summary>
        public UserInfo Author { get; internal set; }

        /// <summary>
        /// Video description
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Like count for this video
        /// </summary>
        public long LikeCount { get; internal set; }

        /// <summary>
        /// Dislike count for this video
        /// </summary>
        public long DisikeCount { get; internal set; }

        internal ExtendedVideoInfo()
        {
        }
    }
}
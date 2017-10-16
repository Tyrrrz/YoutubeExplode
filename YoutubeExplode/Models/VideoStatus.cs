namespace YoutubeExplode.Models
{
    /// <summary>
    /// Video status
    /// </summary>
    public class VideoStatus
    {
        /// <summary>
        /// Whether this video is publicly listed
        /// </summary>
        public bool IsListed { get; }

        /// <summary>
        /// Whether liking/disliking this video is allowed
        /// </summary>
        public bool IsRatingAllowed { get; }

        /// <summary>
        /// Whether the audio track has been muted
        /// </summary>
        public bool IsMuted { get; }

        /// <summary>
        /// Whether embedding this video on other websites is allowed
        /// </summary>
        public bool IsEmbeddingAllowed { get; }

        /// <summary />
        public VideoStatus(bool isListed, bool isRatingAllowed, bool isMuted, bool isEmbeddingAllowed)
        {
            IsListed = isListed;
            IsRatingAllowed = isRatingAllowed;
            IsMuted = isMuted;
            IsEmbeddingAllowed = isEmbeddingAllowed;
        }
    }
}
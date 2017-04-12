namespace YoutubeExplode.Models
{
    /// <summary>
    /// Defines possible media stream content types
    /// </summary>
    public enum MediaStreamContentType
    {
        /// <summary>
        /// Content type could not be identified
        /// </summary>
        Unknown,

        /// <summary>
        /// Contains both video and audio in a single stream
        /// </summary>
        Mixed,

        /// <summary>
        /// Contains only video
        /// </summary>
        Video,

        /// <summary>
        /// Contains only audio
        /// </summary>
        Audio
    }
}
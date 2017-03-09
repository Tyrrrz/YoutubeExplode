namespace YoutubeExplode.Models
{
    /// <summary>
    /// Defines possible media adaptive modes
    /// </summary>
    public enum AdaptiveMode
    {
        /// <summary>
        /// Non-adaptive (mixed video and audio)
        /// </summary>
        None,

        /// <summary>
        /// Only contains video
        /// </summary>
        Video,

        /// <summary>
        /// Only contains audio
        /// </summary>
        Audio
    }
}
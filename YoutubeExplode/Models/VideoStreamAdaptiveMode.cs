namespace YoutubeExplode.Models
{
    /// <summary>
    /// Defines possible video adaptive modes
    /// </summary>
    public enum VideoStreamAdaptiveMode
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
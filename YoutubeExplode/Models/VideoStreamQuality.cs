namespace YoutubeExplode.Models
{
    /// <summary>
    /// Defines possible stream quality values
    /// </summary>
    public enum VideoStreamQuality
    {
        /// <summary>
        /// Video quality could not be identified
        /// </summary>
        Unknown,

        /// <summary>
        /// 144p low-quality video stream
        /// </summary>
        Low144,

        /// <summary>
        /// 240p low-quality video stream
        /// </summary>
        Low240,

        /// <summary>
        /// 360p medium-quality video stream
        /// </summary>
        Medium360,

        /// <summary>
        /// 480p medium-quality video stream
        /// </summary>
        Medium480,

        /// <summary>
        /// 720p HD video stream
        /// </summary>
        High720,

        /// <summary>
        /// 1080p HD video stream
        /// </summary>
        High1080,

        /// <summary>
        /// 1440p HD video stream
        /// </summary>
        High1440,

        /// <summary>
        /// 2160p HD video stream
        /// </summary>
        High2160,

        /// <summary>
        /// 3072p HD video stream
        /// </summary>
        High3072
    }
}
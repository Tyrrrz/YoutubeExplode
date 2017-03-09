// ReSharper disable InconsistentNaming (File extensions)

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Defines possible container types
    /// </summary>
    public enum ContainerType
    {
        /// <summary>
        /// Container type could not be identified
        /// </summary>
        Unknown,

        /// <summary>
        /// MPEG-4 Part 14 (.mp4)
        /// </summary>
        MP4,

        /// <summary>
        /// MPEG-4 Part 14 audio-only (.m4a)
        /// </summary>
        M4A,

        /// <summary>
        /// WebM (.webm)
        /// </summary>
        WebM,

        /// <summary>
        /// 3rd Generation Partnership Project (.3gpp)
        /// </summary>
        TGPP,

        /// <summary>
        /// Flash (.flv)
        /// </summary>
        FLV,

        /// <summary>
        /// Transport stream (.ts)
        /// </summary>
        TS
    }
}
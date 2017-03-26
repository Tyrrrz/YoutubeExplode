// ReSharper disable InconsistentNaming (file types)

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Defines possible media stream container types
    /// </summary>
    public enum MediaStreamContainerType
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
        /// Web Media (.webm)
        /// </summary>
        WebM,

        /// <summary>
        /// 3rd Generation Partnership Project (.3gpp)
        /// </summary>
        TGPP,

        /// <summary>
        /// Flash Video (.flv)
        /// </summary>
        FLV,

        /// <summary>
        /// Transport Stream (.ts)
        /// </summary>
        TS
    }
}
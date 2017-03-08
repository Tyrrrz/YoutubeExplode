// ReSharper disable InconsistentNaming (File extensions)

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Defines possible video stream container types
    /// </summary>
    public enum VideoStreamType
    {
        /// <summary>
        /// Video type could not be identified
        /// </summary>
        Unknown,

        /// <summary>
        /// MPEG-4 Part 14 (.mp4) video stream
        /// </summary>
        MP4,

        /// <summary>
        /// MPEG-4 Part 14 audio-only (.m4a) video stream
        /// </summary>
        M4A,

        /// <summary>
        /// WebM (.webm) video stream
        /// </summary>
        WebM,

        /// <summary>
        /// 3rd Generation Partnership Project (.3gpp) video stream
        /// </summary>
        TGPP,

        /// <summary>
        /// Flash (.flv) video stream
        /// </summary>
        FLV,

        /// <summary>
        /// Transport stream (.ts) video stream
        /// </summary>
        TS
    }
}
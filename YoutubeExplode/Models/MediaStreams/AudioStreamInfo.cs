using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Audio stream info
    /// </summary>
    public class AudioStreamInfo : MediaStreamInfo
    {
        /// <summary>
        /// Bitrate (bit/s)
        /// </summary>
        public long Bitrate { get; }

        /// <summary>
        /// Audio encoding
        /// </summary>
        public AudioEncoding AudioEncoding { get; }

        /// <summary />
        public AudioStreamInfo(int itag, string url, long size, long bitrate)
            : base(itag, url, size)
        {
            Bitrate = bitrate.GuardNotNegative(nameof(bitrate));
            AudioEncoding = GetAudioEncoding(itag);
        }
    }
}
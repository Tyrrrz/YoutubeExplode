using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Metadata associated with a certain <see cref="MediaStream"/> that contains only audio.
    /// </summary>
    public class AudioStreamInfo : MediaStreamInfo
    {
        /// <summary>
        /// Bitrate (bit/s) of the associated stream.
        /// </summary>
        public long Bitrate { get; }

        /// <summary>
        /// Audio encoding of the associated stream.
        /// </summary>
        public AudioEncoding AudioEncoding { get; }

        /// <summary />
        public AudioStreamInfo(int itag, string url, long size, long bitrate)
            : base(itag, url, size)
        {
            Bitrate = bitrate.GuardNotNegative(nameof(bitrate));
            AudioEncoding = ItagHelper.GetAudioEncoding(itag);
        }
    }
}
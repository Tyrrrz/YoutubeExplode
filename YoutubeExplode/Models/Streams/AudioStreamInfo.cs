namespace YoutubeExplode.Models.Streams
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

        /// <inheritdoc />
        public AudioStreamInfo(int itag, string url, long contentLength, long bitrate)
            : base(itag, url, contentLength)
        {
            Bitrate = bitrate;
            AudioEncoding = GetAudioEncoding(itag);
        }
    }
}
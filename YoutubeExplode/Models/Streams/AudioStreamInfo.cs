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
        public AudioEncoding Encoding { get; }

        /// <inheritdoc />
        public AudioStreamInfo(int itag, string url, long bitrate)
            : base(itag, url)
        {
            Bitrate = bitrate;
            Encoding = GetAudioEncoding(itag);
        }
    }
}
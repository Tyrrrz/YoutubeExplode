namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Metadata associated with a certain <see cref="MediaStream"/> that contains only audio.
    /// </summary>
    public class AudioStreamInfo : MediaStreamInfo, IHasAudio
    {
        /// <inheritdoc />
        public AudioEncoding AudioEncoding { get; }

        /// <summary>
        /// Initializes an instance of <see cref="AudioStreamInfo"/>.
        /// </summary>
        public AudioStreamInfo(int itag, string url, Container container, long size, long bitrate,
            AudioEncoding audioEncoding)
            : base(itag, url, container, size, bitrate)
        {
            AudioEncoding = audioEncoding;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Itag} ({Container}) [audio]";
    }
}
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
        public AudioStreamInfo(string url, long size, long bitrate, Container container, AudioEncoding audioEncoding) 
            : base(url, size, bitrate, container)
        {
            AudioEncoding = audioEncoding;
        }
    }
}
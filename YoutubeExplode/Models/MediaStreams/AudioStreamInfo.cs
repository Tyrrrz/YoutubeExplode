using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Metadata associated with a certain <see cref="MediaStream"/> that contains only audio.
    /// </summary>
    public class AudioStreamInfo : MediaStreamInfo, IHasAudio
    {
        /// <inheritdoc />
        public string AudioEncoding { get; }

        /// <summary>
        /// Initializes an instance of <see cref="AudioStreamInfo"/>.
        /// </summary>
        public AudioStreamInfo(string url, long contentLength, long bitrate, string format, string audioEncoding) 
            : base(url, contentLength, bitrate, format)
        {
            AudioEncoding = audioEncoding.GuardNotNull(nameof(audioEncoding));
        }
    }
}
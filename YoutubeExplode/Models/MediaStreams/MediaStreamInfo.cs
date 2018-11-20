using JetBrains.Annotations;
using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.MediaStreams
{
    /// <summary>
    /// Metadata associated with a certain <see cref="MediaStream"/>.
    /// </summary>
    public abstract class MediaStreamInfo
    {
        /// <summary>
        /// URL of the endpoint that serves the associated stream.
        /// </summary>
        [NotNull]
        public string Url { get; }

        /// <summary>
        /// Content length (bytes) of the associated stream.
        /// </summary>
        public long ContentLength { get; }

        /// <summary>
        /// Video bitrate (bits/s) of the associated stream.
        /// </summary>
        public long Bitrate { get; }

        /// <summary>
        /// Format of the associated stream.
        /// </summary>
        [NotNull]
        public string Format { get; }

        /// <summary>
        /// Initializes an instance of <see cref="MediaStreamInfo"/>.
        /// </summary>
        protected MediaStreamInfo(string url, long contentLength, long bitrate, string format)
        {
            Url = url.GuardNotNull(nameof(url));
            ContentLength = contentLength.GuardPositive(nameof(contentLength));
            Bitrate = bitrate.GuardPositive(nameof(bitrate));
            Format = format.GuardNotNull(nameof(format));
        }

        /// <inheritdoc />
        public override string ToString() => Format;
    }
}
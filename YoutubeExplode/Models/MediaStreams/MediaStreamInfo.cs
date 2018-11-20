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
        public long Size { get; }

        /// <summary>
        /// Video bitrate (bits/s) of the associated stream.
        /// </summary>
        public long Bitrate { get; }

        /// <summary>
        /// Container of the associated stream.
        /// </summary>
        public Container Container { get; }

        /// <summary>
        /// Initializes an instance of <see cref="MediaStreamInfo"/>.
        /// </summary>
        protected MediaStreamInfo(string url, long size, long bitrate, Container container)
        {
            Url = url.GuardNotNull(nameof(url));
            Size = size.GuardPositive(nameof(size));
            Bitrate = bitrate.GuardPositive(nameof(bitrate));
            Container = container;
        }

        /// <inheritdoc />
        public override string ToString() => Container.ToString();
    }
}
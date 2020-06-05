using System;

namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// Encapsulates video resolution.
    /// </summary>
    public readonly partial struct VideoResolution
    {
        /// <summary>
        /// Viewport width.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Viewport height.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Initializes an instance of <see cref="VideoResolution"/>.
        /// </summary>
        public VideoResolution(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Width}x{Height}";
    }

    public partial struct VideoResolution : IEquatable<VideoResolution>
    {
        /// <inheritdoc />
        public bool Equals(VideoResolution other) => Width == other.Width && Height == other.Height;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is VideoResolution other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Width, Height);

        /// <summary>
        /// Equality check.
        /// </summary>
        public static bool operator ==(VideoResolution left, VideoResolution right) => left.Equals(right);

        /// <summary>
        /// Equality check.
        /// </summary>
        public static bool operator !=(VideoResolution left, VideoResolution right) => !(left == right);
    }
}
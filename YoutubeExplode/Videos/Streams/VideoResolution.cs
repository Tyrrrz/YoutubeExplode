using System;

namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// Video stream resolution.
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

    public partial struct VideoResolution
    {
        internal static VideoResolution FromVideoQuality(VideoQuality quality) => quality.MaxHeight switch
        {
            144 => new VideoResolution(256, 144),
            240 => new VideoResolution(426, 240),
            360 => new VideoResolution(640, 360),
            480 => new VideoResolution(854, 480),
            720 => new VideoResolution(1280, 720),
            1080 => new VideoResolution(1920, 1080),
            1440 => new VideoResolution(2560, 1440),
            2160 => new VideoResolution(3840, 2160),
            2880 => new VideoResolution(5120, 2880),
            3072 => new VideoResolution(4096, 3072),
            4320 => new VideoResolution(7680, 4320),
            _ => new VideoResolution(16 * quality.MaxHeight / 9, quality.MaxHeight)
        };
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
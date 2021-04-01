using System;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// Video stream quality.
    /// </summary>
    public readonly partial struct VideoQuality
    {
        /// <summary>
        /// Maximum height of the video.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Initializes an instance of <see cref="VideoQuality"/>.
        /// </summary>
        public VideoQuality(int height) => Height = height;

        internal string FormatLabel() => $"{Height}p";

        internal string FormatLabel(Framerate framerate)
        {
            // Framerate appears only if it's above 30
            if (framerate.FramesPerSecond <= 30)
                return FormatLabel();

            // YouTube rounds framerate to nearest next decimal
            var framerateRounded = (int) Math.Ceiling(framerate.FramesPerSecond / 10.0) * 10;
            return FormatLabel() + framerateRounded;
        }

        internal VideoResolution GetDefaultResolution() => Height switch
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
            var height => new VideoResolution(16 * height / 9, height)
        };

        /// <inheritdoc />
        public override string ToString() => FormatLabel();
    }

    public partial struct VideoQuality
    {
        internal static VideoQuality FromLabel(string label) => new(label.StripNonDigit().ParseInt());

        internal static VideoQuality FromTag(int tag) => tag switch
        {
            5 => Low144,
            6 => Low240,
            13 => Low144,
            17 => Low144,
            18 => Medium360,
            22 => High720,
            34 => Medium360,
            35 => Medium480,
            36 => Low240,
            37 => High1080,
            38 => High3072,
            43 => Medium360,
            44 => Medium480,
            45 => High720,
            46 => High1080,
            59 => Medium480,
            78 => Medium480,
            82 => Medium360,
            83 => Medium480,
            84 => High720,
            85 => High1080,
            91 => Low144,
            92 => Low240,
            93 => Medium360,
            94 => Medium480,
            95 => High720,
            96 => High1080,
            100 => Medium360,
            101 => Medium480,
            102 => High720,
            132 => Low240,
            151 => Low144,
            133 => Low240,
            134 => Medium360,
            135 => Medium480,
            136 => High720,
            137 => High1080,
            138 => High4320,
            160 => Low144,
            212 => Medium480,
            213 => Medium480,
            214 => High720,
            215 => High720,
            216 => High1080,
            217 => High1080,
            264 => High1440,
            266 => High2160,
            298 => High720,
            299 => High1080,
            399 => High1080,
            398 => High720,
            397 => Medium480,
            396 => Medium360,
            395 => Low240,
            394 => Low144,
            167 => Medium360,
            168 => Medium480,
            169 => High720,
            170 => High1080,
            218 => Medium480,
            219 => Medium480,
            242 => Low240,
            243 => Medium360,
            244 => Medium480,
            245 => Medium480,
            246 => Medium480,
            247 => High720,
            248 => High1080,
            271 => High1440,
            272 => High2160,
            278 => Low144,
            302 => High720,
            303 => High1080,
            308 => High1440,
            313 => High2160,
            315 => High2160,
            330 => Low144,
            331 => Low240,
            332 => Medium360,
            333 => Medium480,
            334 => High720,
            335 => High1080,
            336 => High1440,
            337 => High2160,
            _ => throw new ArgumentException($"Unrecognized tag '{tag}'.", nameof(tag))
        };
    }

    public partial struct VideoQuality
    {
        /// <summary>
        /// Low quality (144p).
        /// </summary>
        public static VideoQuality Low144 { get; } = new(144);

        /// <summary>
        /// Low quality (240p).
        /// </summary>
        public static VideoQuality Low240 { get; } = new(240);

        /// <summary>
        /// Medium quality (360p).
        /// </summary>
        public static VideoQuality Medium360 { get; } = new(360);

        /// <summary>
        /// Medium quality (480p).
        /// </summary>
        public static VideoQuality Medium480 { get; } = new(480);

        /// <summary>
        /// High quality (720p).
        /// </summary>
        public static VideoQuality High720 { get; } = new(720);

        /// <summary>
        /// High quality (1080p).
        /// </summary>
        public static VideoQuality High1080 { get; } = new(1080);

        /// <summary>
        /// High quality (1440p).
        /// </summary>
        public static VideoQuality High1440 { get; } = new(1440);

        /// <summary>
        /// High quality (2160p).
        /// </summary>
        public static VideoQuality High2160 { get; } = new(2160);

        /// <summary>
        /// High quality (2880p).
        /// </summary>
        public static VideoQuality High2880 { get; } = new(2880);

        /// <summary>
        /// High quality (3072p).
        /// </summary>
        public static VideoQuality High3072 { get; } = new(3072);

        /// <summary>
        /// High quality (4320p).
        /// </summary>
        public static VideoQuality High4320 { get; } = new(4320);
    }

    public partial struct VideoQuality : IComparable<VideoQuality>, IEquatable<VideoQuality>
    {
        /// <inheritdoc />
        public int CompareTo(VideoQuality other) => Height.CompareTo(other.Height);

        /// <inheritdoc />
        public bool Equals(VideoQuality other) => CompareTo(other) == 0;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is VideoQuality other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Height);

        /// <summary>
        /// Equality check.
        /// </summary>
        public static bool operator ==(VideoQuality left, VideoQuality right) => left.Equals(right);

        /// <summary>
        /// Equality check.
        /// </summary>
        public static bool operator !=(VideoQuality left, VideoQuality right) => !(left == right);

        /// <summary>
        /// Comparison.
        /// </summary>
        public static bool operator >(VideoQuality left, VideoQuality right) => left.CompareTo(right) > 0;

        /// <summary>
        /// Comparison.
        /// </summary>
        public static bool operator <(VideoQuality left, VideoQuality right) => left.CompareTo(right) < 0;
    }
}
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
        /// Quality label as seen on YouTube (e.g. 1080p or 720p60).
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Maximum height of the video.
        /// </summary>
        public int MaxHeight { get; }

        /// <summary>
        /// Video framerate.
        /// </summary>
        public int Framerate { get; }

        /// <summary>
        /// Whether this is a high definition video quality (i.e. 1080p or above).
        /// </summary>
        public bool IsHighDefinition => MaxHeight >= 1080;

        /// <summary>
        /// Initializes an instance of <see cref="VideoQuality"/>.
        /// </summary>
        public VideoQuality(string label, int maxHeight, int framerate)
        {
            Label = label;
            MaxHeight = maxHeight;
            Framerate = framerate;
        }

        /// <summary>
        /// Initializes an instance of <see cref="VideoQuality"/>.
        /// </summary>
        public VideoQuality(int maxHeight, int framerate)
            : this(FormatLabel(maxHeight, framerate), maxHeight, framerate)
        {
        }

        /// <inheritdoc />
        public override string ToString() => Label;
    }

    public partial struct VideoQuality
    {
        private static string FormatLabel(int maxHeight, int framerate)
        {
            // Framerate appears only if it's above 30
            if (framerate <= 30)
                return $"{maxHeight}p";

            // YouTube rounds framerate to the next nearest decimal
            var framerateRounded = (int) Math.Ceiling(framerate / 10.0) * 10;
            return $"{maxHeight}p{framerateRounded}";
        }

        internal static VideoQuality FromLabel(string label, int framerateFallback)
        {
            var maxHeight = label.SubstringUntil("p").ParseInt();
            var framerate = label.SubstringAfter("p").NullIfWhiteSpace()?.ParseInt();

            return new VideoQuality(
                label,
                maxHeight,
                framerate ?? framerateFallback
            );
        }

        internal static VideoQuality FromTag(int tag, int framerate)
        {
            var maxHeight = tag switch
            {
                5 => 144,
                6 => 240,
                13 => 144,
                17 => 144,
                18 => 360,
                22 => 720,
                34 => 360,
                35 => 480,
                36 => 240,
                37 => 1080,
                38 => 3072,
                43 => 360,
                44 => 480,
                45 => 720,
                46 => 1080,
                59 => 480,
                78 => 480,
                82 => 360,
                83 => 480,
                84 => 720,
                85 => 1080,
                91 => 144,
                92 => 240,
                93 => 360,
                94 => 480,
                95 => 720,
                96 => 1080,
                100 => 360,
                101 => 480,
                102 => 720,
                132 => 240,
                151 => 144,
                133 => 240,
                134 => 360,
                135 => 480,
                136 => 720,
                137 => 1080,
                138 => 4320,
                160 => 144,
                212 => 480,
                213 => 480,
                214 => 720,
                215 => 720,
                216 => 1080,
                217 => 1080,
                264 => 1440,
                266 => 2160,
                298 => 720,
                299 => 1080,
                399 => 1080,
                398 => 720,
                397 => 480,
                396 => 360,
                395 => 240,
                394 => 144,
                167 => 360,
                168 => 480,
                169 => 720,
                170 => 1080,
                218 => 480,
                219 => 480,
                242 => 240,
                243 => 360,
                244 => 480,
                245 => 480,
                246 => 480,
                247 => 720,
                248 => 1080,
                271 => 1440,
                272 => 2160,
                278 => 144,
                302 => 720,
                303 => 1080,
                308 => 1440,
                313 => 2160,
                315 => 2160,
                330 => 144,
                331 => 240,
                332 => 360,
                333 => 480,
                334 => 720,
                335 => 1080,
                336 => 1440,
                337 => 2160,
                _ => throw new ArgumentException($"Unrecognized tag '{tag}'.", nameof(tag))
            };

            return new VideoQuality(maxHeight, framerate);
        }
    }

    public partial struct VideoQuality : IComparable<VideoQuality>, IEquatable<VideoQuality>
    {
        /// <inheritdoc />
        public int CompareTo(VideoQuality other)
        {
            var maxHeightComparison = MaxHeight.CompareTo(other.MaxHeight);
            var framerateComparison = Framerate.CompareTo(other.Framerate);

            return maxHeightComparison != 0
                ? maxHeightComparison
                : framerateComparison;
        }

        /// <inheritdoc />
        public bool Equals(VideoQuality other) => CompareTo(other) == 0;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is VideoQuality other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(MaxHeight, Framerate);

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
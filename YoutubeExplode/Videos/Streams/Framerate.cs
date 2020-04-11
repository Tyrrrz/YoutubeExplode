using System;

namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// Encapsulates framerate.
    /// </summary>
    [Equals(DoNotAddEqualityOperators = true)]
    public readonly partial struct Framerate : IComparable<Framerate>
    {
        /// <summary>
        /// Framerate as frames per second
        /// </summary>
        public double FramesPerSecond { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Framerate"/>.
        /// </summary>
        public Framerate(double framesPerSecond) => FramesPerSecond = framesPerSecond;

        /// <inheritdoc />
        public int CompareTo(Framerate other) => FramesPerSecond.CompareTo(other.FramesPerSecond);

        /// <inheritdoc />
        public override string ToString() => $"{FramesPerSecond:N0} FPS";
    }

    public partial struct Framerate
    {
        /// <summary>
        /// Creates framerate as frames per second.
        /// </summary>
        public static Framerate FromFramesPerSecond(double framesPerSecond) => new Framerate(framesPerSecond);

        internal static Framerate FromFramesPerSecond(int framesPerSecond) => FromFramesPerSecond((double) framesPerSecond);
    }
}
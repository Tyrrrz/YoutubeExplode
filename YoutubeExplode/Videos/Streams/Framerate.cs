using System;

namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// Encapsulates framerate.
    /// </summary>
    [Equals(DoNotAddEqualityOperators = true)]
    public readonly struct Framerate : IComparable<Framerate>
    {
        /// <summary>
        /// Framerate as frames per second.
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
}
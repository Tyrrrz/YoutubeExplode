using System;

namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// Encapsulates framerate.
    /// </summary>
    public readonly partial struct Framerate
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
        public override string ToString() => $"{FramesPerSecond:N0} FPS";
    }

    public partial struct Framerate : IComparable<Framerate>, IEquatable<Framerate>
    {
        /// <inheritdoc />
        public int CompareTo(Framerate other) => FramesPerSecond.CompareTo(other.FramesPerSecond);

        /// <inheritdoc />
        public bool Equals(Framerate other) => CompareTo(other) == 0;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Framerate other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(FramesPerSecond);

        /// <summary>
        /// Equality check.
        /// </summary>
        public static bool operator ==(Framerate left, Framerate right) => left.Equals(right);

        /// <summary>
        /// Equality check.
        /// </summary>
        public static bool operator !=(Framerate left, Framerate right) => !(left == right);

        /// <summary>
        /// Comparison.
        /// </summary>
        public static bool operator >(Framerate left, Framerate right) => left.CompareTo(right) > 0;

        /// <summary>
        /// Comparison.
        /// </summary>
        public static bool operator <(Framerate left, Framerate right) => left.CompareTo(right) < 0;
    }
}
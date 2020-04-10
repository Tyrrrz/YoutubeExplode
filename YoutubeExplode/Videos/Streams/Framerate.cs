using System;

namespace YoutubeExplode.Videos.Streams
{
    [Equals(DoNotAddEqualityOperators = true)]
    public readonly partial struct Framerate : IComparable<Framerate>
    {
        public double FramesPerSecond { get; }

        public Framerate(double framesPerSecond)
        {
            FramesPerSecond = framesPerSecond;
        }

        public override string ToString() => $"{FramesPerSecond:N0} FPS";

        public int CompareTo(Framerate other) => FramesPerSecond.CompareTo(other.FramesPerSecond);
    }

    public partial struct Framerate
    {
        public static Framerate FromFramesPerSecond(double framesPerSecond) => new Framerate(framesPerSecond);

        public static Framerate FromFramesPerSecond(int framesPerSecond) => FromFramesPerSecond((double) framesPerSecond);
    }
}
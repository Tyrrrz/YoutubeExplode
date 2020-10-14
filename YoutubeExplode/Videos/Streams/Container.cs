using System;

namespace YoutubeExplode.Videos.Streams
{
    /// <summary>
    /// Stream container.
    /// </summary>
    public readonly partial struct Container
    {
        /// <summary>
        /// Container name.
        /// Can be used as file extension.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Container"/>.
        /// </summary>
        public Container(string name) => Name = name;

        /// <inheritdoc />
        public override string ToString() => Name;
    }

    public partial struct Container
    {
        /// <summary>
        /// MPEG-4 Part 14 (.mp4).
        /// </summary>
        public static Container Mp4 { get; }  = new Container("mp4");

        /// <summary>
        /// Web Media (.webm).
        /// </summary>
        public static Container WebM { get; }  = new Container("webm");

        /// <summary>
        /// 3rd Generation Partnership Project (.3gpp).
        /// </summary>
        public static Container Tgpp { get; }  = new Container("3gpp");

        /// <summary>
        /// Parse a container from name.
        /// </summary>
        public static Container Parse(string name)
        {
            if (name.Equals("mp4", StringComparison.OrdinalIgnoreCase))
                return Mp4;

            if (name.Equals("webm", StringComparison.OrdinalIgnoreCase))
                return WebM;

            if (name.Equals("3gpp", StringComparison.OrdinalIgnoreCase))
                return Tgpp;

            throw new ArgumentException($"Unrecognized container '{name}'.", nameof(name));
        }
    }

    public partial struct Container : IEquatable<Container>
    {
        /// <inheritdoc />
        public bool Equals(Container other) => StringComparer.OrdinalIgnoreCase.Equals(Name, other.Name);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Container other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Name);

        /// <summary>
        /// Equality check.
        /// </summary>
        public static bool operator ==(Container left, Container right) => left.Equals(right);

        /// <summary>
        /// Equality check.
        /// </summary>
        public static bool operator !=(Container left, Container right) => !(left == right);
    }
}
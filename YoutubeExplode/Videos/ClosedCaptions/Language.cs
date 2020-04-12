namespace YoutubeExplode.Videos.ClosedCaptions
{
    /// <summary>
    /// Encapsulates a human written language.
    /// </summary>
    [Equals(DoNotAddEqualityOperators = true)]
    public readonly struct Language
    {
        /// <summary>
        /// ISO 639-1 code of this language.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Full English name of this language.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes an instance of <see cref="Language"/>.
        /// </summary>
        public Language(string code, string name)
        {
            Code = code;
            Name = name;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Code} ({Name})";
    }
}
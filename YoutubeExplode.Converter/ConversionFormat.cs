using System;
using System.Collections.Generic;

namespace YoutubeExplode.Converter
{
    /// <summary>
    /// Encapsulates conversion media format.
    /// </summary>
    public readonly partial struct ConversionFormat
    {
        /// <summary>
        /// Format name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Whether this format is a known audio-only format.
        /// </summary>
        public bool IsAudioOnly => AudioOnlyFormats.Contains(Name);

        /// <summary>
        /// Initializes an instance of <see cref="ConversionFormat"/>.
        /// </summary>
        public ConversionFormat(string name) => Name = name;

        /// <inheritdoc />
        public override string ToString() => Name;
    }

    public partial struct ConversionFormat
    {
        private static readonly HashSet<string> AudioOnlyFormats = new(StringComparer.OrdinalIgnoreCase)
            {"mp3", "m4a", "wav", "wma", "ogg", "aac", "opus"};
    }
}
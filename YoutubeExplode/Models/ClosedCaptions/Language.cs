using System;

namespace YoutubeExplode.Models.ClosedCaptions
{
    /// <summary>
    /// Language information
    /// </summary>
    public class Language
    {
        /// <summary>
        /// The ISO 639-1 Code of this language
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// The full english name of this language
        /// </summary>
        public string Name { get; }

        /// <inheritdoc />
        public Language(string code, string name)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
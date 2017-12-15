﻿using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.ClosedCaptions
{
    /// <summary>
    /// Language information.
    /// </summary>
    public class Language
    {
        /// <summary>
        /// ISO 639-1 code of this language.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Full English name of this language.
        /// </summary>
        public string Name { get; }

        /// <summary />
        public Language(string code, string name)
        {
            Code = code.GuardNotNull(nameof(code));
            Name = name.GuardNotNull(nameof(name));
        }

        /// <inheritdoc />
        public override string ToString() => $"{Code} ({Name})";
    }
}
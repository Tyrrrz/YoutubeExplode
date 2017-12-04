﻿using YoutubeExplode.Internal;

namespace YoutubeExplode.Models
{
    /// <summary>
    /// Channel
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// URL
        /// </summary>
        public string Url => $"https://www.youtube.com/channel/{Id}";

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Logo image URL
        /// </summary>
        public string LogoUrl { get; }

        /// <summary />
        public Channel(string id, string title, string logoUrl)
        {
            Id = id.GuardNotNull(nameof(id));
            Title = title.GuardNotNull(nameof(title));
            LogoUrl = logoUrl.GuardNotNull(nameof(logoUrl));
        }

        /// <inheritdoc />
        public override string ToString() => Title;
    }
}
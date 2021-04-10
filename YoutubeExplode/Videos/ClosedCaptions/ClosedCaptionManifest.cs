using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Videos.ClosedCaptions
{
    /// <summary>
    /// Contains information about available closed caption tracks for a specific video.
    /// </summary>
    public class ClosedCaptionManifest
    {
        /// <summary>
        /// Available closed caption tracks.
        /// </summary>
        public IReadOnlyList<ClosedCaptionTrackInfo> Tracks { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ClosedCaptionManifest"/>.
        /// </summary>
        public ClosedCaptionManifest(IReadOnlyList<ClosedCaptionTrackInfo> tracks)
        {
            Tracks = tracks;
        }

        /// <summary>
        /// Gets the closed caption track in the specified language.
        /// Returns null if not found.
        /// </summary>
        public ClosedCaptionTrackInfo? TryGetByLanguage(string language) =>
            Tracks.FirstOrDefault(t =>
                string.Equals(t.Language.Code, language, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.Language.Name, language, StringComparison.OrdinalIgnoreCase)
            );

        /// <summary>
        /// Gets the closed caption track in the specified language.
        /// </summary>
        public ClosedCaptionTrackInfo GetByLanguage(string language) =>
            TryGetByLanguage(language) ??
            throw new InvalidOperationException($"No closed caption track available for language '{language}'.");
    }
}
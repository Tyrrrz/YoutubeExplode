using YoutubeExplode.Internal;

namespace YoutubeExplode.Models.ClosedCaptions
{
    /// <summary>
    /// Closed caption track info
    /// </summary>
    public class ClosedCaptionTrackInfo
    {
        /// <summary>
        /// Manifest URL
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Text language
        /// </summary>
        public Language Language { get; }

        /// <summary />
        public ClosedCaptionTrackInfo(string url, Language language)
        {
            Url = url.GuardNotNull(nameof(url));
            Language = language.GuardNotNull(nameof(language));
        }

        /// <inheritdoc />
        public override string ToString() => $"{Language}";
    }
}
namespace YoutubeExplode.Models
{
    /// <summary>
    /// Dash manifest meta data
    /// </summary>
    public class VideoDashManifestInfo
    {
        /// <summary>
        /// URL for the dash manifest
        /// </summary>
        public string Url { get; internal set; }

        /// <summary>
        /// Authorization signature
        /// </summary>
        internal string Signature { get; set; }

        /// <summary>
        /// Whether the signature needs to be deciphered before manifest can be accessed by URL
        /// </summary>
        internal bool NeedsDeciphering { get; set; }

        internal VideoDashManifestInfo()
        {
        }
    }
}
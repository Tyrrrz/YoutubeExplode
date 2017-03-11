namespace YoutubeExplode.Models
{
    /// <summary>
    /// Dash manifest metadata
    /// </summary>
    public class DashManifestInfo
    {
        /// <summary>
        /// URL for this dash manifest
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

        internal DashManifestInfo()
        {
        }
    }
}
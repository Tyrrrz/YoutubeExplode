namespace YoutubeExplode.Models
{
    /// <summary>
    /// Dash manifest metadata
    /// </summary>
    public class DashManifestInfo
    {
        /// <summary>
        /// Manifest URL
        /// </summary>
        public string Url { get; internal set; }

        /// <summary>
        /// Authorization signature
        /// </summary>
        internal string Signature { get; set; }

        /// <summary>
        /// Whether the signature needs to be deciphered
        /// </summary>
        internal bool NeedsDeciphering { get; set; }

        internal DashManifestInfo()
        {
        }
    }
}
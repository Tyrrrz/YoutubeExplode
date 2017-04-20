namespace YoutubeExplode.Models
{
    /// <summary>
    /// Dash manifest info
    /// </summary>
    public class DashManifestInfo
    {
        /// <summary>
        /// Manifest URL
        /// </summary>
        public string Url { get; }

        /// <inheritdoc />
        public DashManifestInfo(string url)
        {
            Url = url;
        }
    }
}
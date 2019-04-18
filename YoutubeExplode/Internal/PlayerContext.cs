namespace YoutubeExplode.Internal
{
    internal class PlayerContext
    {
        public string SourceUrl { get; }

        public string Sts { get; }

        public PlayerContext(string sourceUrl, string sts)
        {
            SourceUrl = sourceUrl;
            Sts = sts;
        }
    }
}
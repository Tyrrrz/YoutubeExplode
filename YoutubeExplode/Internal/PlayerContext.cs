namespace YoutubeExplode.Internal
{
    internal class PlayerContext
    {
        public string Version { get; }

        public string Sts { get; }

        public PlayerContext(string version, string sts)
        {
            Version = version;
            Sts = sts;
        }
    }
}
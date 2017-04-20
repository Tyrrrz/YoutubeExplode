namespace YoutubeExplode.Internal
{
    internal class VideoContext
    {
        public string PlayerVersion { get; }

        public string Sts { get; }

        public VideoContext(string playerVersion, string sts)
        {
            PlayerVersion = playerVersion;
            Sts = sts;
        }
    }
}
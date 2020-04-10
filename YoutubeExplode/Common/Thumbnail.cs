namespace YoutubeExplode.Common
{
    public class Thumbnail
    {
        public string Url { get; }

        public int Width { get; }

        public int Height { get; }

        public Thumbnail(string url, int width, int height)
        {
            Url = url;
            Width = width;
            Height = height;
        }
    }
}
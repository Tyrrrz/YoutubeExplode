namespace YoutubeExplode.Videos.Streams
{
    public interface IAudioStreamInfo : IStreamInfo
    {
        string AudioCodec { get; }
    }
}
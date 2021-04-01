namespace YoutubeExplode.ReverseEngineering
{
    internal interface IStreamInfoProvider
    {
        int GetTag();

        string GetUrl();

        string? TryGetSignature();

        string? TryGetSignatureParameter();

        long? TryGetContentLength();

        long GetBitrate();

        string GetContainer();

        string? TryGetAudioCodec();

        string? TryGetVideoCodec();

        string? TryGetVideoQualityLabel();

        int? TryGetVideoWidth();

        int? TryGetVideoHeight();

        int? TryGetFramerate();
    }
}
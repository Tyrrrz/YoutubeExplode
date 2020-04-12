namespace YoutubeExplode.ReverseEngineering.Responses
{
    internal interface IStreamInfoProvider
    {
        int GetTag();

        string GetUrl();

        string? TryGetSignature();

        string? TryGetSignatureParameter();

        long? TryGetContentLength();

        double GetBitrate();

        string GetContainer();

        string? TryGetAudioCodec();

        string? TryGetVideoCodec();

        string? TryGetVideoQualityLabel();

        int? TryGetVideoWidth();

        int? TryGetVideoHeight();

        int? TryGetFramerate();
    }
}
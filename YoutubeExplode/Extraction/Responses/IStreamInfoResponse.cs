namespace YoutubeExplode.Extraction.Responses
{
    internal interface IStreamInfoResponse
    {
        int? TryGetTag();

        string? TryGetUrl();

        string? TryGetSignature();

        string? TryGetSignatureParameter();

        long? TryGetContentLength();

        long? TryGetBitrate();

        string? TryGetContainer();

        string? TryGetAudioCodec();

        string? TryGetVideoCodec();

        string? TryGetVideoQualityLabel();

        int? TryGetVideoWidth();

        int? TryGetVideoHeight();

        int? TryGetFramerate();
    }
}
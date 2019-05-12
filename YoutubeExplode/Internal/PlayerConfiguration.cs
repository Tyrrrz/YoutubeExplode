using System;

namespace YoutubeExplode.Internal
{
    internal class PlayerConfiguration
    {
        public string PlayerSourceUrl { get; }

        public string DashManifestUrl { get; }

        public string HlsManifestUrl { get; }

        public string MuxedStreamInfosUrlEncoded { get; }

        public string AdaptiveStreamInfosUrlEncoded { get; }

        public DateTimeOffset ValidUntil { get; }

        public PlayerConfiguration(string playerSourceUrl, string dashManifestUrl, string hlsManifestUrl,
            string muxedStreamInfosUrlEncoded, string adaptiveStreamInfosUrlEncoded, DateTimeOffset validUntil)
        {
            PlayerSourceUrl = playerSourceUrl;
            DashManifestUrl = dashManifestUrl;
            HlsManifestUrl = hlsManifestUrl;
            MuxedStreamInfosUrlEncoded = muxedStreamInfosUrlEncoded;
            AdaptiveStreamInfosUrlEncoded = adaptiveStreamInfosUrlEncoded;
            ValidUntil = validUntil;
        }
    }
}
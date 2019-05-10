using System;
using System.Collections.Generic;
using YoutubeExplode.Internal.CipherOperations;

namespace YoutubeExplode.Internal
{
    internal class PlayerConfiguration
    {
        public IReadOnlyList<ICipherOperation> CipherOperations { get; }

        public string DashManifestUrl { get; }

        public string HlsManifestUrl { get; }

        public string MuxedStreamInfosUrlEncoded { get; }

        public string AdaptiveStreamInfosUrlEncoded { get; }

        public DateTimeOffset ValidUntil { get; }
        public PlayerConfiguration(IReadOnlyList<ICipherOperation> cipherOperations, string dashManifestUrl, string hlsManifestUrl,
            string muxedStreamInfosUrlEncoded, string adaptiveStreamInfosUrlEncoded, DateTimeOffset validUntil)
        {
            CipherOperations = cipherOperations;
            DashManifestUrl = dashManifestUrl;
            HlsManifestUrl = hlsManifestUrl;
            MuxedStreamInfosUrlEncoded = muxedStreamInfosUrlEncoded;
            AdaptiveStreamInfosUrlEncoded = adaptiveStreamInfosUrlEncoded;
            ValidUntil = validUntil;
        }
    }
}
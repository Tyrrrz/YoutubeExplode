using System.Collections.Generic;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Internal
{
    internal class VideoInfo
    {
        private readonly string _videoId;
        private readonly IReadOnlyDictionary<string, string> _dic;

        public VideoInfo(string videoId, IReadOnlyDictionary<string, string> dic)
        {
            _videoId = videoId;
            _dic = dic;
        }

        public bool Contains(string key)
        {
            return _dic.ContainsKey(key);
        }

        public string Get(string key)
        {
            if (_dic.TryGetValue(key, out var result))
                return result;

            throw new KeyNotFoundException($"Could not find metadata with key [{key}]");
        }

        public string GetOrDefault(string key)
        {
            return _dic.TryGetValue(key, out var result) ? result : null;
        }

        public void ThrowIfUnavailable()
        {
            if (!_dic.ContainsKey("errorcode")) return;

            var errorCode = _dic.Get("errorcode").ParseInt();
            var errorReason = _dic.Get("reason");
            throw new VideoNotAvailableException(_videoId, errorCode, errorReason);
        }

        public void ThrowIfRequiresPurchase()
        {
            if (_dic.GetOrDefault("requires_purchase") != "1") return;

            var previewVideoId = _dic.Get("ypc_vid");
            throw new VideoRequiresPurchaseException(_videoId, previewVideoId);
        }
    }
}
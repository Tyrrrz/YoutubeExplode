using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal.CipherOperations;
using YoutubeExplode.Models;

namespace YoutubeExplode.Internal
{
    internal static class Parser
    {
        public static VideoContext VideoContextFromHtml(string rawHtml)
        {
            if (rawHtml.IsBlank())
                throw new ArgumentNullException(nameof(rawHtml));

            // Get player version
            string version = Regex.Match(rawHtml, @"<script\s*src=""/yts/jsbin/player-(.*?)/base.js").Groups[1].Value;
            if (version.IsBlank())
                throw new Exception("Could not parse player version");

            // Get sts (wtf is sts?)
            string sts = Regex.Match(rawHtml, @"""sts""\s*:\s*(\d+)").Groups[1].Value;
            if (sts.IsBlank())
                throw new Exception("Could not parse sts");

            // Populate
            var result = new VideoContext();
            result.PlayerVersion = version;
            result.Sts = sts;

            return result;
        }

        public static Dictionary<string, string> DictionaryFromUrlEncoded(string rawUrlEncoded)
        {
            if (rawUrlEncoded.IsBlank())
                throw new ArgumentNullException(nameof(rawUrlEncoded));

            var dic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var keyValuePairsRaw = rawUrlEncoded.Split("&");
            foreach (string keyValuePairRaw in keyValuePairsRaw)
            {
                string keyValuePairRawDecoded = keyValuePairRaw.UrlDecode();
                if (keyValuePairRawDecoded.IsBlank())
                    continue;

                // Look for the equals sign
                int equalsPos = keyValuePairRawDecoded.IndexOf('=');
                if (equalsPos <= 0)
                    continue;

                // Get the key and value
                string key = keyValuePairRawDecoded.Substring(0, equalsPos);
                string value = equalsPos < keyValuePairRawDecoded.Length
                    ? keyValuePairRawDecoded.Substring(equalsPos + 1)
                    : "";

                // Add to dictionary
                dic[key] = value;
            }

            return dic;
        }

        public static IEnumerable<VideoStreamInfo> VideoStreamInfosFromUrlEncoded(string rawUrlEncoded)
        {
            if (rawUrlEncoded.IsBlank())
                throw new ArgumentNullException(nameof(rawUrlEncoded));

            foreach (string streamRaw in rawUrlEncoded.Split(","))
            {
                var dic = DictionaryFromUrlEncoded(streamRaw);

                // Get values
                string url = dic.GetOrDefault("url");
                string sig = dic.GetOrDefault("s");
                bool needsDeciphering = sig.IsNotBlank();
                int itag = dic.GetOrDefault("itag").ParseIntOrDefault();
                int width = (dic.GetOrDefault("size")?.SubstringUntil("x")).ParseIntOrDefault();
                int height = (dic.GetOrDefault("size")?.SubstringAfter("x")).ParseIntOrDefault();
                long bitrate = dic.GetOrDefault("bitrate").ParseLongOrDefault();
                double fps = dic.GetOrDefault("fps").ParseDoubleOrDefault();
                long size = dic.GetOrDefault("clen").ParseLongOrDefault();

                // Populate
                var result = new VideoStreamInfo();
                result.Url = url;
                result.Signature = sig;
                result.NeedsDeciphering = needsDeciphering;
                result.Itag = itag;
                result.Resolution = new VideoStreamResolution(width, height);
                result.Bitrate = bitrate;
                result.Fps = fps;
                result.FileSize = size;

                yield return result;
            }
        }

        public static IEnumerable<VideoStreamInfo> VideoStreamInfosFromMpd(string rawMpd)
        {
            if (rawMpd.IsBlank())
                throw new ArgumentNullException(nameof(rawMpd));

            var root = XElement.Parse(rawMpd);
            var ns = root.Name.Namespace;
            var xStreamInfos = root.Descendants(ns + "Representation");

            if (xStreamInfos == null)
                throw new Exception("Cannot find streams in input MPD");

            foreach (var xStreamInfo in xStreamInfos)
            {
                // Skip partial streams // TODO: add support for partial streams
                string initUrl =
                    xStreamInfo.Descendants(ns + "Initialization").FirstOrDefault()?.Attribute("sourceURL")?.Value;
                if (initUrl.IsNotBlank() && initUrl.ContainsInvariant("sq/"))
                    continue;

                // Get values
                string url = xStreamInfo.Element(ns + "BaseURL")?.Value;
                int itag = (xStreamInfo.Attribute("id")?.Value).ParseIntOrDefault();
                int width = (xStreamInfo.Attribute("width")?.Value).ParseIntOrDefault();
                int height = (xStreamInfo.Attribute("height")?.Value).ParseIntOrDefault();
                long bitrate = (xStreamInfo.Attribute("bandwidth")?.Value).ParseLongOrDefault();
                double fps = (xStreamInfo.Attribute("frameRate")?.Value).ParseDoubleOrDefault();

                // Populate
                var result = new VideoStreamInfo();
                result.Url = url;
                result.Signature = null;
                result.NeedsDeciphering = false;
                result.Itag = itag;
                result.Resolution = new VideoStreamResolution(width, height);
                result.Bitrate = bitrate;
                result.Fps = fps;

                yield return result;
            }
        }

        public static IEnumerable<VideoCaptionTrackInfo> VideoCaptionTrackInfosFromUrlEncoded(string rawUrlEncoded)
        {
            if (rawUrlEncoded.IsBlank())
                throw new ArgumentNullException(nameof(rawUrlEncoded));

            foreach (string captionRaw in rawUrlEncoded.Split(","))
            {
                var dic = DictionaryFromUrlEncoded(captionRaw);

                // Get values
                string url = dic.GetOrDefault("u");
                string lang = dic.GetOrDefault("lc");
                bool isAuto = dic.GetOrDefault("v")?.ContainsInvariant("a.") ?? false;

                // Populate
                var result = new VideoCaptionTrackInfo();
                result.Url = url;
                result.Language = lang;
                result.IsAutoGenerated = isAuto;

                yield return result;
            }
        }

        public static VideoDashManifestInfo VideoDashManifestInfoFromUrl(string rawUrl)
        {
            if (rawUrl.IsBlank())
                throw new ArgumentNullException(nameof(rawUrl));

            // Get values
            string url = rawUrl;
            string sig = Regex.Match(url, @"/s/(.*?)(?:/|$)").Groups[1].Value;
            bool needsDeciphering = sig.IsNotBlank();

            // Populate
            var result = new VideoDashManifestInfo();
            result.Url = url;
            result.Signature = sig;
            result.NeedsDeciphering = needsDeciphering;

            return result;
        }

        public static VideoInfo VideoInfoFromUrlEncoded(string rawUrlEncoded)
        {
            if (rawUrlEncoded.IsBlank())
                throw new ArgumentNullException(nameof(rawUrlEncoded));

            // Get dictionary
            var dic = DictionaryFromUrlEncoded(rawUrlEncoded);

            // Check the status
            string status = dic.GetOrDefault("status");
            string reason = dic.GetOrDefault("reason");
            int errorCode = dic.GetOrDefault("errorcode").ParseIntOrDefault();
            if (status.EqualsInvariant("fail"))
                throw new YoutubeErrorException(errorCode, reason);

            // Get values
            string id = dic.GetOrDefault("video_id");
            string title = dic.GetOrDefault("title");
            string author = dic.GetOrDefault("author");
            var length = TimeSpan.FromSeconds(dic.GetOrDefault("length_seconds").ParseDoubleOrDefault());
            long viewCount = dic.GetOrDefault("view_count").ParseLongOrDefault();
            double averageRating = dic.GetOrDefault("avg_rating").ParseDoubleOrDefault();
            var keywords = dic.GetOrDefault("keywords").Split(",");
            var watermarks = dic.GetOrDefault("watermark").Split(",");
            bool isListed = dic.GetOrDefault("is_listed").ParseIntOrDefault(1) == 1;
            bool isRatingAllowed = dic.GetOrDefault("allow_ratings").ParseIntOrDefault(1) == 1;
            bool isMuted = dic.GetOrDefault("muted").ParseIntOrDefault() == 1;
            bool isEmbeddingAllowed = dic.GetOrDefault("allow_embed").ParseIntOrDefault(1) == 1;

            // Get the embedded stream meta data
            var streams = new List<VideoStreamInfo>();
            string streamsRaw = dic.GetOrDefault("adaptive_fmts");
            if (streamsRaw.IsNotBlank())
                streams.AddRange(VideoStreamInfosFromUrlEncoded(streamsRaw));
            streamsRaw = dic.GetOrDefault("url_encoded_fmt_stream_map");
            if (streamsRaw.IsNotBlank())
                streams.AddRange(VideoStreamInfosFromUrlEncoded(streamsRaw));

            // Get the caption track meta data
            var captions = new List<VideoCaptionTrackInfo>();
            string captionsRaw = dic.GetOrDefault("caption_tracks");
            if (captionsRaw.IsNotBlank())
                captions.AddRange(VideoCaptionTrackInfosFromUrlEncoded(captionsRaw));

            // Dash manifest
            string dashMpdUrl = dic.GetOrDefault("dashmpd");
            var dashManifest = dashMpdUrl.IsNotBlank() ? VideoDashManifestInfoFromUrl(dashMpdUrl) : null;

            // Populate
            var result = new VideoInfo();
            result.Id = id;
            result.Title = title;
            result.Author = author;
            result.Length = length;
            result.ViewCount = viewCount;
            result.AverageRating = averageRating;
            result.Keywords = keywords;
            result.Watermarks = watermarks;
            result.IsListed = isListed;
            result.IsRatingAllowed = isRatingAllowed;
            result.IsMuted = isMuted;
            result.IsEmbeddingAllowed = isEmbeddingAllowed;
            result.Streams = streams.ToArray();
            result.Captions = captions.ToArray();
            result.DashManifest = dashManifest;

            return result;
        }

        public static PlaylistInfo PlaylistInfoFromJson(string rawJson)
        {
            if (rawJson.IsBlank())
                throw new ArgumentNullException(nameof(rawJson));

            // Get video ids
            var videoIdMatches = Regex.Matches(rawJson, @"""video_id""\s*:\s*""(.*?)""").Cast<Match>();
            var videoIds = videoIdMatches
                .Select(m => m.Groups[1].Value)
                .Where(m => m.IsNotBlank())
                .Distinct()
                .ToArray();

            // Populate
            var result = new PlaylistInfo();
            result.VideoIds = videoIds;

            return result;
        }

        public static string FunctionCallFromLineJs(string rawJs)
        {
            if (rawJs.IsBlank())
                throw new ArgumentNullException(nameof(rawJs));

            return Regex.Match(rawJs, @"\w+\.(\w+)\(").Groups[1].Value;
        }

        public static IEnumerable<ICipherOperation> CipherOperationsFromJs(string rawJs)
        {
            // Original code credit: Decipherer class of https://github.com/flagbug/YoutubeExtractor

            if (rawJs.IsBlank())
                throw new ArgumentNullException(nameof(rawJs));

            // Get the name of the function that handles deciphering
            string funcName = Regex.Match(rawJs, @"\""signature"",\s?([a-zA-Z0-9\$]+)\(").Groups[1].Value;
            if (funcName.IsBlank())
                throw new Exception("Could not find the entry function for signature deciphering");

            // Get the body of the function
            string funcBody = Regex.Match(rawJs, @"(?!h\.)" + Regex.Escape(funcName) + @"=function\(\w+\)\{.*?\}", RegexOptions.Singleline).Value;
            if (funcBody.IsBlank())
                throw new Exception("Could not get the signature decipherer function body");
            var funcLines = funcBody.Split(";").Skip(1).SkipLast(1).ToArray();

            // Identify cipher functions
            string reverseFuncName = null;
            string sliceFuncName = null;
            string charSwapFuncName = null;

            // Analyze the function body to determine the names of cipher functions
            foreach (string line in funcLines)
            {
                // Break when all functions are found
                if (reverseFuncName.IsNotBlank() && sliceFuncName.IsNotBlank() && charSwapFuncName.IsNotBlank())
                    break;

                // Get the function called on this line
                string calledFunctionName = FunctionCallFromLineJs(line);

                // Compose regexes to identify what function we're dealing with
                // -- reverse (0 params)
                var reverseFuncRegex = new Regex($@"{Regex.Escape(calledFunctionName)}:\bfunction\b\(\w+\)");
                // -- slice (1 param)
                var sliceFuncRegex = new Regex($@"{Regex.Escape(calledFunctionName)}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\.");
                // -- swap (1 param)
                var swapFuncRegex = new Regex($@"{Regex.Escape(calledFunctionName)}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b");

                // Determine the function type and assign the name
                if (reverseFuncRegex.Match(rawJs).Success)
                    reverseFuncName = calledFunctionName;
                else if (sliceFuncRegex.Match(rawJs).Success)
                    sliceFuncName = calledFunctionName;
                else if (swapFuncRegex.Match(rawJs).Success)
                    charSwapFuncName = calledFunctionName;
            }

            // Analyze the function body again to determine the operation set and order
            foreach (string line in funcLines)
            {
                // Get the function called on this line
                string calledFunctionName = FunctionCallFromLineJs(line);

                // Swap operation
                if (calledFunctionName == charSwapFuncName)
                {
                    int index = Regex.Match(line, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    yield return new SwapCipherOperation(index);
                }
                // Slice operation
                else if (calledFunctionName == sliceFuncName)
                {
                    int index = Regex.Match(line, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    yield return new SliceCipherOperation(index);
                }
                // Reverse operation
                else if (calledFunctionName == reverseFuncName)
                {
                    yield return new ReverseCipherOperation();
                }
            }
        }

        public static PlayerSource PlayerSourceFromJs(string rawJs)
        {
            if (rawJs.IsBlank())
                throw new ArgumentNullException(nameof(rawJs));

            // Get cipher operations
            var operations = CipherOperationsFromJs(rawJs).ToArray();

            // Populate
            var result = new PlayerSource();
            result.CipherOperations = operations;

            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
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
        // This contains all parsing logic in YoutubeExplode and is kinda messy
        // ...mostly because Youtube's internal API is messy itself

        public static string FunctionCallFromLineJs(string rawJs)
        {
            if (rawJs == null)
                throw new ArgumentNullException(nameof(rawJs));

            return Regex.Match(rawJs, @"\w+\.(\w+)\(").Groups[1].Value;
        }

        public static IEnumerable<ICipherOperation> CipherOperationsFromJs(string rawJs)
        {
            // Original code credit: Decipherer class of https://github.com/flagbug/YoutubeExtractor

            if (rawJs == null)
                throw new ArgumentNullException(nameof(rawJs));

            // Get the name of the function that handles deciphering
            string funcName = Regex.Match(rawJs, @"\""signature"",\s?([a-zA-Z0-9\$]+)\(").Groups[1].Value;
            if (funcName.IsBlank())
                throw new ParserException("Could not find the entry function for signature deciphering");

            // Get the body of the function
            string funcBody = Regex.Match(rawJs, @"(?!h\.)" + Regex.Escape(funcName) + @"=function\(\w+\)\{.*?\}", RegexOptions.Singleline).Value;
            if (funcBody.IsBlank())
                throw new ParserException("Could not get the signature decipherer function body");
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
            if (rawJs == null)
                throw new ArgumentNullException(nameof(rawJs));

            // Get cipher operations
            var operations = CipherOperationsFromJs(rawJs).ToArray();

            // Populate
            var result = new PlayerSource();
            result.CipherOperations = operations;

            return result;
        }

        public static VideoContext VideoContextFromHtml(string rawHtml)
        {
            if (rawHtml == null)
                throw new ArgumentNullException(nameof(rawHtml));

            // Get player version
            string version = Regex.Match(rawHtml, @"<script\s*src=""/yts/jsbin/player-(.*?)/base.js").Groups[1].Value;
            if (version.IsBlank())
                throw new ParserException("Could not parse player version");

            // Get sts (wtf is sts?)
            string sts = Regex.Match(rawHtml, @"""sts""\s*:\s*(\d+)").Groups[1].Value;
            if (sts.IsBlank())
                throw new ParserException("Could not parse sts");

            // Populate
            var result = new VideoContext();
            result.PlayerVersion = version;
            result.Sts = sts;

            return result;
        }

        public static Dictionary<string, string> DictionaryFromUrlEncoded(string rawUrlEncoded)
        {
            if (rawUrlEncoded == null)
                throw new ArgumentNullException(nameof(rawUrlEncoded));

            var dic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var keyValuePairsRaw = rawUrlEncoded.Split("&");
            foreach (string keyValuePairRaw in keyValuePairsRaw)
            {
                string keyValuePairRawDecoded = keyValuePairRaw.UrlDecode();

                // Look for the equals sign
                int equalsPos = keyValuePairRawDecoded.IndexOf('=');
                if (equalsPos <= 0)
                    continue;

                // Get the key and value
                string key = keyValuePairRawDecoded.Substring(0, equalsPos);
                string value = equalsPos < keyValuePairRawDecoded.Length
                    ? keyValuePairRawDecoded.Substring(equalsPos + 1)
                    : string.Empty;

                // Add to dictionary
                dic[key] = value;
            }

            return dic;
        }

        public static IEnumerable<MediaStreamInfo> MediaStreamInfosFromUrlEncoded(string rawUrlEncoded)
        {
            if (rawUrlEncoded == null)
                throw new ArgumentNullException(nameof(rawUrlEncoded));

            foreach (string streamRaw in rawUrlEncoded.Split(","))
            {
                var dic = DictionaryFromUrlEncoded(streamRaw);

                // Get metadata
                string url = dic.GetOrDefault("url");
                string sig = dic.GetOrDefault("s");
                bool needsDeciphering = sig.IsNotBlank();
                int itag = dic.GetOrDefault("itag").ParseIntOrDefault();
                long bitrate = dic.GetOrDefault("bitrate").ParseLongOrDefault();
                double fps = dic.GetOrDefault("fps").ParseDoubleOrDefault();
                long size = dic.GetOrDefault("clen").ParseLongOrDefault();

                // Get resolution
                int width = (dic.GetOrDefault("size")?.SubstringUntil("x")).ParseIntOrDefault();
                int height = (dic.GetOrDefault("size")?.SubstringAfter("x")).ParseIntOrDefault();
                var resolution = new Resolution(width, height);

                // Populate
                var result = new MediaStreamInfo();
                result.Url = url;
                result.Signature = sig;
                result.NeedsDeciphering = needsDeciphering;
                result.Itag = itag;
                result.Resolution = resolution;
                result.Bitrate = bitrate;
                result.Fps = fps;
                result.FileSize = size;

                yield return result;
            }
        }

        public static IEnumerable<MediaStreamInfo> MediaStreamInfosFromXml(string rawXml)
        {
            if (rawXml == null)
                throw new ArgumentNullException(nameof(rawXml));

            var root = XElement.Parse(rawXml).StripNamespaces();
            var xStreamInfos = root.Descendants("Representation");

            foreach (var xStreamInfo in xStreamInfos)
            {
                // Skip partial streams
                // TODO: maybe add support for partial streams
                string initUrl = xStreamInfo.Descendant("Initialization")?.Attribute("sourceURL")?.Value;
                if (initUrl.IsNotBlank() && initUrl.ContainsInvariant("sq/"))
                    continue;

                // Get base URL node
                var xBaseUrl = xStreamInfo.Element("BaseURL");

                // Get metadata
                string url = xBaseUrl?.Value;
                int itag = (xStreamInfo.Attribute("id")?.Value).ParseIntOrDefault();
                long bitrate = (xStreamInfo.Attribute("bandwidth")?.Value).ParseLongOrDefault();
                double fps = (xStreamInfo.Attribute("frameRate")?.Value).ParseDoubleOrDefault();
                long size = (xBaseUrl?.Attribute("contentLength")?.Value).ParseLongOrDefault();

                // Get resolution
                int width = (xStreamInfo.Attribute("width")?.Value).ParseIntOrDefault();
                int height = (xStreamInfo.Attribute("height")?.Value).ParseIntOrDefault();
                var resolution = new Resolution(width, height);

                // Populate
                var result = new MediaStreamInfo();
                result.Url = url;
                result.Signature = null;
                result.NeedsDeciphering = false;
                result.Itag = itag;
                result.Resolution = resolution;
                result.Bitrate = bitrate;
                result.Fps = fps;
                result.FileSize = size;

                yield return result;
            }
        }

        public static IEnumerable<ClosedCaptionTrackInfo> ClosedCaptionTrackInfosFromUrlEncoded(string rawUrlEncoded)
        {
            if (rawUrlEncoded == null)
                throw new ArgumentNullException(nameof(rawUrlEncoded));

            foreach (string captionRaw in rawUrlEncoded.Split(","))
            {
                var dic = DictionaryFromUrlEncoded(captionRaw);

                // Get metadata
                string url = dic.GetOrDefault("u");
                string lang = dic.GetOrDefault("lc");
                bool isAuto = dic.GetOrDefault("v")?.ContainsInvariant("a.") ?? false;

                // HACK: Google uses wrong code for Hebrew
                if (lang == "iw") lang = "he";

                // Create culture info
                var culture = lang.IsNotBlank() ? new CultureInfo(lang) : null;

                // Populate
                var result = new ClosedCaptionTrackInfo();
                result.Url = url;
                result.Culture = culture;
                result.IsAutoGenerated = isAuto;
                yield return result;
            }
        }

        public static DashManifestInfo DashManifestInfoFromUrl(string rawUrl)
        {
            if (rawUrl == null)
                throw new ArgumentNullException(nameof(rawUrl));

            // Get metadata
            string url = rawUrl;
            string sig = Regex.Match(url, @"/s/(.*?)(?:/|$)").Groups[1].Value;
            bool needsDeciphering = sig.IsNotBlank();

            // Populate
            var result = new DashManifestInfo();
            result.Url = url;
            result.Signature = sig;
            result.NeedsDeciphering = needsDeciphering;

            return result;
        }

        public static VideoInfo VideoInfoFromUrlEncoded(string rawUrlEncoded)
        {
            if (rawUrlEncoded == null)
                throw new ArgumentNullException(nameof(rawUrlEncoded));

            // Get dictionary
            var dic = DictionaryFromUrlEncoded(rawUrlEncoded);

            // Check the status
            string status = dic.GetOrDefault("status");
            if (status.EqualsInvariant("fail"))
            {
                string reason = dic.GetOrDefault("reason");
                int errorCode = dic.GetOrDefault("errorcode").ParseIntOrDefault();
                throw new YoutubeErrorException(errorCode, reason);
            }

            // Get metadata
            string id = dic.GetOrDefault("video_id");
            string title = dic.GetOrDefault("title");
            var length = TimeSpan.FromSeconds(dic.GetOrDefault("length_seconds").ParseDoubleOrDefault());
            long viewCount = dic.GetOrDefault("view_count").ParseLongOrDefault();
            var keywords = dic.GetOrDefault("keywords")?.Split(",") ?? new string[0];
            var watermarks = dic.GetOrDefault("watermark")?.Split(",") ?? new string[0];
            bool isListed = dic.GetOrDefault("is_listed").ParseIntOrDefault(1) == 1;
            bool isRatingAllowed = dic.GetOrDefault("allow_ratings").ParseIntOrDefault(1) == 1;
            bool isMuted = dic.GetOrDefault("muted").ParseIntOrDefault() == 1;
            bool isEmbeddingAllowed = dic.GetOrDefault("allow_embed").ParseIntOrDefault(1) == 1;

            // Get adaptive streams
            string adaptiveStreamsRaw = dic.GetOrDefault("adaptive_fmts");
            var adaptiveStreams = adaptiveStreamsRaw.IsNotBlank()
                ? MediaStreamInfosFromUrlEncoded(adaptiveStreamsRaw).ToArray()
                : new MediaStreamInfo[0];

            // Get mixed streams
            string mixedStreamsRaw = dic.GetOrDefault("url_encoded_fmt_stream_map");
            var mixedStreams = mixedStreamsRaw.IsNotBlank()
                ? MediaStreamInfosFromUrlEncoded(mixedStreamsRaw).ToArray()
                : new MediaStreamInfo[0];

            // Get the caption tracks
            string captionTracksRaw = dic.GetOrDefault("caption_tracks");
            var captionTracks = captionTracksRaw.IsNotBlank()
                ? ClosedCaptionTrackInfosFromUrlEncoded(captionTracksRaw).ToArray()
                : new ClosedCaptionTrackInfo[0];

            // Dash manifest
            string dashMpdUrl = dic.GetOrDefault("dashmpd");
            var dashManifest = dashMpdUrl.IsNotBlank()
                ? DashManifestInfoFromUrl(dashMpdUrl)
                : null;

            // Populate
            var result = new VideoInfo();
            result.Id = id;
            result.Title = title;
            result.Length = length;
            result.ViewCount = viewCount;
            result.Keywords = keywords;
            result.Watermarks = watermarks;
            result.IsListed = isListed;
            result.IsRatingAllowed = isRatingAllowed;
            result.IsMuted = isMuted;
            result.IsEmbeddingAllowed = isEmbeddingAllowed;
            result.Streams = adaptiveStreams.Concat(mixedStreams).ToArray();
            result.ClosedCaptionTracks = captionTracks;
            result.DashManifest = dashManifest;

            return result;
        }

        public static VideoInfo VideoInfoFromXml(string rawXml)
        {
            if (rawXml == null)
                throw new ArgumentNullException(nameof(rawXml));

            var root = XElement.Parse(rawXml).StripNamespaces();

            // Get nodes
            var xHtmlContent = root.Element("html_content");
            var xVideoInfo = xHtmlContent?.Element("video_info");
            var xUserInfo = xHtmlContent?.Element("user_info");

            // Get metadata
            string description = xVideoInfo?.Element("description")?.Value;
            long likeCount = (xVideoInfo?.Element("likes_count_unformatted")?.Value).ParseLongOrDefault();
            long dislikeCount = (xVideoInfo?.Element("dislikes_count_unformatted")?.Value).ParseLongOrDefault();

            // Get author
            string authorId = xUserInfo?.Element("channel_external_id")?.Value;
            string authorName = xUserInfo?.Element("username")?.Value;
            string authorDisplayName = xUserInfo?.Element("public_name")?.Value;
            string authorChannelTitle = xUserInfo?.Element("channel_title")?.Value;
            bool authorIsPaid = (xUserInfo?.Element("channel_paid")?.Value).ParseIntOrDefault() == 1;
            var author = new UserInfo();
            author.Id = authorId;
            author.Name = authorName;
            author.DisplayName = authorDisplayName;
            author.ChannelTitle = authorChannelTitle;
            author.IsPaid = authorIsPaid;

            // Populate
            var result = new VideoInfo();
            result.Author = author;
            result.Description = description;
            result.LikeCount = likeCount;
            result.DislikeCount = dislikeCount;

            return result;
        }

        public static PlaylistInfo PlaylistInfoFromXml(string rawXml)
        {
            if (rawXml == null)
                throw new ArgumentNullException(nameof(rawXml));

            var root = XElement.Parse(rawXml).StripNamespaces();

            // Get playlist metadata
            string title = root.Element("title")?.Value;
            string author = root.Element("author")?.Value;
            string description = root.Element("description")?.Value;
            long viewCount = (root.Element("views")?.Value).ParseLongOrDefault();

            // Get video ids
            var ids = root.Descendants("encrypted_id").Select(e => e.Value);

            // Populate
            var result = new PlaylistInfo();
            result.Title = title;
            result.Author = author;
            result.Description = description;
            result.ViewCount = viewCount;
            result.VideoIds = ids.ToArray();

            return result;
        }

        public static IEnumerable<ClosedCaption> ClosedCaptionsFromXml(string rawXml)
        {
            if (rawXml == null)
                throw new ArgumentNullException(nameof(rawXml));

            var root = XElement.Parse(rawXml).StripNamespaces();
            var xTexts = root.Descendants("text");

            foreach (var xText in xTexts)
            {
                // Get metadata
                string text = xText.Value;
                var offset = TimeSpan.FromSeconds((xText.Attribute("start")?.Value).ParseDoubleOrDefault());
                var duration = TimeSpan.FromSeconds((xText.Attribute("dur")?.Value).ParseDoubleOrDefault());

                // Populate
                var result = new ClosedCaption();
                result.Text = text;
                result.Offset = offset;
                result.Duration = duration;

                yield return result;
            }
        }

        public static ClosedCaptionTrack ClosedCaptionTrackFromXml(string rawXml)
        {
            if (rawXml == null)
                throw new ArgumentNullException(nameof(rawXml));

            // Get metadata
            var captions = ClosedCaptionsFromXml(rawXml).ToArray();

            // Populate
            var result = new ClosedCaptionTrack();
            result.Captions = captions;

            return result;
        }
    }
}
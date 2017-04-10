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

        public static IEnumerable<ICipherOperation> CipherOperationsFromJs(string rawJs)
        {
            // Original code credit: Decipherer class of https://github.com/flagbug/YoutubeExtractor

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
                string calledFunctionName = Regex.Match(line, @"\w+\.(\w+)\(").Groups[1].Value;
                if (calledFunctionName.IsBlank())
                    continue;

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
                string calledFunctionName = Regex.Match(line, @"\w+\.(\w+)\(").Groups[1].Value;
                if (calledFunctionName.IsBlank())
                    continue;

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
            // Parse
            var result = new PlayerSource();
            result.CipherOperations = CipherOperationsFromJs(rawJs).ToArray();

            return result;
        }

        public static VideoContext VideoContextFromHtml(string rawHtml)
        {
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
            foreach (string streamRaw in rawUrlEncoded.Split(","))
            {
                var dic = DictionaryFromUrlEncoded(streamRaw);

                // Parse
                var result = new MediaStreamInfo();
                result.Url = dic.GetOrDefault("url");
                result.Itag = dic.GetOrDefault("itag").ParseIntOrDefault();
                result.Signature = dic.GetOrDefault("s");
                result.NeedsDeciphering = result.Signature.IsNotBlank();
                result.Bitrate = dic.GetOrDefault("bitrate").ParseLongOrDefault();
                result.Framerate = dic.GetOrDefault("fps").ParseDoubleOrDefault();
                result.FileSize = dic.GetOrDefault("clen").ParseLongOrDefault();

                // Parse resolution
                int width = (dic.GetOrDefault("size")?.SubstringUntil("x")).ParseIntOrDefault();
                int height = (dic.GetOrDefault("size")?.SubstringAfter("x")).ParseIntOrDefault();
                result.Resolution = width != 0 && height != 0
                    ? new MediaStreamVideoResolution(width, height)
                    : MediaStreamVideoResolution.Empty;

                yield return result;
            }
        }

        public static IEnumerable<MediaStreamInfo> MediaStreamInfosFromXml(string rawXml)
        {
            var xRoot = XElement.Parse(rawXml).StripNamespaces();
            var xStreamInfos = xRoot.Descendants("Representation");

            // Skip partial streams
            // TODO: maybe add support for partial streams
            xStreamInfos = xStreamInfos.Where(x => !(x.Descendant("Initialization")
                                                         ?.Attribute("sourceURL")
                                                         ?.Value.ContainsInvariant("sq/") ?? false));

            foreach (var xStreamInfo in xStreamInfos)
            {
                // Get base URL node
                var xBaseUrl = xStreamInfo.Element("BaseURL");

                // Parse
                var result = new MediaStreamInfo();
                result.Url = xBaseUrl?.Value;
                result.Itag = (xStreamInfo.Attribute("id")?.Value).ParseIntOrDefault();
                result.Bitrate = (xStreamInfo.Attribute("bandwidth")?.Value).ParseLongOrDefault();
                result.Framerate = (xStreamInfo.Attribute("frameRate")?.Value).ParseDoubleOrDefault();
                result.FileSize = (xBaseUrl?.Attribute("contentLength")?.Value).ParseLongOrDefault();

                // Parse resolution
                int width = (xStreamInfo.Attribute("width")?.Value).ParseIntOrDefault();
                int height = (xStreamInfo.Attribute("height")?.Value).ParseIntOrDefault();
                result.Resolution = width != 0 && height != 0
                    ? new MediaStreamVideoResolution(width, height)
                    : MediaStreamVideoResolution.Empty;

                yield return result;
            }
        }

        public static IEnumerable<ClosedCaptionTrackInfo> ClosedCaptionTrackInfosFromUrlEncoded(string rawUrlEncoded)
        {
            foreach (string captionRaw in rawUrlEncoded.Split(","))
            {
                var dic = DictionaryFromUrlEncoded(captionRaw);

                // Parse
                var result = new ClosedCaptionTrackInfo();
                result.Url = dic.GetOrDefault("u");
                result.IsAutoGenerated = dic.GetOrDefault("v")?.ContainsInvariant("a.") ?? false;

                // Parse culture
                string lang = dic.GetOrDefault("lc");
                if (lang == "iw") lang = "he"; // HACK: Google uses wrong code for Hebrew
                result.Culture = new CultureInfo(lang);

                yield return result;
            }
        }

        public static DashManifestInfo DashManifestInfoFromUrl(string rawUrl)
        {
            // Parse
            var result = new DashManifestInfo();
            result.Url = rawUrl;
            result.Signature = Regex.Match(rawUrl, @"/s/(.*?)(?:/|$)").Groups[1].Value;
            result.NeedsDeciphering = result.Signature.IsNotBlank();

            return result;
        }

        public static VideoInfo VideoInfoFromUrlEncoded(string rawUrlEncoded)
        {
            // Get dictionary
            var dic = DictionaryFromUrlEncoded(rawUrlEncoded);

            // Check the status
            string status = dic.GetOrDefault("status");
            if (status.EqualsInvariant("fail"))
            {
                int errorCode = dic.GetOrDefault("errorcode").ParseIntOrDefault();
                string reason = dic.GetOrDefault("reason");
                throw new YoutubeException(errorCode, reason);
            }

            // Parse
            var result = new VideoInfo();
            result.Id = dic.GetOrDefault("video_id");
            result.Title = dic.GetOrDefault("title");
            result.Duration = TimeSpan.FromSeconds(dic.GetOrDefault("length_seconds").ParseDoubleOrDefault());
            result.ViewCount = dic.GetOrDefault("view_count").ParseLongOrDefault();
            result.Keywords = dic.GetOrDefault("keywords")?.Split(",") ?? new string[0];
            result.Watermarks = dic.GetOrDefault("watermark")?.Split(",") ?? new string[0];
            result.IsListed = dic.GetOrDefault("is_listed").ParseIntOrDefault(1) == 1;
            result.IsRatingAllowed = dic.GetOrDefault("allow_ratings").ParseIntOrDefault(1) == 1;
            result.IsMuted = dic.GetOrDefault("muted").ParseIntOrDefault() == 1;
            result.IsEmbeddingAllowed = dic.GetOrDefault("allow_embed").ParseIntOrDefault(1) == 1;

            // Parse streams
            string adaptiveStreamsRaw = dic.GetOrDefault("adaptive_fmts");
            var adaptiveStreams = adaptiveStreamsRaw.IsNotBlank()
                ? MediaStreamInfosFromUrlEncoded(adaptiveStreamsRaw).ToArray()
                : new MediaStreamInfo[0];
            string mixedStreamsRaw = dic.GetOrDefault("url_encoded_fmt_stream_map");
            var mixedStreams = mixedStreamsRaw.IsNotBlank()
                ? MediaStreamInfosFromUrlEncoded(mixedStreamsRaw).ToArray()
                : new MediaStreamInfo[0];
            result.Streams = adaptiveStreams.Concat(mixedStreams).ToArray();

            // Prase closed caption tracks
            string captionTracksRaw = dic.GetOrDefault("caption_tracks");
            var captionTracks = captionTracksRaw.IsNotBlank()
                ? ClosedCaptionTrackInfosFromUrlEncoded(captionTracksRaw).ToArray()
                : new ClosedCaptionTrackInfo[0];
            result.ClosedCaptionTracks = captionTracks;

            // Prase dash manifest
            string dashMpdUrl = dic.GetOrDefault("dashmpd");
            var dashManifest = dashMpdUrl.IsNotBlank()
                ? DashManifestInfoFromUrl(dashMpdUrl)
                : null;
            result.DashManifest = dashManifest;

            return result;
        }

        public static VideoInfo VideoInfoFromXml(string rawXml)
        {
            var xRoot = XElement.Parse(rawXml).StripNamespaces();

            // Get nodes
            var xHtmlContent = xRoot.Element("html_content");
            var xVideoInfo = xHtmlContent?.Element("video_info");
            var xUserInfo = xHtmlContent?.Element("user_info");

            // Parse
            var result = new VideoInfo();
            result.Description = xVideoInfo?.Element("description")?.Value;
            result.LikeCount = (xVideoInfo?.Element("likes_count_unformatted")?.Value).ParseLongOrDefault();
            result.DislikeCount = (xVideoInfo?.Element("dislikes_count_unformatted")?.Value).ParseLongOrDefault();

            // Parse author
            var author = new UserInfo();
            author.Id = xUserInfo?.Element("channel_external_id")?.Value;
            author.Name = xUserInfo?.Element("username")?.Value;
            author.DisplayName = xUserInfo?.Element("public_name")?.Value;
            author.ChannelTitle = xUserInfo?.Element("channel_title")?.Value;
            author.IsPaid = (xUserInfo?.Element("channel_paid")?.Value).ParseIntOrDefault() == 1;
            author.ChannelLogoUrl = xUserInfo?.Element("channel_logo_url")?.Value;
            author.ChannelBannerUrl = xUserInfo?.Element("channel_banner_url")?.Value;
            result.Author = author;

            return result;
        }

        public static PlaylistInfo PlaylistInfoFromXml(string rawXml)
        {
            var xRoot = XElement.Parse(rawXml).StripNamespaces();

            // Parse
            var result = new PlaylistInfo();
            result.Title = xRoot.Element("title")?.Value;
            result.Author = xRoot.Element("author")?.Value;
            result.Description = xRoot.Element("description")?.Value;
            result.ViewCount = (xRoot.Element("views")?.Value).ParseLongOrDefault();
            result.LikeCount = (xRoot.Element("likes")?.Value).ParseLongOrDefault();
            result.DislikeCount = (xRoot.Element("likes")?.Value).ParseLongOrDefault();
            result.VideoIds = xRoot.Descendants("encrypted_id").Select(e => e.Value).ToArray();

            return result;
        }

        public static IEnumerable<ClosedCaption> ClosedCaptionsFromXml(string rawXml)
        {
            var xRoot = XElement.Parse(rawXml).StripNamespaces();
            var xTexts = xRoot.Descendants("text");

            foreach (var xText in xTexts)
            {
                // Parse
                var result = new ClosedCaption();
                result.Text = xText.Value;
                result.Offset = TimeSpan.FromSeconds((xText.Attribute("start")?.Value).ParseDoubleOrDefault());
                result.Duration = TimeSpan.FromSeconds((xText.Attribute("dur")?.Value).ParseDoubleOrDefault());

                yield return result;
            }
        }

        public static ClosedCaptionTrack ClosedCaptionTrackFromXml(string rawXml)
        {
            // Parse
            var result = new ClosedCaptionTrack();
            result.Captions = ClosedCaptionsFromXml(rawXml).ToArray();

            return result;
        }
    }
}
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;
using YoutubeExplode.Videos.Streams.Resolving.Cipher;

namespace YoutubeExplode.Videos.Streams.Resolving
{
    internal class PlayerSourceResolver
    {
        private readonly HttpClient _httpClient;
        private readonly string _url;
        private readonly Cache _cache = new();

        public PlayerSourceResolver(HttpClient httpClient, string url)
        {
            _httpClient = httpClient;
            _url = url;
        }

        private ValueTask<string> GetSourceAsync() =>
            _cache.WrapAsync(async () => await _httpClient.GetStringAsync(_url));

        public ValueTask<string> GetSignatureTimestampAsync() => _cache.WrapAsync(async () =>
        {
            var source = await GetSourceAsync();

            return

                // Current
                source
                    .Pipe(s => Regex.Match(s, @"(?<=invalid namespace.*?;[\w\s]+=)\d+").Value)
                    .NullIfWhiteSpace() ??

                // Legacy
                source
                    .Pipe(s => Regex.Match(s, @"(?<=signatureTimestamp[=\:])\d+").Value)
                    .NullIfWhiteSpace() ??

                throw FatalFailureException.Generic("Could not find signature timestamp in player source.");
        });

        public ValueTask<IReadOnlyList<ICipherOperation>> GetCipherOperationsAsync() => _cache.WrapAsync(async () =>
        {
            var source = await GetSourceAsync();

            var result = new List<ICipherOperation>();

            string? TryGetDeciphererFuncBody()
            {
                var funcName = Regex.Match(source, @"(\w+)=function\(\w+\){(\w+)=\2\.split\(\x22{2}\);.*?return\s+\2\.join\(\x22{2}\)}")
                    .Groups[0]
                    .Value;

                return funcName;
            }

            string? TryGetDeciphererDefinitionBody(string body)
            {
                var objName = Regex.Match(body, "([\\$_\\w]+).\\w+\\(\\w+,\\d+\\);")
                    .Groups[1]
                    .Value;

                var escapedObjName = Regex.Escape(objName);

                return Regex.Match(source, $@"var\s+{escapedObjName}=\{{(\w+:function\(\w+(,\w+)?\)\{{(.*?)\}}),?\}};", RegexOptions.Singleline)
                    .Groups[0]
                    .Value
                    .NullIfWhiteSpace();
            }

            var deciphererFuncBody =
                TryGetDeciphererFuncBody() ??
                throw FatalFailureException.Generic("Could not find signature decipherer function body.");

            var deciphererDefinitionBody =
                TryGetDeciphererDefinitionBody(deciphererFuncBody) ??
                throw FatalFailureException.Generic("Could not find signature decipherer definition body.");

            // Analyze statements to determine cipher function names
            foreach (var statement in deciphererFuncBody.Split(";"))
            {
                // Get the name of the function called in this statement
                var calledFuncName = Regex.Match(statement, @"\w+(?:.|\[)(\""?\w+(?:\"")?)\]?\(").Groups[1].Value;
                if (string.IsNullOrWhiteSpace(calledFuncName))
                {
                    continue;
                }

                // Slice
                if (Regex.IsMatch(deciphererDefinitionBody, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
                {
                    var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    result.Add(new SliceCipherOperation(index));
                }

                // Swap
                else if (Regex.IsMatch(deciphererDefinitionBody, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"))
                {
                    var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    result.Add(new SwapCipherOperation(index));
                }

                // Reverse
                else if (Regex.IsMatch(deciphererDefinitionBody, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\)"))
                {
                    result.Add(new ReverseCipherOperation());
                }
            }

            return (IReadOnlyList<ICipherOperation>) result;
        });
    }
}
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal;
using YoutubeExplode.Internal.Extensions;
using YoutubeExplode.ReverseEngineering.Cipher;

namespace YoutubeExplode.ReverseEngineering.Responses
{
    internal partial class PlayerSource
    {
        private readonly string _root;

        public PlayerSource(string root) => _root = root;

        public string GetSts() =>
            _root
                .Pipe(s => Regex.Match(s, @"(?<=invalid namespace.*?;[\w\s]+=)\d+").Value)
                .NullIfWhiteSpace() ??
            _root
                .Pipe(s => Regex.Match(s, @"(?<=signatureTimestamp[=\:])\d+").Value)
                .NullIfWhiteSpace() ??
            throw FatalFailureException.Generic("Could not find sts in player source.");

        public IEnumerable<ICipherOperation> GetCipherOperations()
        {
            string? TryGetDeciphererFuncBody()
            {
                var funcName = Regex.Match(_root, @"(\w+)=function\(\w+\){(\w+)=\2\.split\(\x22{2}\);.*?return\s+\2\.join\(\x22{2}\)}")
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

                return Regex.Match(_root, $@"var\s+{escapedObjName}=\{{(\w+:function\(\w+(,\w+)?\)\{{(.*?)\}}),?\}};", RegexOptions.Singleline)
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
                    continue;

                // Slice
                if (Regex.IsMatch(deciphererDefinitionBody, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
                {
                    var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    yield return new SliceCipherOperation(index);
                }

                // Swap
                else if (Regex.IsMatch(deciphererDefinitionBody, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"))
                {
                    var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    yield return new SwapCipherOperation(index);
                }

                // Reverse
                else if (Regex.IsMatch(deciphererDefinitionBody, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\)"))
                {
                    yield return new ReverseCipherOperation();
                }
            }
        }
    }

    internal partial class PlayerSource
    {
        public static PlayerSource Parse(string raw) => new(raw);

        public static async Task<PlayerSource> GetAsync(YoutubeHttpClient httpClient, string url) =>
            await Retry.WrapAsync(async () =>
            {
                var raw = await httpClient.GetStringAsync(url);
                return Parse(raw);
            });
    }
}

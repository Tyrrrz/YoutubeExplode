using System.Collections.Generic;
using System.Net.Http;
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

        public PlayerSource(string root)
        {
            _root = root;
        }

        public string GetSts() => _root
            .Pipe(s => Regex.Match(s, @"(?<=invalid namespace.*?;var \w\s*=)\d+").Value)
            .NullIfWhiteSpace() ?? throw new ParsingFailureException("Could not find sts in player source.");

        public IEnumerable<ICipherOperation> GetCipherOperations()
        {
            // Find the name of the function that handles deciphering
            var deciphererFuncName = Regex.Match(_root,
                @"(\w+)=function\(\w+\){(\w+)=\2\.split\(\x22{2}\);.*?return\s+\2\.join\(\x22{2}\)}").Groups[1].Value;

            if (string.IsNullOrWhiteSpace(deciphererFuncName))
            {
                throw new ParsingFailureException(
                    "Could not find signature decipherer function name. Please report this issue on GitHub.");
            }

            // Find the body of the function
            var deciphererFuncBody = Regex.Match(_root,
                @"(?!h\.)" + Regex.Escape(deciphererFuncName) + @"=function\(\w+\)\{(.*?)\}", RegexOptions.Singleline).Groups[1].Value;

            if (string.IsNullOrWhiteSpace(deciphererFuncBody))
            {
                throw new ParsingFailureException(
                    "Could not find signature decipherer function body. Please report this issue on GitHub.");
            }

            // Split the function body into statements
            var deciphererFuncBodyStatements = deciphererFuncBody.Split(";");

            // Find the name of block that defines functions used in decipherer
            var deciphererDefinitionName = Regex.Match(deciphererFuncBody, "(\\w+).\\w+\\(\\w+,\\d+\\);").Groups[1].Value;

            // Find the body of the function
            var deciphererDefinitionBody = Regex.Match(_root,
                @"var\s+" +
                Regex.Escape(deciphererDefinitionName) +
                @"=\{(\w+:function\(\w+(,\w+)?\)\{(.*?)\}),?\};", RegexOptions.Singleline).Groups[0].Value;

            // Identify cipher functions
            var operations = new List<ICipherOperation>();

            // Analyze statements to determine cipher function names
            foreach (var statement in deciphererFuncBodyStatements)
            {
                // Get the name of the function called in this statement
                var calledFuncName = Regex.Match(statement, @"\w+(?:.|\[)(\""?\w+(?:\"")?)\]?\(").Groups[1].Value;
                if (string.IsNullOrWhiteSpace(calledFuncName))
                    continue;

                // Slice
                if (Regex.IsMatch(deciphererDefinitionBody, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
                {
                    var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    operations.Add(new SliceCipherOperation(index));
                }

                // Swap
                else if (Regex.IsMatch(deciphererDefinitionBody, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"))
                {
                    var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    operations.Add(new SwapCipherOperation(index));
                }

                // Reverse
                else if (Regex.IsMatch(deciphererDefinitionBody, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\)"))
                {
                    operations.Add(new ReverseCipherOperation());
                }
            }

            return operations;
        }
    }

    internal partial class PlayerSource
    {
        public static PlayerSource Parse(string raw) => new PlayerSource(raw);

        public static async Task<PlayerSource> GetAsync(HttpClient httpClient, string url) =>
            await Retry.WrapAsync(async () =>
            {
                var raw = await httpClient.GetStringAsync(url);
                return Parse(raw);
            });
    }
}
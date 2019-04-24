using System.Collections.Generic;
using System.Text.RegularExpressions;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal.CipherOperations;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class PlayerSourceParser : Cached
    {
        private readonly string _raw;

        public PlayerSourceParser(string raw)
        {
            _raw = raw;
        }

        public IReadOnlyList<ICipherOperation> GetCipherOperations() => Cache(() =>
        {
            // Find the name of the function that handles deciphering
            var deciphererFuncName = Regex.Match(_raw,
                @"\bc\s*&&\s*d\.set\([^,]+,\s*(?:encodeURIComponent\s*\()?\s*([\w$]+)\(").Groups[1].Value;

            if (deciphererFuncName.IsNullOrWhiteSpace())
                throw new ParserException("Could not find signature decipherer function name.");

            // Find the body of the function
            var deciphererFuncBody = Regex.Match(_raw,
                @"(?!h\.)" + Regex.Escape(deciphererFuncName) + @"=function\(\w+\)\{(.*?)\}", RegexOptions.Singleline).Groups[1].Value;

            if (deciphererFuncBody.IsNullOrWhiteSpace())
                throw new ParserException("Could not find signature decipherer function body.");

            // Split the function body into statements
            var deciphererFuncBodyStatements = deciphererFuncBody.Split(";");

            // Identify cipher functions
            var operations = new List<ICipherOperation>();
            var reverseFuncName = "";
            var sliceFuncName = "";
            var swapFuncName = "";

            // Analyze statements to determine cipher function names
            foreach (var statement in deciphererFuncBodyStatements)
            {
                // Break when all functions are found
                if (!reverseFuncName.IsNullOrWhiteSpace() &&
                    !sliceFuncName.IsNullOrWhiteSpace() &&
                    !swapFuncName.IsNullOrWhiteSpace())
                    break;

                // Get the name of the function called in this statement
                var calledFuncName = Regex.Match(statement, @"\w+(?:.|\[)(\""?\w+(?:\"")?)\]?\(").Groups[1].Value;
                if (calledFuncName.IsNullOrWhiteSpace())
                    continue;

                // Determine cipher function names by signature
                if (Regex.IsMatch(_raw, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\)"))
                {
                    reverseFuncName = calledFuncName;
                }
                else if (Regex.IsMatch(_raw, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
                {
                    sliceFuncName = calledFuncName;
                }
                else if (Regex.IsMatch(_raw, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"))
                {
                    swapFuncName = calledFuncName;
                }
            }

            // Analyze cipher function calls to determine their order and parameters
            foreach (var statement in deciphererFuncBodyStatements)
            {
                // Get the name of the function called in this statement
                var calledFuncName = Regex.Match(statement, @"\w+(?:.|\[)(\""?\w+(?:\"")?)\]?\(").Groups[1].Value;
                if (calledFuncName.IsNullOrWhiteSpace())
                    continue;

                // Reverse operation
                if (calledFuncName == reverseFuncName)
                {
                    operations.Add(new ReverseCipherOperation());
                }
                // Slice operation
                else if (calledFuncName == sliceFuncName)
                {
                    var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    operations.Add(new SliceCipherOperation(index));
                }
                // Swap operation
                else if (calledFuncName == swapFuncName)
                {
                    var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    operations.Add(new SwapCipherOperation(index));
                }
            }

            return operations;
        });
    }

    internal partial class PlayerSourceParser
    {
        public static PlayerSourceParser Initialize(string raw) => new PlayerSourceParser(raw);
    }
}
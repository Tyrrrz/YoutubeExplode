using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Internal.CipherOperations;

namespace YoutubeExplode.Internal.Parsers
{
    internal partial class PlayerSourceParser
    {
        private readonly string _raw;

        public PlayerSourceParser(string raw)
        {
            _raw = raw;
        }

        public IReadOnlyList<ICipherOperation> ParseCipherOperations()
        {
            // Original code credit:
            // https://github.com/flagbug/YoutubeExtractor/blob/3106efa1063994fd19c0e967793315f6962b2d3c/YoutubeExtractor/YoutubeExtractor/Decipherer.cs
            // No copyright, MIT license
            // Regexes found in this method have been sourced by contributors and from other projects

            // Find the name of the function that handles deciphering
            var entryPoint = Regex.Match(_raw, @"\bc\s*&&\s*d\.set\([^,]+\s*,\s*\([^)]*\)\s*\(\s*([a-zA-Z0-9$]+)\(").Groups[1].Value;
            if (entryPoint.IsBlank())
                throw new ParseException("Could not find the entry function for signature deciphering.");

            // Find the body of the function
            var entryPointPattern = @"(?!h\.)" + Regex.Escape(entryPoint) + @"=function\(\w+\)\{(.*?)\}";
            var entryPointBody = Regex.Match(_raw, entryPointPattern, RegexOptions.Singleline).Groups[1].Value;
            if (entryPointBody.IsBlank())
                throw new ParseException("Could not find the signature decipherer function body.");
            var entryPointLines = entryPointBody.Split(";").ToArray();

            // Identify cipher functions
            string reverseFuncName = null;
            string sliceFuncName = null;
            string charSwapFuncName = null;
            var operations = new List<ICipherOperation>();

            // Analyze the function body to determine the names of cipher functions
            foreach (var line in entryPointLines)
            {
                // Break when all functions are found
                if (reverseFuncName.IsNotBlank() && sliceFuncName.IsNotBlank() && charSwapFuncName.IsNotBlank())
                    break;

                // Get the function called on this line
                var calledFuncName = Regex.Match(line, @"\w+(?:.|\[)(\""?\w+(?:\"")?)\]?\(").Groups[1].Value;
                if (calledFuncName.IsBlank())
                    continue;

                // Find cipher function names
                if (Regex.IsMatch(_raw, $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\)"))
                {
                    reverseFuncName = calledFuncName;
                }
                else if (Regex.IsMatch(_raw,
                    $@"{Regex.Escape(calledFuncName)}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
                {
                    sliceFuncName = calledFuncName;
                }
                else if (Regex.IsMatch(_raw,
                    $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"))
                {
                    charSwapFuncName = calledFuncName;
                }
            }

            // Analyze the function body again to determine the operation set and order
            foreach (var line in entryPointLines)
            {
                // Get the function called on this line
                var calledFuncName = Regex.Match(line, @"\w+(?:.|\[)(\""?\w+(?:\"")?)\]?\(").Groups[1].Value;
                if (calledFuncName.IsBlank())
                    continue;

                // Swap operation
                if (calledFuncName == charSwapFuncName)
                {
                    var index = Regex.Match(line, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    operations.Add(new SwapCipherOperation(index));
                }
                // Slice operation
                else if (calledFuncName == sliceFuncName)
                {
                    var index = Regex.Match(line, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    operations.Add(new SliceCipherOperation(index));
                }
                // Reverse operation
                else if (calledFuncName == reverseFuncName)
                {
                    operations.Add(new ReverseCipherOperation());
                }
            }

            return operations;
        }
    }

    internal partial class PlayerSourceParser
    {
        public static PlayerSourceParser Initialize(string raw)
        {
            return new PlayerSourceParser(raw);
        }
    }
}
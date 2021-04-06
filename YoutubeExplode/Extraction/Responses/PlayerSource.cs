using System.Collections.Generic;
using System.Text.RegularExpressions;
using YoutubeExplode.Extraction.Responses.Signature;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Extraction.Responses
{
    internal class PlayerSource
    {
        private readonly string _root;
        private readonly Memo _memo = new();

        public PlayerSource(string root) => _root = root;

        public string? TryGetSignatureTimestamp() => _memo.Wrap(() =>
            _root
                .Pipe(s => Regex.Match(s, @"(?<=invalid namespace.*?;[\w\s]+=)\d+").Value)
                .NullIfWhiteSpace() ??

            _root
                .Pipe(s => Regex.Match(s, @"(?<=signatureTimestamp[=\:])\d+").Value)
                .NullIfWhiteSpace()
        );

        private string? TryGetScramblerBody() => _memo.Wrap(() =>
            Regex.Match(_root,
                    @"(\w+)=function\(\w+\){(\w+)=\2\.split\(\x22{2}\);.*?return\s+\2\.join\(\x22{2}\)}")
                .Groups[0]
                .Value
        );

        private string? TryGetScramblerDefinition() => _memo.Wrap(() =>
        {
            var body = TryGetScramblerBody();
            if (string.IsNullOrWhiteSpace(body))
                return null;

            var objName = Regex.Match(body, "([\\$_\\w]+).\\w+\\(\\w+,\\d+\\);")
                .Groups[1]
                .Value;

            if (string.IsNullOrWhiteSpace(objName))
                return null;

            var escapedObjName = Regex.Escape(objName);

            return Regex.Match(_root, $@"var\s+{escapedObjName}=\{{(\w+:function\(\w+(,\w+)?\)\{{(.*?)\}}),?\}};",
                    RegexOptions.Singleline)
                .Groups[0]
                .Value
                .NullIfWhiteSpace();
        });

        public Scrambler? TryGetSignatureScrambler() => _memo.Wrap(() =>
        {
            var scramblerBody = TryGetScramblerBody();
            if (string.IsNullOrWhiteSpace(scramblerBody))
                return null;

            var scramblerDefinition = TryGetScramblerDefinition();
            if (string.IsNullOrWhiteSpace(scramblerDefinition))
                return null;

            var operations = new List<IScramblerOperation>();

            foreach (var statement in scramblerBody.Split(";"))
            {
                // Get the name of the function called in this statement
                var calledFuncName = Regex.Match(statement, @"\w+(?:.|\[)(\""?\w+(?:\"")?)\]?\(").Groups[1].Value;
                if (string.IsNullOrWhiteSpace(calledFuncName))
                    continue;

                // Slice
                if (Regex.IsMatch(scramblerDefinition,
                    $@"{Regex.Escape(calledFuncName)}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
                {
                    var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    operations.Add(new SliceScramblerOperation(index));
                }

                // Swap
                else if (Regex.IsMatch(scramblerDefinition,
                    $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"))
                {
                    var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                    operations.Add(new SwapScramblerOperation(index));
                }

                // Reverse
                else if (Regex.IsMatch(scramblerDefinition,
                    $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\)"))
                {
                    operations.Add(new ReverseScramblerOperation());
                }
            }

            return new Scrambler(operations);
        });
    }
}

using System.Collections.Generic;
using System.Text.RegularExpressions;
using YoutubeExplode.Bridge.Cipher;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class PlayerSourceExtractor
{
    private readonly string _content;

    public PlayerSourceExtractor(string content) => _content = content;

    public string? TryGetSignatureTimestamp() => Memo.Cache(this, () =>
        Regex.Match(_content, @"(?:signatureTimestamp|sts)\s*:\s*(?<sts>[0-9]{5})")
            .Groups[1]
            .Value
            .NullIfWhiteSpace()
    );

    private string? TryGetCipherBody() => Memo.Cache(this, () =>
        Regex.Match(_content,
                @"(\w+)=function\(\w+\){(\w+)=\2\.split\(\x22{2}\);.*?return\s+\2\.join\(\x22{2}\)}")
            .Groups[0]
            .Value
            .NullIfWhiteSpace()
    );

    private string? TryGetCipherDefinition() => Memo.Cache(this, () =>
    {
        var body = TryGetCipherBody();
        if (string.IsNullOrWhiteSpace(body))
            return null;

        var objName = Regex.Match(body, "([\\$_\\w]+).\\w+\\(\\w+,\\d+\\);")
            .Groups[1]
            .Value;

        if (string.IsNullOrWhiteSpace(objName))
            return null;

        return Regex.Match(_content, $@"var\s+{Regex.Escape(objName)}=\{{(\w+:function\(\w+(,\w+)?\)\{{(.*?)\}}),?\}};",
                RegexOptions.Singleline)
            .Groups[0]
            .Value
            .NullIfWhiteSpace();
    });

    public CipherManifest? TryGetCipherManifest() => Memo.Cache(this, () =>
    {
        var body = TryGetCipherBody();
        if (string.IsNullOrWhiteSpace(body))
            return null;

        var definition = TryGetCipherDefinition();
        if (string.IsNullOrWhiteSpace(definition))
            return null;

        var operations = new List<ICipherOperation>();

        foreach (var statement in body.Split(";"))
        {
            // Get the name of the function called in this statement
            var calledFuncName = Regex.Match(statement, @"\w+(?:.|\[)(\""?\w+(?:\"")?)\]?\(").Groups[1].Value;
            if (string.IsNullOrWhiteSpace(calledFuncName))
                continue;

            // Splice
            if (Regex.IsMatch(definition,
                    $@"{Regex.Escape(calledFuncName)}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
            {
                var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                operations.Add(new SpliceCipherOperation(index));
            }

            // Swap
            else if (Regex.IsMatch(definition,
                         $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"))
            {
                var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                operations.Add(new SwapCipherOperation(index));
            }

            // Reverse
            else if (Regex.IsMatch(definition,
                         $@"{Regex.Escape(calledFuncName)}:\bfunction\b\(\w+\)"))
            {
                operations.Add(new ReverseCipherOperation());
            }
        }

        return new CipherManifest(operations);
    });
}

internal partial class PlayerSourceExtractor
{
    public static PlayerSourceExtractor Create(string raw) => new(raw);
}
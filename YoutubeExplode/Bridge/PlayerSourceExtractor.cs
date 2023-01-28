using System;
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
        Regex.Match(_content, @"(?:signatureTimestamp|sts):(\d{5})")
            .Groups[1]
            .Value
            .NullIfWhiteSpace()
    );

    private string? TryGetCipherCallsite() => Memo.Cache(this, () =>
        Regex.Match(
            _content,
            """
            \w+=function\(\w+\){(\w+)=\1\.split\(['"]{2}\);.*?return \1\.join\(['"]{2}\)}
            """,
            RegexOptions.Singleline
        ).Groups[0].Value.NullIfWhiteSpace()
    );

    private string? TryGetCipherDefinition() => Memo.Cache(this, () =>
    {
        var callsite = TryGetCipherCallsite();
        if (string.IsNullOrWhiteSpace(callsite))
            return null;

        var objName = Regex.Match(callsite, @"(\w+)\.\w+\(\w+,\d+\);")
            .Groups[1]
            .Value;

        if (string.IsNullOrWhiteSpace(objName))
            return null;

        return Regex.Match(
                _content,
                $$"""
                var {{Regex.Escape(objName)}}={.*?};
                """,
                RegexOptions.Singleline
            ).Groups[0].Value.NullIfWhiteSpace();
    });

    public CipherManifest? TryGetCipherManifest() => Memo.Cache(this, () =>
    {
        var callsite = TryGetCipherCallsite();
        if (string.IsNullOrWhiteSpace(callsite))
            return null;

        var definition = TryGetCipherDefinition();
        if (string.IsNullOrWhiteSpace(definition))
            return null;

        var swapFuncName = Regex.Match(
            definition,
            @"(\w+):function\(\w+,\w+\){+[^}]*?%[^}]*?}",
            RegexOptions.Singleline
        ).Groups[1].Value.NullIfWhiteSpace();

        var spliceFuncName = Regex.Match(
            definition,
            @"(\w+):function\(\w+,\w+\){+[^}]*?splice[^}]*?}",
            RegexOptions.Singleline
        ).Groups[1].Value.NullIfWhiteSpace();

        var reverseFuncName = Regex.Match(
            definition,
            @"(\w+):function\(\w+\){+[^}]*?reverse[^}]*?}",
            RegexOptions.Singleline
        ).Groups[1].Value.NullIfWhiteSpace();

        var operations = new List<ICipherOperation>();

        foreach (var statement in callsite.Split(";"))
        {
            var calledFuncName = Regex.Match(statement, @"\w+\.(\w+)\(\w+,\d+\)").Groups[1].Value;
            if (string.IsNullOrWhiteSpace(calledFuncName))
                continue;

            if (string.Equals(calledFuncName, swapFuncName, StringComparison.Ordinal))
            {
                var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                operations.Add(new SwapCipherOperation(index));
            }
            else if (string.Equals(calledFuncName, spliceFuncName, StringComparison.Ordinal))
            {
                var index = Regex.Match(statement, @"\(\w+,(\d+)\)").Groups[1].Value.ParseInt();
                operations.Add(new SpliceCipherOperation(index));
            }
            else if (string.Equals(calledFuncName, reverseFuncName, StringComparison.Ordinal))
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
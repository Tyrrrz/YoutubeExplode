using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using YoutubeExplode.Bridge.Cipher;
using YoutubeExplode.Utils;
using YoutubeExplode.Utils.Extensions;

namespace YoutubeExplode.Bridge;

internal partial class PlayerSource
{
    private readonly string _content;

    public string? SignatureTimestamp => Memo.Cache(this, () =>
        Regex.Match(_content, @"(?:signatureTimestamp|sts):(\d{5})")
            .Groups[1]
            .Value
            .NullIfWhiteSpace()
    );

    private string? CipherCallsite => Memo.Cache(this, () =>
        Regex.Match(
            _content,
            """
            \w+=function\(\w+\){(\w+)=\1\.split\(['"]{2}\);.*?return \1\.join\(['"]{2}\)}
            """,
            RegexOptions.Singleline
        ).Groups[0].Value.NullIfWhiteSpace()
    );

    private string? CipherDefinition => Memo.Cache(this, () =>
    {
        if (string.IsNullOrWhiteSpace(CipherCallsite))
            return null;

        var objName = Regex.Match(CipherCallsite, @"(\w+)\.\w+\(\w+,\d+\);")
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

    public CipherManifest? CipherManifest => Memo.Cache(this, () =>
    {
        if (string.IsNullOrWhiteSpace(CipherCallsite))
            return null;

        if (string.IsNullOrWhiteSpace(CipherDefinition))
            return null;

        var swapFuncName = Regex.Match(
            CipherDefinition,
            @"(\w+):function\(\w+,\w+\){+[^}]*?%[^}]*?}",
            RegexOptions.Singleline
        ).Groups[1].Value.NullIfWhiteSpace();

        var spliceFuncName = Regex.Match(
            CipherDefinition,
            @"(\w+):function\(\w+,\w+\){+[^}]*?splice[^}]*?}",
            RegexOptions.Singleline
        ).Groups[1].Value.NullIfWhiteSpace();

        var reverseFuncName = Regex.Match(
            CipherDefinition,
            @"(\w+):function\(\w+\){+[^}]*?reverse[^}]*?}",
            RegexOptions.Singleline
        ).Groups[1].Value.NullIfWhiteSpace();

        var operations = new List<ICipherOperation>();

        foreach (var statement in CipherCallsite.Split(';'))
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

    public PlayerSource(string content) => _content = content;
}

internal partial class PlayerSource
{
    public static PlayerSource Parse(string raw) => new(raw);
}
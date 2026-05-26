using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Lazy;
using PowerKit.Extensions;
using YoutubeExplode.Bridge.Cipher;
using YoutubeExplode.Exceptions;

namespace YoutubeExplode.Bridge;

internal partial class PlayerSource(string content)
{
    [Lazy]
    public CipherManifest CipherManifest
    {
        get
        {
            // Extract the signature timestamp
            var signatureTimestamp = Regex
                .Match(content, @"(?:signatureTimestamp|sts):(\d{5})")
                .Groups[1]
                .Value.NullIfWhiteSpace();

            if (string.IsNullOrWhiteSpace(signatureTimestamp))
                throw new YoutubeExplodeException(
                    "Failed to extract cipher signature timestamp from player source."
                );

            // Find where the player calls the cipher functions
            var cipherCallsite = Regex
                .Match(
                    content,
                    """
                    [$_\w]+=function\([$_\w]+\){([$_\w]+)=\1\.split\(['"]{2}\);.*?return \1\.join\(['"]{2}\)}
                    """,
                    RegexOptions.Singleline
                )
                .Groups[0]
                .Value.NullIfWhiteSpace();

            if (string.IsNullOrWhiteSpace(cipherCallsite))
                throw new YoutubeExplodeException(
                    "Failed to find cipher callsite in player source."
                );

            // Find the object that defines the cipher functions
            var cipherContainerName = Regex
                .Match(cipherCallsite, @"([$_\w]+)\.[$_\w]+\([$_\w]+,\d+\);")
                .Groups[1]
                .Value;

            if (string.IsNullOrWhiteSpace(cipherContainerName))
                throw new YoutubeExplodeException(
                    "Failed to extract cipher container name from player source."
                );

            // Find the definition of the cipher functions
            var cipherDefinition = Regex
                .Match(
                    content,
                    $$"""
                    var {{Regex.Escape(cipherContainerName)}}={.*?};
                    """,
                    RegexOptions.Singleline
                )
                .Groups[0]
                .Value.NullIfWhiteSpace();

            if (string.IsNullOrWhiteSpace(cipherDefinition))
                throw new YoutubeExplodeException(
                    "Failed to find cipher function definitions in player source."
                );

            // Identify the swap cipher function
            var swapFuncName = Regex
                .Match(
                    cipherDefinition,
                    @"([$_\w]+):function\([$_\w]+,[$_\w]+\){+[^}]*?%[^}]*?}",
                    RegexOptions.Singleline
                )
                .Groups[1]
                .Value.NullIfWhiteSpace();

            // Identify the splice cipher function
            var spliceFuncName = Regex
                .Match(
                    cipherDefinition,
                    @"([$_\w]+):function\([$_\w]+,[$_\w]+\){+[^}]*?splice[^}]*?}",
                    RegexOptions.Singleline
                )
                .Groups[1]
                .Value.NullIfWhiteSpace();

            // Identify the reverse cipher function
            var reverseFuncName = Regex
                .Match(
                    cipherDefinition,
                    @"([$_\w]+):function\([$_\w]+\){+[^}]*?reverse[^}]*?}",
                    RegexOptions.Singleline
                )
                .Groups[1]
                .Value.NullIfWhiteSpace();

            var operations = new List<ICipherOperation>();
            foreach (var statement in cipherCallsite.Split(';'))
            {
                var calledFuncName = Regex
                    .Match(statement, @"[$_\w]+\.([$_\w]+)\([$_\w]+,\d+\)")
                    .Groups[1]
                    .Value;

                if (string.IsNullOrWhiteSpace(calledFuncName))
                    continue;

                if (string.Equals(calledFuncName, swapFuncName, StringComparison.Ordinal))
                {
                    var index = Regex
                        .Match(statement, @"\([$_\w]+,(\d+)\)")
                        .Groups[1]
                        .Value.Pipe(s => int.Parse(s, CultureInfo.InvariantCulture));

                    operations.Add(new SwapCipherOperation(index));
                }
                else if (string.Equals(calledFuncName, spliceFuncName, StringComparison.Ordinal))
                {
                    var index = Regex
                        .Match(statement, @"\([$_\w]+,(\d+)\)")
                        .Groups[1]
                        .Value.Pipe(s => int.Parse(s, CultureInfo.InvariantCulture));

                    operations.Add(new SpliceCipherOperation(index));
                }
                else if (string.Equals(calledFuncName, reverseFuncName, StringComparison.Ordinal))
                {
                    operations.Add(new ReverseCipherOperation());
                }
            }

            return new CipherManifest(signatureTimestamp, operations);
        }
    }
}

internal partial class PlayerSource
{
    public static PlayerSource Parse(string raw) => new(raw);
}

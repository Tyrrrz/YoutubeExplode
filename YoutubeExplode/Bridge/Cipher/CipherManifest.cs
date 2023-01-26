using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Bridge.Cipher;

internal partial class CipherManifest
{
    private IReadOnlyList<ICipherOperation> Operations { get; }

    public CipherManifest(IReadOnlyList<ICipherOperation> operations) =>
        Operations = operations;

    public string Decipher(string input) =>
        Operations.Aggregate(input, (acc, op) => op.Decipher(acc));
}

internal partial class CipherManifest
{
    public static CipherManifest Null { get; } = new(Array.Empty<ICipherOperation>());
}
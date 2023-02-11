using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Bridge.Cipher;

internal class CipherManifest
{
    private IReadOnlyList<ICipherOperation> Operations { get; }

    public CipherManifest(IReadOnlyList<ICipherOperation> operations) =>
        Operations = operations;

    public string Decipher(string input) =>
        Operations.Aggregate(input, (acc, op) => op.Decipher(acc));
}
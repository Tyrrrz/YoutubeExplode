using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Bridge.Cipher;

internal class CipherManifest
{
    public string SignatureTimestamp { get; }

    public IReadOnlyList<ICipherOperation> Operations { get; }

    public CipherManifest(string signatureTimestamp, IReadOnlyList<ICipherOperation> operations)
    {
        SignatureTimestamp = signatureTimestamp;
        Operations = operations;
    }

    public string Decipher(string input) =>
        Operations.Aggregate(input, (acc, op) => op.Decipher(acc));
}
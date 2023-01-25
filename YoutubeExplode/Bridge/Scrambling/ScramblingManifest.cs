using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Bridge.Scrambling;

internal partial class ScramblingManifest
{
    private IReadOnlyList<IScramblingOperation> Operations { get; }

    public ScramblingManifest(IReadOnlyList<IScramblingOperation> operations) =>
        Operations = operations;

    public string Unscramble(string input) =>
        Operations.Aggregate(input, (acc, op) => op.Unscramble(acc));
}

internal partial class ScramblingManifest
{
    public static ScramblingManifest Null { get; } = new(Array.Empty<IScramblingOperation>());
}
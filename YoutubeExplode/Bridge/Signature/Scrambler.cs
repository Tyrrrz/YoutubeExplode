using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Bridge.Signature
{
    internal partial class Scrambler
    {
        private IReadOnlyList<IScramblerOperation> Operations { get; }

        public Scrambler(IReadOnlyList<IScramblerOperation> operations) =>
            Operations = operations;

        public string Unscramble(string input) =>
            Operations.Aggregate(input, (acc, op) => op.Unscramble(acc));
    }

    internal partial class Scrambler
    {
        public static Scrambler Null { get; } = new(Array.Empty<IScramblerOperation>());
    }
}
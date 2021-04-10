using System;
using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Bridge.SignatureScrambling
{
    internal partial class SignatureScrambler
    {
        private IReadOnlyList<IScramblerOperation> Operations { get; }

        public SignatureScrambler(IReadOnlyList<IScramblerOperation> operations) =>
            Operations = operations;

        public string Unscramble(string input) =>
            Operations.Aggregate(input, (acc, op) => op.Unscramble(acc));
    }

    internal partial class SignatureScrambler
    {
        public static SignatureScrambler Null { get; } = new(Array.Empty<IScramblerOperation>());
    }
}
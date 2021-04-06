using System.Collections.Generic;
using System.Linq;

namespace YoutubeExplode.Extraction.Responses.Signature
{
    internal class Scrambler
    {
        private IReadOnlyList<IScramblerOperation> Operations { get; }

        public Scrambler(IReadOnlyList<IScramblerOperation> operations) =>
            Operations = operations;

        public string Unscramble(string input) =>
            Operations.Aggregate(input, (acc, op) => op.Unscramble(acc));
    }
}
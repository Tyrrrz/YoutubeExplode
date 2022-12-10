using System.Diagnostics.CodeAnalysis;

namespace YoutubeExplode.Bridge.SignatureScrambling
{
    internal class SliceScramblerOperation : IScramblerOperation
    {
        private readonly int _index;

        public SliceScramblerOperation(int index) => _index = index;

        public string Unscramble(string input) => input[_index..];

        [ExcludeFromCodeCoverage]
        public override string ToString() => $"Slice ({_index})";
    }
}
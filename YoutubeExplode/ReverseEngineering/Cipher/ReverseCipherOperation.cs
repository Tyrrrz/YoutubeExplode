using YoutubeExplode.Internal.Extensions;

namespace YoutubeExplode.ReverseEngineering.Cipher
{
    internal class ReverseCipherOperation : ICipherOperation
    {
        public string Decipher(string input) => input.Reverse();

        public override string ToString() => "Reverse";
    }
}
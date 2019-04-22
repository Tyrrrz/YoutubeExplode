namespace YoutubeExplode.Internal.Abstractions.CipherOperations
{
    internal class ReverseCipherOperation : ICipherOperation
    {
        public string Decipher(string input)
        {
            return input.Reverse();
        }
    }
}
namespace YoutubeExplode.Internal.CipherOperations
{
    internal class ReverseCipherOperation : ICipherOperation
    {
        public string Decipher(string input)
        {
            return input.Reverse();
        }
    }
}
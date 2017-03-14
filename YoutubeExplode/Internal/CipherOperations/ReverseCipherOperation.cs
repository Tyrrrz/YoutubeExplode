using System;

namespace YoutubeExplode.Internal.CipherOperations
{
    internal class ReverseCipherOperation : ICipherOperation
    {
        public string Decipher(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return input.Reverse();
        }
    }
}
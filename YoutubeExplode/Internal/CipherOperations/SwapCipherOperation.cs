using System;
using System.Text;

namespace YoutubeExplode.Internal.CipherOperations
{
    internal class SwapCipherOperation : ICipherOperation
    {
        private readonly int _index;

        public SwapCipherOperation(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Cannot be negative");

            _index = index;
        }

        public string Decipher(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var sb = new StringBuilder(input)
            {
                [0] = input[_index],
                [_index] = input[0]
            };
            return sb.ToString();
        }
    }
}
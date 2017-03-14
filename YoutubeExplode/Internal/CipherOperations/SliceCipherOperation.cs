using System;

namespace YoutubeExplode.Internal.CipherOperations
{
    internal class SliceCipherOperation : ICipherOperation
    {
        private readonly int _index;

        public SliceCipherOperation(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Cannot be negative");

            _index = index;
        }

        public string Decipher(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return input.Substring(_index);
        }
    }
}
using System;
using YoutubeExplode.Internal.CipherOperations;

namespace YoutubeExplode.Internal
{
    internal class PlayerSource
    {
        public string Version { get; internal set; }

        public ICipherOperation[] CipherOperations { get; internal set; }

        public string Decipher(string input)
        {
            if (input.IsBlank())
                throw new ArgumentNullException(input);

            foreach (var operation in CipherOperations)
                input = operation.Decipher(input);
            return input;
        }

        public override string ToString()
        {
            return $"{Version}";
        }
    }
}
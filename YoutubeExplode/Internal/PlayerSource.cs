using System.Collections.Generic;
using System.Linq;
using YoutubeExplode.Internal.CipherOperations;

namespace YoutubeExplode.Internal
{
    internal class PlayerSource
    {
        public string Version { get; }

        public IReadOnlyList<ICipherOperation> CipherOperations { get; }

        public PlayerSource(string version, IEnumerable<ICipherOperation> cipherOperations)
        {
            Version = version;
            CipherOperations = cipherOperations.ToArray();
        }

        public string Decipher(string input)
        {
            foreach (var operation in CipherOperations)
                input = operation.Decipher(input);
            return input;
        }
    }
}
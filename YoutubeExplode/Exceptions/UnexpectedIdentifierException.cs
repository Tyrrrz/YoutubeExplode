using System;

namespace YoutubeExplode.Exceptions
{
    /// <summary>
    /// Thrown when trying to process an unexpected identifier
    /// </summary>
    public class UnexpectedIdentifierException : Exception
    {
        internal UnexpectedIdentifierException(string message)
            : base(message)
        {
        }
    }
}

using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;
using System.Text.RegularExpressions;

namespace YoutubeExplode.Bridge.SignatureScrambling
{
    internal class NSignatureScrambler
    {
        public string? Unscramble(string jsFunction, string input) => UnscrambleNSignature(jsFunction, input);

        private string? UnscrambleNSignature(string jsFunction, string signature)
        {
            var functionNameRegex = Regex.Match(jsFunction, @"var\s(\w*)=");
            if (!string.IsNullOrWhiteSpace(jsFunction) && functionNameRegex.Success)
            {
                var context = new Context();
                context.Eval(jsFunction);
                return context.GetVariable(functionNameRegex.Groups[1].Value).As<Function>().Call(new Arguments { signature }).Value.ToString();
            }
            return signature;
        }
    }
}

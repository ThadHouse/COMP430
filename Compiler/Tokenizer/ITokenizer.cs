using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer
{
    public interface ITokenizer
    {
        IReadOnlyList<IToken> EnumerateTokens(ReadOnlySpan<char> input);
    }
}

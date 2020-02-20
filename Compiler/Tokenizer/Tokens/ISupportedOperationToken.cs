using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Tokens
{
    public interface ISupportedOperationToken : IToken
    {
        SupportedOperation Operation { get; }
    }
}

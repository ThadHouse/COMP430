using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Tokens
{
    public interface ISupportedOperationToken : ISingleCharToken
    {
        char Operation { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer
{
#pragma warning disable CA1040 // Avoid empty interfaces
    public interface IKeywordToken : IToken
#pragma warning restore CA1040 // Avoid empty interfaces
    {
    }
}

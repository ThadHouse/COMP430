using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Tokens
{
    public class LessThenOrEqualToToken : ISupportedOperationToken, IMultiCharOperationToken
    {
        public SupportedOperation Operation => SupportedOperation.LessThenOrEqualTo;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Tokens
{
    class GreaterThenOrEqualToToken : ISupportedOperationToken, IMultiCharOperationToken
    {
        public SupportedOperation Operation => SupportedOperation.GreaterThenOrEqualTo;
    }
}

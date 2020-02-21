using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Tokens
{
    class NotEqualsToken : ISupportedOperationToken, IMultiCharOperationToken
    {
        public SupportedOperation Operation => SupportedOperation.NotEqual;
    }
}

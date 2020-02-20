using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Tokens
{
    public class StarToken : ISupportedOperationToken, ISingleCharToken
    {
        public const char CharValue = '*';

        public SupportedOperation Operation => SupportedOperation.Multiply;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Tokens
{
    public class MinusToken : ISupportedOperationToken
    {
        public const char CharValue = '-';

        public char Operation => CharValue;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Tokens
{
    public enum SupportedOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Equals,
        GreaterThen,
        GreaterThenOrEqualTo,
        LessThen,
        LessThenOrEqualTo,
        Or,
        And,
        NotEqual
    }
}
